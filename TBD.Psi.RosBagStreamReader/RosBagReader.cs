using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TBD.Psi.RosBagStreamReader
{
    using System.Linq;
    using Deserializers;
    using Microsoft.Psi;
    using TBD.Psi.RosBagStreamReader.Deserializers;

    public class RosBagReader
    {
        private List<FileStream> bagFileStreams = new List<FileStream>();
        private Dictionary<string, TopicInformation> metaInformation = new Dictionary<string, TopicInformation>();
        private List<RosStreamMetaData> streamMetaList = new List<RosStreamMetaData>();
        private Dictionary<string, MsgDeserializer> typeDeserializers = new Dictionary<string, MsgDeserializer>();
        private List<(string name, MsgDeserializer deserializer)> nameDeserializers = new List<(string, MsgDeserializer)>();
        private readonly object streamLock = new object();

        private DateTime bagStartTime;
        private DateTime bagEndTime;

        public RosBagReader()
        {
            this.loadDefaultDeserializers();
        }

        public void Initialize(List<string> paths)
        {
            // we assume the incoming path is already sorted
            this.FirstBagName = System.IO.Path.GetFileName(paths[0]);
            this.BagDirectory = System.IO.Path.GetDirectoryName(paths[0]);

            // Read bags one by one
            // get information about the topics
            foreach (var path in paths)
            {
                // open the file stream & enable it to be shared among threads
                var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                // validate the filestream is a rosbag and we can figure out the version
                Helper.validateRosBag(fileStream);
                // add filestream to list
                this.bagFileStreams.Add(fileStream);
                // get the index
                var bagIndex = this.bagFileStreams.Count - 1;
                // Read the ros bag header information (3.1 in the rosbag format)
                var nextRecordOffset = ReadSingleRosBagHeader(bagIndex, 13);
                // dictionary to help facilitate faster lookup
                var topicMapping = new Dictionary<int, string>();
                // read until we reach end of file stream.
                while (nextRecordOffset < fileStream.Length)
                {
                    // read next record
                    (var headers, var dataOffset, var dataLen) = Helper.ReadNextRecord(this.bagFileStreams[bagIndex], nextRecordOffset);
                    // get the data bytes
                    var dataBytes = new byte[dataLen];
                    this.bagFileStreams[bagIndex].Read(dataBytes, 0, dataLen);

                    switch (headers["op"][0])
                    {
                        case 0x07: // 3.3 Connection record

                            // construct a connection object
                            this.bagFileStreams[bagIndex].Seek(dataOffset, SeekOrigin.Begin);

                            var name = Encoding.UTF8.GetString(headers["topic"]);
                            var connId = BitConverter.ToInt32(headers["conn"], 0);

                            // check if we already know this topic exist
                            if (!this.metaInformation.ContainsKey(name))
                            {
                                this.metaInformation[name] = new TopicInformation(headers, dataBytes);
                            }
                            // Add the connection ID of this topic for this bag into the list
                            if (!this.metaInformation[name].ConnectionIds.ContainsKey(bagIndex))
                            {
                                this.metaInformation[name].ConnectionIds[bagIndex] = new List<int>();
                            }
                            this.metaInformation[name].ConnectionIds[bagIndex].Add(connId);
                            topicMapping[connId] = name;
                            break;
                        case 0x06: // 3.6 Chunk Information

                            // get the number of counts for this chunk
                            var count = BitConverter.ToInt32(headers["count"], 0);

                            // update the start and end time
                            var startTime = Helper.ReadRosBaseType<DateTime>(headers["start_time"]);
                            var endTime = Helper.ReadRosBaseType<DateTime>(headers["end_time"]);

                            // get all the connections and counts
                            for (var i = 0; i < count; i++)
                            {
                                var conn = BitConverter.ToInt32(dataBytes, (i * 8));
                                var msgCount = BitConverter.ToInt32(dataBytes, (i * 8) + 4);

                                // update the count in messages
                                this.metaInformation[topicMapping[conn]].MessageCount += msgCount;

                                // update the time
                                if (this.metaInformation[topicMapping[conn]].StartTime > startTime)
                                {
                                    this.metaInformation[topicMapping[conn]].StartTime = startTime;
                                }
                                if (this.metaInformation[topicMapping[conn]].EndTime < endTime)
                                {
                                    this.metaInformation[topicMapping[conn]].EndTime = endTime;
                                }

                                //update the chunk information
                                if (!this.metaInformation[topicMapping[conn]].ChunkPointerList.ContainsKey(bagIndex))
                                {
                                    this.metaInformation[topicMapping[conn]].ChunkPointerList[bagIndex] = new List<long>();
                                }
                                this.metaInformation[topicMapping[conn]].ChunkPointerList[bagIndex].Add(BitConverter.ToInt64(headers["chunk_pos"], 0));
                            }
                            break;
                    }
                    // progress to next record
                    nextRecordOffset = dataOffset + dataLen;
                }
            }

            // set start and end time
            this.bagStartTime = metaInformation.Values.OrderBy(m => m.StartTime).First().StartTime;
            this.bagEndTime = metaInformation.Values.OrderByDescending(m => m.EndTime).First().EndTime;

            // Now we figure out how to deserialize each message into a format that we can work with.
            int streamIds = 0;
            foreach (var info in metaInformation)
            {
                // see if there is a match in terms of name
                var nameMatches = this.nameDeserializers.Where(p => info.Key.Contains(p.name) && p.deserializer.RosMessageTypeName == info.Value.Type);
                if (nameMatches.Count() > 0)
                {
                    // check for perfect matches
                    if (nameMatches.Where(m => m.name == info.Key).Any())
                    {
                        info.Value.deserializer = nameMatches.Where(m => m.name == info.Key).First().deserializer;
                    }
                    else
                    {
                        info.Value.deserializer = nameMatches.First().deserializer;
                    }
                }
                // see if there is a registered type of converter for this message type.
                else if (this.typeDeserializers.ContainsKey(info.Value.Type))
                {
                    // found a deserializer for this type.
                    info.Value.deserializer = this.typeDeserializers[info.Value.Type];
                }
                else
                {
                    // add generic deserializer
                    info.Value.deserializer = new GenericMsgDeserializer(info.Value, true);

                }

                // create all the values for the stream
                info.Value.sourceId = streamIds;
                var psiStreamMetaData = new RosStreamMetaData(info.Key, streamIds, info.Value.deserializer.AssemblyName,
                    this.FirstBagName, this.BagDirectory, info.Value.StartTime, info.Value.EndTime, 0, info.Value.MessageCount, 0
                )
                {
                    deserializeTypeName = info.Value.deserializer.AssemblyName
                };
                streamMetaList.Add(psiStreamMetaData);
                streamIds++;
            }
        }

        public void AddDeserializer(MsgDeserializer deserializer, string topicName = "")
        {
            if (topicName != "")
            {
                this.nameDeserializers.Add((topicName, deserializer));
            }
            else
            {
                this.typeDeserializers[deserializer.RosMessageTypeName] = deserializer;
            }
        }

        private void loadDefaultDeserializers()
        {
            // load named deserializers
            this.AddDeserializer(new SensorMsgsImageAsDepthImageDeserializer(true), "depth");
            // load generic deserializers
            // std_msgs
            this.AddDeserializer(new StdMsgsDefaultDeserializer<string>("std_msgs/String"));
            this.AddDeserializer(new StdMsgsDefaultDeserializer<bool>("std_msgs/Bool"));
            this.AddDeserializer(new StdMsgsDefaultDeserializer<sbyte>("std_msgs/Byte"));
            this.AddDeserializer(new StdMsgsDefaultDeserializer<byte>("std_msgs/Char"));
            this.AddDeserializer(new StdMsgsDefaultDeserializer<float>("std_msgs/Float32"));
            this.AddDeserializer(new StdMsgsDefaultDeserializer<double>("std_msgs/Float64"));
            this.AddDeserializer(new StdMsgsDefaultDeserializer<sbyte>("std_msgs/Int8"));
            this.AddDeserializer(new StdMsgsDefaultDeserializer<byte>("std_msgs/UInt8"));
            this.AddDeserializer(new StdMsgsDefaultDeserializer<Int16>("std_msgs/Int16"));
            this.AddDeserializer(new StdMsgsDefaultDeserializer<UInt16>("std_msgs/UInt16"));
            this.AddDeserializer(new StdMsgsDefaultDeserializer<Int32>("std_msgs/Int32"));
            this.AddDeserializer(new StdMsgsDefaultDeserializer<UInt32>("std_msgs/UInt32"));
            this.AddDeserializer(new StdMsgsDefaultDeserializer<Int64>("std_msgs/Int64"));
            this.AddDeserializer(new StdMsgsDefaultDeserializer<UInt64>("std_msgs/UInt64"));
            this.AddDeserializer(new StdMsgsDefaultDeserializer<DateTime>("std_msgs/Time"));
            this.AddDeserializer(new StdMsgsDefaultDeserializer<TimeSpan>("std_msgs/Duration"));
            this.AddDeserializer(new StdMsgsColorRGBADeserializer());
            
            // sensor_msgs 
            this.AddDeserializer(new SensorMsgsImageDeserializer(true));
            this.AddDeserializer(new SensorMsgsCompressedImageDeserializer(true));
            this.AddDeserializer(new SensorMsgsJointStateDeserializer(true));
            this.AddDeserializer(new SensorMsgsCameraInfoDeserializer(true));
            this.AddDeserializer(new SensorMsgsPointCloudDeserializer());
            this.AddDeserializer(new SensorMsgsPointCloud2Deserializer());
            this.AddDeserializer(new SensorMsgsPointFieldDeserializer());

            // geometry_msgs
            this.AddDeserializer(new GeometrymsgsPoseStampedDeserializer(false));
            this.AddDeserializer(new GeometrymsgsPointDeserializer());
            this.AddDeserializer(new GeometrymsgsPoint32Deserializer());
            this.AddDeserializer(new GeometrymsgsPoseDeserializer());
            this.AddDeserializer(new GeometrymsgsQuaternionDeserializer());
            this.AddDeserializer(new GeometrymsgsTransformDeserializer());
            this.AddDeserializer(new GeometrymsgsVector3Deserializer());

            // visualization_msgs
            this.AddDeserializer(new VisualizationMsgsMarkerDeserializer());

            // others
            this.AddDeserializer(new AudioCommonMsgsAudioDataDeserializer());
            this.AddDeserializer(new TBDAudioMsgsAudioDataStampedDeserializer(true));
            this.AddDeserializer(new TBDAudioMsgsVADStampedDeserializer(true));
            this.AddDeserializer(new TBDAudioMsgsUtterancedDeserializer(true));
        }

        public IEnumerable<RosStreamMetaData> GetStreamMetaData()
        {
            return this.streamMetaList;
        }

        public string FirstBagName { private set; get; }

        public string BagDirectory { private set; get; }

        public DateTime BagStartTime
        {
            get => this.bagStartTime;
        }

        public DateTime BagEndTime
        {
            get => this.bagEndTime;
        }

        public int? ReadableTopicNum
        {
            get => this.streamMetaList.Count;
        }

        public MsgDeserializer GetDeserializer(string topicName)
        {
            return this.metaInformation[topicName].deserializer;
        }

        public bool Read(int bagIndex, long pointer, int length, out byte[] result)
        {
            // TODO some kind of checking?
            result = new byte[length];
            lock (this.streamLock)
            {
                // move read header to that point
                this.bagFileStreams[bagIndex].Seek(pointer, SeekOrigin.Begin);
                this.bagFileStreams[bagIndex].Read(result, 0, length);
            }
            return true;
        }

        public bool Next(string topicName, out int bagIndex, out long pointer, out int length, out Envelope envelope)
        {
            // get information about this topic
            var topicInfo = this.metaInformation[topicName];
            // no more message to read
            if (topicInfo.readCounter >= topicInfo.MessageCount)
            {
                bagIndex = default;
                pointer = default;
                length = default;
                envelope = default;
                return false;
            }

            // if we finish reading all the chuncks in this bag & the next message is in the next bag
            if (topicInfo.ChunkIndex >= topicInfo.ChunkPointerList[topicInfo.bagIndex].Count)
            {
                topicInfo.ChunkIndex = 0;
                topicInfo.bagIndex++;
            }

            // if we somehow run out of bags, then it is done too. It
            if (topicInfo.bagIndex >= this.bagFileStreams.Count)
            {
                bagIndex = default;
                pointer = default;
                length = default;
                envelope = default;
                return false;
            }

            // read the chunk
            long nextRecordPos;
            long chuckDataOffset;
            long chunkDataLen;
            lock (this.streamLock)
            {
                (_, chuckDataOffset, chunkDataLen) = Helper.ReadNextRecord(this.bagFileStreams[topicInfo.bagIndex], topicInfo.ChunkPointerList[topicInfo.bagIndex][topicInfo.ChunkIndex]);
            }
            // read the info record proceeding the chunk
            nextRecordPos = chuckDataOffset + chunkDataLen;
            long indexDataOffset = -1;
            int indexDataLen = -1;
            Dictionary<string, byte[]> indexHeader;
            int msgCount = -1;
            byte[] indexDataBytes = new byte[12];
            byte[] intBytes = new byte[4];

            do
            {
                lock (this.streamLock)
                {
                    (indexHeader, indexDataOffset, indexDataLen) = Helper.ReadNextRecord(this.bagFileStreams[topicInfo.bagIndex], nextRecordPos);
                }
                if (indexHeader["op"][0] != (byte)0x04)
                {
                    throw new Exception($"Except to see Index data Record (0x04) but got {indexHeader["op"][0]}");
                }
                msgCount = BitConverter.ToInt32(indexHeader["count"], 0);
                nextRecordPos = indexDataOffset + indexDataLen;
            }
            while (!topicInfo.ConnectionIds[topicInfo.bagIndex].Contains(BitConverter.ToInt32(indexHeader["conn"], 0)));

            lock (this.streamLock)
            {
                // seek to the correct part
                this.bagFileStreams[topicInfo.bagIndex].Seek(indexDataOffset + topicInfo.ChunkMsgIndex * 12, SeekOrigin.Begin);
                // from the index, read the time and offset into chunk
                this.bagFileStreams[topicInfo.bagIndex].Read(indexDataBytes, 0, 12);
            }
            // get the time and offset
            var messageTime = Helper.ReadRosBaseType<DateTime>(indexDataBytes);
            // get offset
            var offset = BitConverter.ToInt32(indexDataBytes, 8);

            lock (this.streamLock)
            {
                // now we can look into the chunk record and read data
                this.bagFileStreams[topicInfo.bagIndex].Seek(chuckDataOffset + offset, SeekOrigin.Begin);
                // read header to progress the file stream 
                Helper.ReadRecordHeader(this.bagFileStreams[topicInfo.bagIndex]);
                // get data len
                this.bagFileStreams[topicInfo.bagIndex].Read(intBytes, 0, 4);
                pointer = this.bagFileStreams[topicInfo.bagIndex].Position;
            }

            // set the outgoing variables
            length = BitConverter.ToInt32(intBytes, 0);
            bagIndex = topicInfo.bagIndex;
            envelope = new Envelope(messageTime, messageTime, topicInfo.sourceId, topicInfo.readCounter);

            // update the internal read index
            topicInfo.ChunkMsgIndex++;
            topicInfo.readCounter++;
            if (topicInfo.ChunkMsgIndex >= msgCount)
            {
                topicInfo.ChunkMsgIndex = 0;
                topicInfo.ChunkIndex++;
            }
            return true;

        }


        public void Seek(string topic)
        {
            this.metaInformation[topic].readCounter = 0;
            this.metaInformation[topic].bagIndex = 0;
            this.metaInformation[topic].ChunkIndex = 0;
            this.metaInformation[topic].ChunkMsgIndex = 0;

        }


        private long ReadSingleRosBagHeader(int streamIndex, long offset)
        {
            lock (this.streamLock)
            {
                (var header, _, _) = Helper.ReadNextRecord(this.bagFileStreams[streamIndex], offset);
                return BitConverter.ToInt64(header["index_pos"], 0);
            }
        }
    }
}

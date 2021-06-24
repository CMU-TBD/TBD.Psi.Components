using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TBD.Psi.RosBagStreamReader
{
    using System.Linq;
    using Deserailizers;
    using Microsoft.Psi;
    using TBD.Psi.RosBagStreamReader.Deserializers;

    public class RosBagReader
    {
        private List<FileStream> bagFileStreams = new List<FileStream>();
        private Dictionary<string, TopicInformation> metaInformation = new Dictionary<string, TopicInformation>();
        private List<RosStreamMetaData> streamMetaList = new List<RosStreamMetaData>();
        private Dictionary<string, MsgDeserializer> deserializers = new Dictionary<string, MsgDeserializer>();
        private readonly object streamLock = new object();

        private DateTime bagStartTime;
        private DateTime bagEndTime;

        public RosBagReader(List<string> paths)
        {

            this.FirstBagName = System.IO.Path.GetFileName(paths[0]);
            this.BagDirectory = System.IO.Path.GetDirectoryName(paths[0]);

            this.loadDeserializers();

             
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
                // read until we reach end of file stream
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
                            var startTime = Helper.FromBytesToDateTime(headers["start_time"]);
                            var endTime = Helper.FromBytesToDateTime(headers["end_time"]);

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
                // see if there is a registered type of converter for this message type.
                // the converter takes in bytes[] and return the correct fixed types
                if (this.deserializers.ContainsKey(info.Value.Type))
                {
                    // found a deserializer for this type.
                    info.Value.deserializer = this.deserializers[info.Value.Type];
                    // generate all the information needed by the Psi Store
                    info.Value.sourceId = streamIds;
                    var psiStreamMetaData = new RosStreamMetaData(info.Key, streamIds, info.Value.deserializer.AssemblyName,
                        this.FirstBagName, this.BagDirectory, info.Value.StartTime, info.Value.EndTime,0,info.Value.MessageCount, 0
                    );
                    psiStreamMetaData.deserializeTypeName = info.Value.deserializer.AssemblyName;
                    streamMetaList.Add(psiStreamMetaData);
                    streamIds++;
                }
            }
        }

        private void loadDeserializer(MsgDeserializer deserializer)
        {
            this.deserializers[deserializer.RosMessageTypeName] = deserializer;
        }

        private void loadDeserializers()
        {
            this.loadDeserializer(new StdMsgsStringDeserializer());
            this.loadDeserializer(new StdMsgsBoolDeserializer());
            this.loadDeserializer(new StdMsgsColorRGBADeserializer());
            this.loadDeserializer(new SensorMsgsImageDeserializer(true));
            this.loadDeserializer(new SensorMsgsCompressedImageDeserializer(true));      
            this.loadDeserializer(new SensorMsgsJointStateDeserializer(true));      
            this.loadDeserializer(new AudioCommonMsgsAudioDataDeserializer());
            this.loadDeserializer(new TBDAudioMsgsAudioDataStampedDeserializer(true));
            this.loadDeserializer(new TBDAudioMsgsVADStampedDeserializer(true));
            this.loadDeserializer(new TBDAudioMsgsUtterancedDeserializer(true));
            this.loadDeserializer(new GeometrymsgsPoseStampedDeserializer(false));
            this.loadDeserializer(new GeometrymsgsPointDeserializer());
            this.loadDeserializer(new GeometrymsgsPoseDeserializer());
            this.loadDeserializer(new GeometrymsgsQuaternionDeserializer());
            this.loadDeserializer(new GeometrymsgsTransformDeserializer());
            this.loadDeserializer(new GeometrymsgsVector3Deserializer());
            this.loadDeserializer(new TFMessageDeserializer());
        }

        public IEnumerable<RosStreamMetaData> GetStreamMetaData() {
            return this.streamMetaList;
        }

        public string FirstBagName { private set; get; }

        public string BagDirectory { private set; get; }

        public DateTime BagStartTime {
            get => this.bagStartTime;
        }

        public DateTime BagEndTime {
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
            var messageTime = Helper.FromBytesToDateTime(indexDataBytes, 0);
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

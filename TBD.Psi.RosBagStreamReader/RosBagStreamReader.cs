

namespace TBD.Psi.RosBagStreamReader
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Data;
    using System.IO;

    [StreamReader("ROS Bags", ".bag")]
    public class RosBagStreamReader : IStreamReader
    {
        private RosBagReader bagInterface;
        private Dictionary<string, DateTime> subscribedTopics = new Dictionary<string, DateTime>();
        private Dictionary<string, List<Action<byte[], Envelope>>> outputs = new Dictionary<string, List<Action<byte[], Envelope>>>();
        private Dictionary<string, List<Action<int, long, int, Envelope>>> outputTargets = new Dictionary<string, List<Action<int, long, int, Envelope>>>();

        public RosBagStreamReader(string name, string path)
        {
            // get all bags in that directory
            string searchPattern = "*.bag";
            if (name.Contains('_'))
            {
                searchPattern = name.Split('_')[0] + searchPattern;
            }
            var bagFiles = Directory.GetFiles(path, searchPattern).ToList();
            bagFiles.Sort();

            // Use the interface to the read the bag
            bagInterface = new RosBagReader(bagFiles);

        }

        public RosBagStreamReader(RosBagReader reader)
        {
            this.bagInterface = reader;
        }

        public string Name {
            get => this.bagInterface.FirstBagName;
        }

        public string Path {
            get => this.bagInterface.BagDirectory;
        }

        public IEnumerable<IStreamMetadata> AvailableStreams {
            get => this.bagInterface.GetStreamMetaData();
        }

        public TimeInterval MessageCreationTimeInterval {
            get => new TimeInterval(this.bagInterface.BagStartTime, this.bagInterface.BagEndTime);
        }

        public TimeInterval MessageOriginatingTimeInterval {
            get => new TimeInterval(this.bagInterface.BagStartTime, this.bagInterface.BagEndTime);
        }

        public bool ContainsStream(string name)
        {
            var metaData = this.bagInterface.GetStreamMetaData();
            return metaData.Where(m => m.Name == name).Any();
        }

        public void Dispose()
        {
            // nothing right now
        }

        public IStreamMetadata GetStreamMetadata(string streamName)
        {
            return this.bagInterface.GetStreamMetaData().Where(m => m.Name == streamName).FirstOrDefault();
        }

        public T GetSupplementalMetadata<T>(string streamName)
        {
            // Not sure what we can use this for....
            throw new NotImplementedException();
        }

        public bool IsLive()
        {
            return false;
        }

        public bool MoveNext(out Envelope envelope)
        {
            envelope = default;
            while (!this.subscribedTopics.All(m => m.Value == DateTime.MaxValue))
            {
                // Go through all the topics and find the one with the lowest time
                var earliestTopic = this.subscribedTopics.OrderBy(m => m.Value).First().Key;
                // try reading that data
                if (this.bagInterface.Next(earliestTopic, out var bagIndex, out var pointer, out var length, out envelope))
                {
                    // update the topic times
                    this.subscribedTopics[earliestTopic] = envelope.OriginatingTime;

                    // invoke output
                    this.InvokeOutputs(earliestTopic, bagIndex, pointer, length, envelope);

                    // TODO: This might cause a problem where since we are not sure whether there is still data left here.
                    return true;
                }
                else
                {
                    // We don't have any more data in this topic
                    this.subscribedTopics[earliestTopic] = DateTime.MaxValue;
                }
            }
            return false;
        }

        public IStreamReader OpenNew()
        {
            return new RosBagStreamReader(this.bagInterface);
        }

        private void validateStreamNameAndType<T>(string name)
        {
            // make sure the name exist
            if (!this.bagInterface.GetStreamMetaData().Where(m => m.Name == name).Any())
            {
                throw new ArgumentException($"Topic {name} does not exist");
            }
            // check the type
            if (typeof(T).AssemblyQualifiedName != this.bagInterface.GetStreamMetaData().Where(m => m.Name == name).First().deserializeTypeName)
            {
                throw new ArgumentException($"Topic {name} can only be converted to type {this.bagInterface.GetStreamMetaData().Where(m => m.Name == name).First().deserializeTypeName}");
            }
        }

        private void InvokeOutputs(string topicName, int bagIndex, long position, int length, Envelope envelope)
        {
            // call the standard outputs
            if (this.outputs.ContainsKey(topicName) && this.outputs[topicName].Count > 0)
            {
                this.bagInterface.Read(bagIndex, position, length, out var dataArr);
                foreach (var output in this.outputs[topicName])
                {
                    output(dataArr, envelope);
                }
            }

            if (this.outputTargets.ContainsKey(topicName))
            {
                // call the target outputs
                foreach (var target in this.outputTargets[topicName])
                {
                    target(bagIndex, position, length, envelope);
                }
            }

        }

        public IStreamMetadata OpenStream<T>(string name, Action<T, Envelope> target, Func<T> allocator = null)
        {
            this.validateStreamNameAndType<T>(name);

            var deserializer = this.bagInterface.GetDeserializer(name);
            subscribedTopics.Add(name, DateTime.MinValue);
            // TODO make this a list since there could be multiple subscribers.
            if (! this.outputs.ContainsKey(name))
            {
                this.outputs[name] = new List<Action<byte[], Envelope>>();
            }
            this.outputs[name].Add((b, env) =>
            {
                T output = deserializer.deserialize<T>(b);
                // call action
                target(output, env);
            });
            
                
            return this.bagInterface.GetStreamMetaData().Where(m => m.Name == name).First();
        }

        public T Read<T>(int bagIndex, long pointer, int length, IDeserializer deserializer)
        {
            this.bagInterface.Read(bagIndex, pointer, length, out var dataArr);
            return deserializer.deserialize<T>(dataArr);
        }

        public IStreamMetadata OpenStreamIndex<T>(string name, Action<Func<IStreamReader, T>, Envelope> target)
        {
            this.validateStreamNameAndType<T>(name);

            if (!this.outputTargets.ContainsKey(name))
            {
                this.outputTargets[name] = new List<Action<int, long, int, Envelope>>();
            }
            subscribedTopics.Add(name, DateTime.MinValue);
            var deserializer = this.bagInterface.GetDeserializer(name);
            this.outputTargets[name].Add((bag, pointer, length, envelope) =>
            {
                // return an Index function that actually reads the data when called.
                target(new Func<IStreamReader, T>(reader => ((RosBagStreamReader)reader).Read<T>(bag, pointer, length, deserializer)), envelope);
            });

            return this.bagInterface.GetStreamMetaData().Where(m => m.Name == name).First();
        }

        public void ReadAll(ReplayDescriptor descriptor, CancellationToken cancelationToken = default)
        {

            this.Seek(descriptor.Interval);

            while (!cancelationToken.IsCancellationRequested && !this.subscribedTopics.All(m => m.Value == DateTime.MaxValue))
            {
                // Go through all the topics and find the one with the lowest time
                var earliestTopic = this.subscribedTopics.OrderBy(m => m.Value).First().Key;

                // try reading that data
                if (this.bagInterface.Next(earliestTopic, out var bagIndex, out var pointer, out var length, out var envelope))
                {
                    // update the topic times
                    this.subscribedTopics[earliestTopic] = envelope.OriginatingTime;

                    // if not in the correct time interval ignore
                    if (!this.seekInterval.PointIsWithin(envelope.CreationTime))
                    {
                        continue;
                    }

                    this.InvokeOutputs(earliestTopic, bagIndex, pointer, length, envelope);

                }
                else
                {
                    // We don't have any more data in this topic
                    this.subscribedTopics[earliestTopic] = DateTime.MaxValue;
                }
            }
        }

        private TimeInterval seekInterval = TimeInterval.Infinite;

        public void Seek(TimeInterval interval, bool useOriginatingTime = false)
        {
            // restart all information
            foreach(var topic in this.subscribedTopics.Keys.ToList())
            {
                // set the new time
                this.subscribedTopics[topic] = this.bagInterface.GetStreamMetaData().Where(m => m.Name == topic).First().FirstMessageOriginatingTime;

                // update baginterface
                this.bagInterface.Seek(topic);
            }

            this.seekInterval = interval;
        }
    }
}

namespace TBD.Psi.RosBagStreamReader
{
    using System;
    using Microsoft.Psi;
    public class RosStreamMetaData : StreamMetadataBase
    {
        public RosStreamMetaData(string name, int sourceId, string typeName, string partitionName, string partitionPath, DateTime first, DateTime last, int averageMessageSize, int messageCount, int averageLatency)
                : base(name, sourceId, typeName, partitionName, partitionPath, first, last, messageCount, averageMessageSize, averageLatency)
            {
            this.CreationTimeInterval = new TimeInterval(first, last);
            }

        public string deserializeTypeName;
        public TimeInterval CreationTimeInterval { get; private set; }
    }
}
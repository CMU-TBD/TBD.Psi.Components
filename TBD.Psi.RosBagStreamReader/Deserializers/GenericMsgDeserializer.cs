using Microsoft.Psi;
using System;
using System.Linq;

namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    class GenericMsgDeserializer : MsgDeserializer
    {
        private TopicInformation topicInfo;
        private int headerOffset = -1;
        private bool containHeader = false;
        public GenericMsgDeserializer(TopicInformation topicInfo, bool useHeaderTime)
            : base(typeof(object).AssemblyQualifiedName, "", useHeaderTime)
        {
            this.topicInfo = topicInfo;
            int offset = 0;
            if (useHeaderTime)
            {
                // if this topic has a header
                if (topicInfo.TopicFields.Where(field => field.type == "Header" || field.type == "std_msgs/Header").Any())
                {
                    this.containHeader = true;
                    try
                    {
                        // see if we can get the header time straight from the topicInfo or its dynamic
                        foreach (var fields in topicInfo.TopicFields)
                        {
                            if (fields.type == "Header" || fields.type == "std_msgs/Header")
                            {
                                headerOffset = offset;
                            }
                            offset += Helper.GetRosBaseTypeByteLength(fields.type);
                        }
                    }
                    catch (InvalidCastException)
                    {
                        //this means we cannot figure it out statically and the offset needs to be dynamically calculated.
                    }
                }
            }
        }
        public override T Deserialize<T>(byte[] msgByteArr, ref Envelope env)
        {
            if (this.containHeader && this.useHeaderTimeAsOriginatingTime)
            {
                var offset = -1;
                // get headertime
                if (this.headerOffset != -1)
                {
                    (_, var headerTime, _) = Helper.ReadStdMsgsHeader(msgByteArr, out offset, this.headerOffset);
                    this.UpdateEnvelope(ref env, headerTime);
                }
                else
                {
                    // we will try dynamically figuring out the header
                    // TODO for future.
                }
            }

            return (T) new object();
        }
    }
}

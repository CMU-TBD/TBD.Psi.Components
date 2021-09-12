namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System;
    using System.Text;
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Imaging;
    using System.IO;
    using MathNet.Spatial.Euclidean;
    using MathNet.Numerics.LinearAlgebra;

    public class RosgraphMsgsLogDeserializer : MsgDeserializer
    {
        public RosgraphMsgsLogDeserializer(bool useHeader)
            : base(typeof(string).AssemblyQualifiedName, "rosgraph_msgs/Log", useHeader)
        { 
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // convert texts
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out var offset, 0);
            this.UpdateEnvelope(ref env, originTime);
            var level = Helper.ReadRosBaseType<byte>(data, out offset, offset);
            var nodeName = Helper.ReadRosBaseType<string>(data, out offset, offset);
            var msg = Helper.ReadRosBaseType<string>(data, out offset, offset);
            var file = Helper.ReadRosBaseType<string>(data, out offset, offset);
            var function = Helper.ReadRosBaseType<string>(data, out offset, offset);
            var line = Helper.ReadRosBaseType<uint>(data, out offset, offset);
            var topics = Helper.ReadRosBaseTypeArray<string>(data, out offset, offset);
            // combine for a useful text
            var output = $"[{this.parseLevel(level)}]{nodeName}:{msg}";

            if (this.parseLevel(level) == "WARN")
            {
                return default(T);
            }

            return (T)(object) output;
        }

        private string parseLevel(byte val)
        {
            switch(val)
            {
                case 1:
                    return "DEBUG";
                case 2:
                    return "INFO";
                case 4:
                    return "WARN";
                case 8:
                    return "ERROR";
                case 16:
                    return "FATAL";
            }
            throw new InvalidCastException("Unknown level.");
        }
    }
}

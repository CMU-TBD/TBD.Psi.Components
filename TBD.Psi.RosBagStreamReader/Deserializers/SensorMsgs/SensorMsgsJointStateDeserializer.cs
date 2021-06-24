namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System;
    using System.Text;
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Imaging;
    using System.IO;

    public class SensorMsgsJointStateDeserializer : MsgDeserializer
    {
        public SensorMsgsJointStateDeserializer(bool useHeaderTime)
            : base(typeof((string[] name, double[] position, double[] velocity, double[] effort)).AssemblyQualifiedName, "sensor_msgs/JointState", useHeaderTime)
        { 
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // read the header and get location
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out var offset, 0);
            this.UpdateEnvelope(ref env, originTime);

            string[] names = Helper.ReadRosBaseTypeArray<string>(data, out offset, offset);
            double[] position = Helper.ReadRosBaseTypeArray<double>(data, out offset, offset);
            double[] velocity = Helper.ReadRosBaseTypeArray<double>(data, out offset, offset);
            double[] effort = Helper.ReadRosBaseTypeArray<double>(data, out offset, offset);

            return (T) (object) (names, position, velocity, effort);
        }
    }
}

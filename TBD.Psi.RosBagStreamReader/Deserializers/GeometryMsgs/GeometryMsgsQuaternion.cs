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

    public class GeometrymsgsQuaternionDeserializer : MsgDeserializer
    {
        public GeometrymsgsQuaternionDeserializer()
            : base(typeof(Quaternion).AssemblyQualifiedName, "geometry_msgs/Quaternion")
        { 
        }

        public static Quaternion Deserialize(byte[] data, ref int offset)
        {
            var quaternion_x = Helper.ReadMsgFloat64(data, out offset, offset);
            var quaternion_y = Helper.ReadMsgFloat64(data, out offset, offset);
            var quaternion_z = Helper.ReadMsgFloat64(data, out offset, offset);
            var quaternion_w = Helper.ReadMsgFloat64(data, out offset, offset);
            return new Quaternion(quaternion_w, quaternion_x, quaternion_y, quaternion_z);
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // convert to quaternion
            int offset = 0;
            var quaternion = Deserialize(data, ref offset);

            return (T)(object)quaternion;
        }
    }
}

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

    public class GeometrymsgsVector3Deserializer : MsgDeserializer
    {
        public GeometrymsgsVector3Deserializer()
            : base(typeof(Vector3D).AssemblyQualifiedName, "geometry_msgs/Vector3")
        { 
        }

        public static Vector3D Deserialize(byte[] data, ref int offset)
        {
            var position_x = Helper.ReadMsgFloat64(data, out offset, offset);
            var position_y = Helper.ReadMsgFloat64(data, out offset, offset);
            var position_z = Helper.ReadMsgFloat64(data, out offset, offset);
            return new Vector3D(position_x, position_y, position_z);
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // convert to the vector
            int offset = 0;
            return (T)(object) Deserialize(data, ref offset);
        }
    }
}

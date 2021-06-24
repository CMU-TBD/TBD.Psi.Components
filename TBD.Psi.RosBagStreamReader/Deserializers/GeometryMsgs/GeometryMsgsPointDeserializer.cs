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

    public class GeometrymsgsPointDeserializer : MsgDeserializer
    {
        public GeometrymsgsPointDeserializer()
            : base(typeof(Point3D).AssemblyQualifiedName, "geometry_msgs/Point")
        { 
        }

        public static Point3D Deserialize(byte[] data, ref int offset)
        {
            var position_x = Helper.ReadRosBaseType<double>(data, out offset, offset);
            var position_y = Helper.ReadRosBaseType<double>(data, out offset, offset);
            var position_z = Helper.ReadRosBaseType<double>(data, out offset, offset);
            return new Point3D(position_x, position_y, position_z);
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // convert to coordinate systems
            int offset = 0;
            var cs = Deserialize(data, ref offset);
            return (T)(object)cs;
        }
    }
}

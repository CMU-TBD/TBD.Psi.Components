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

    public class GeometrymsgsTransformStampedDeserializer : MsgDeserializer
    {
        public GeometrymsgsTransformStampedDeserializer(bool useHeaderTime)
            : base(typeof(CoordinateSystem).AssemblyQualifiedName, "geometry_msgs/TransformStamped", useHeaderTime)
        {
        }

        public static (DateTime time, string parent, string child, CoordinateSystem transform) Deserialize(byte[] data, ref int offset)
        {
            // read the header and get location
            (_, var originTime, var parentFrame) = Helper.ReadStdMsgsHeader(data, out offset, offset);
            // get the child
            var childFrame = Helper.ReadMsgString(data, out offset, offset);
            // get the transform
            var transform = GeometrymsgsTransformDeserializer.Deserialize(data, ref offset);

            return (originTime, parentFrame, childFrame, transform);
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // convert the data 
            int offset = 0;
            var info = Deserialize(data, ref offset);

            return (T)(object)info.transform;
        }
    }
}

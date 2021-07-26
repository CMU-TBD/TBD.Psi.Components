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
    using System.Numerics;

    public class VisualizationMsgsMarkerDeserializer : MsgDeserializer
    {
        public VisualizationMsgsMarkerDeserializer()
            : base(typeof(CoordinateSystem).AssemblyQualifiedName, "visualization_msgs/Marker")
        { 
        }

        public static (CoordinateSystem pose, int id) Deserialize(byte[] data, ref int offset)
        {
            (_, _, _) = Helper.ReadStdMsgsHeader(data, out offset, offset);
            _ = Helper.ReadRosBaseType<String>(data, out offset, offset);
            int id = (int)BitConverter.ToInt32(data, offset);
            offset = offset + 4 + 4 + 4;
            CoordinateSystem pose = GeometrymsgsPoseDeserializer.Deserialize(data, ref offset);
            _ = GeometrymsgsVector3Deserializer.Deserialize(data, ref offset);
            offset = offset + 4 + 4 + 4 + 4 + 8 + 1 + 4 + 4 + 4 + 4 + 1;
            return (pose, id);
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // convert to coordinate systems
            int offset = 0;
            var info = Deserialize(data, ref offset);
            return (T)(object)info.pose;
        }
    }
}

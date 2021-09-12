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
            /*  The following deserializer extracts the geometry_msgs/Pose pose and int32 id from
             *  the Marker and returns it in a tuple. The other variables in the message are ignored,
             *  but can be accessed by giving them a variable name.
             */
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out offset, offset);
            _ = Helper.ReadRosBaseType<String>(data, out offset, offset);                       // string ns
            int id = Helper.ReadRosBaseType<Int32>(data, out offset, offset);                   // int32 id
            _ = Helper.ReadRosBaseType<Int32>(data, out offset, offset);                        // int32 type
            _ = Helper.ReadRosBaseType<Int32>(data, out offset, offset);                        // int32 action
            CoordinateSystem pose = GeometrymsgsPoseDeserializer.Deserialize(data, ref offset); // geometry_msgs/Pose pose
            _ = GeometrymsgsVector3Deserializer.Deserialize(data, ref offset);                  // geometry_msgs/Vector3 scale
            _ = StdMsgsColorRGBADeserializer.Deserialize(data, ref offset);                     // std_msgs/ColorRGBA color
            _ = Helper.ReadRosBaseType<DateTime>(data, out offset, offset);                     // duration lifetime
            _ = Helper.ReadRosBaseType<Boolean>(data, out offset, offset);                      // bool frame_locked

            // geometry_msgs/Point[] points; if one wants to use this variable, create an array/list of Point3D and
            // instead of `_` in the for loop, put the array at index `i`.
            int size = Helper.ReadRosBaseType<Int32>(data, out offset, offset);
            for (int i = 0; i < size; i++) {
                _ = GeometrymsgsPointDeserializer.Deserialize(data, ref offset);
            }

            // std_msgs/ColorRGBA[] colors; if one wants to use this variable, create an array/list of double[] and
            // instead of `_` in the for loop, put the array at index `i`.
            size = Helper.ReadRosBaseType<Int32>(data, out offset, offset);
            for (int i = 0; i < size; i++) {
                _ = StdMsgsColorRGBADeserializer.Deserialize(data, ref offset);
            }

            _ = Helper.ReadRosBaseType<String>(data, out offset, offset);                       // string text
            _ = Helper.ReadRosBaseType<String>(data, out offset, offset);                       // string mesh_resource
            _ = Helper.ReadRosBaseType<Boolean>(data, out offset, offset);                      // mesh_use_embedded_materials

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

namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System;
    using Microsoft.Psi;
    using MathNet.Spatial.Euclidean;
    using System.Collections.Generic;

    public class SensorMsgsPointCloudDeserializer : MsgDeserializer
    {

        public SensorMsgsPointCloudDeserializer()
            : base(typeof(List<Point3D>).AssemblyQualifiedName, "sensor_msgs/PointCloud")
        { 
        }

        public static List<Point3D> Deserialize(byte[] data, ref int offset)
        {
            /*  The following deserializer extracts the geometry_msgs/Point32 points variable from
             *  the PointCloud ROS message and returns a list of Point3D. The ChannelFloat32[] channels
             *  variable is ignored but can be readily implemented.
             */
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out offset, offset);

            List<Point3D> points = new List<Point3D>();
            int size = Helper.ReadRosBaseType<Int32>(data, out offset, offset);
            for (int i = 0; i < size; i++) {
                Point3D point = GeometrymsgsPoint32Deserializer.Deserialize(data, ref offset);
                points.Add(point);
            }

            size = Helper.ReadRosBaseType<Int32>(data, out offset, offset);
            for (int i = 0; i < size; i++) {
                _ = Helper.ReadRosBaseType<String>(data, out offset, offset);               // string name
                int values_size = Helper.ReadRosBaseType<Int32>(data, out offset, offset);  // size of float32[] values
                for (int j = 0; j < values_size; j++) {
                    _ = Helper.ReadRosBaseType<float>(data, out offset, offset);            // values[j]
                }
            }
            return points;
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // convert to coordinate systems
            int offset = 0;
            return (T)(object)Deserialize(data, ref offset);
        }
    }
}

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
    using Microsoft.Psi.AzureKinect;
    using System.Diagnostics;
    using Microsoft.Azure.Kinect.BodyTracking;
    using System.Collections.Generic;

    public class SensorMsgsPointCloud2Deserializer : MsgDeserializer
    {

        public SensorMsgsPointCloud2Deserializer()
            : base(typeof(List<Point3D>).AssemblyQualifiedName, "sensor_msgs/PointCloud2")
        { 
        }

        public static List<Point3D> Deserialize(byte[] data, ref int offset)
        {
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out offset, offset);
            //int height = Helper.ReadRosBaseType<Int32>(data, out offset, offset);
            //int width = Helper.ReadRosBaseType<Int32>(data, out offset, offset);
            offset += 4 + 4;
            // PointField[] fields
            //_ = Helper.ReadRosBaseType<Int32>(data, out offset, offset); // size of PointField[]
            offset += 4;
            var x = SensorMsgsPointFieldDeserializer.Deserialize(data, ref offset);
            var y = SensorMsgsPointFieldDeserializer.Deserialize(data, ref offset);
            var z = SensorMsgsPointFieldDeserializer.Deserialize(data, ref offset);

            //_ = Helper.ReadRosBaseType<Boolean>(data, out offset, offset); // is_bigendian
            //_ = Helper.ReadRosBaseType<Int32>(data, out offset, offset); // point_step
            //_ = Helper.ReadRosBaseType<Int32>(data, out offset, offset); // row_step
            offset += 1 + 4 + 4;
            int size = Helper.ReadRosBaseType<Int32>(data, out offset, offset); // size == (row_step * height)

            // byte[] point_data = Helper.ReadRosBaseTypeArray<Byte>(data, out offset, offset);
            // byte[] point_data = data.Skip(offset).ToArray();
            List<Point3D> points = new List<Point3D>();
            for (int end_point = offset + size; offset < end_point;)
            {
                //Point3D point = GeometrymsgsPoint32Deserializer.Deserialize(data, ref offset);
                points.Add(GeometrymsgsPoint32Deserializer.Deserialize(data, ref offset));
            }
            // _ = Helper.ReadRosBaseType<Boolean>(data, out offset, offset); // is_dense
            offset += 1;
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

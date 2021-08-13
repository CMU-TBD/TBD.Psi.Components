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
            /*  The following deserializer extracts the points from uint8[] data from the PointCloud2
             *  message and returns a Point3D list. The other variables in the message are ignored, but
             *  can be implemented by giving them a variable name.
             */
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out offset, offset);
            _ = Helper.ReadRosBaseType<Int32>(data, out offset, offset);            // height
            _ = Helper.ReadRosBaseType<Int32>(data, out offset, offset);            // width
            
            // PointField[] fields
            _ = Helper.ReadRosBaseType<Int32>(data, out offset, offset);            // size of PointField[] fields
            var x = SensorMsgsPointFieldDeserializer.Deserialize(data, ref offset); // fields[0]
            var y = SensorMsgsPointFieldDeserializer.Deserialize(data, ref offset); // fields[1]
            var z = SensorMsgsPointFieldDeserializer.Deserialize(data, ref offset); // fields[2]

            _ = Helper.ReadRosBaseType<Boolean>(data, out offset, offset);          // bool is_bigendian
            _ = Helper.ReadRosBaseType<Int32>(data, out offset, offset);            // point_step
            _ = Helper.ReadRosBaseType<Int32>(data, out offset, offset);            // row_step
            
            // Looping through uint8[] data to extract the points and put them in a Point3D list.
            // This is assuming the dtype of x, y, and z are equivalent and equal to 7, indicating
            // that the datatype of the points is Float32.
            int size = Helper.ReadRosBaseType<Int32>(data, out offset, offset);
            List<Point3D> points = new List<Point3D>(size);
            for (int end_point = offset + size; offset < end_point;) {
                Point3D point = GeometrymsgsPoint32Deserializer.Deserialize(data, ref offset);
                points.Add(point);
            }
            _ = Helper.ReadRosBaseType<Boolean>(data, out offset, offset);          // bool is_dense
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

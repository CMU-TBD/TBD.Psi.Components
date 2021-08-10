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

    public class SensorMsgsPointCloudDeserializer : MsgDeserializer
    {

        public SensorMsgsPointCloudDeserializer()
            : base(typeof(List<Point3D>).AssemblyQualifiedName, "sensor_msgs/PointCloud")
        { 
        }

        public static List<Point3D> Deserialize(byte[] data, ref int offset)
        {
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out offset, offset);
            List<Point3D> points = new List<Point3D>();

            int num_points = Helper.ReadRosBaseType<Int32>(data, out offset, offset);
            for (int i = 0; i < num_points; i++) {
                Point3D point = GeometrymsgsPoint32Deserializer.Deserialize(data, ref offset);
                points.Add(point);
            }

            // The following portion is for getting color data from the ChannelFloat32.msg
            // However, we won't implement it as Psi does not give us a way to color individual points
            int channels_size = Helper.ReadRosBaseType<Int32>(data, out offset, offset);
            for (int i = 0; i < channels_size; i++) {
                _ = Helper.ReadRosBaseType<String>(data, out offset, offset);
                int values_size = Helper.ReadRosBaseType<Int32>(data, out offset, offset);
                for (int j = 0; j < values_size; j++) {
                    _ = Helper.ReadRosBaseType<float>(data, out offset, offset);
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

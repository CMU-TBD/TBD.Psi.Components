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

    public class SensorMsgsPointFieldDeserializer : MsgDeserializer
    {
        public SensorMsgsPointFieldDeserializer()
            : base(typeof((string name, int off, byte dtype, int count)).AssemblyQualifiedName, "sensor_msgs/PointField")
        { 
        }

        public static (string name, int off, byte dtype, int count) Deserialize(byte[] data, ref int offset)
        {
            var name = Helper.ReadRosBaseType<string>(data, out offset, offset);
            var off = Helper.ReadRosBaseType<Int32>(data, out offset, offset);
            var dtype = Helper.ReadRosBaseType<Byte>(data, out offset, offset);
            var count = Helper.ReadRosBaseType<Int32>(data, out offset, offset);
            return (name, off, dtype, count);

        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            int offset = 0;
            return (T) (object) Deserialize(data, ref offset);
        }
    }
}

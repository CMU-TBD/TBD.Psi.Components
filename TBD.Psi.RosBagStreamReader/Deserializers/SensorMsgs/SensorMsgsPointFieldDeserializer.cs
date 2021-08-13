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
            /*  The following deserializer extracts the four variables within a sensor_msgs/PointField ROS message
             *  and returns them in order within a tuple. The four variables describe the name of the field, the offset
             *  from the start of the point struct, the datatype enumeration, and how many elements are in the field, respectively.
             */
            string name = Helper.ReadRosBaseType<string>(data, out offset, offset);     // string name
            int off = Helper.ReadRosBaseType<Int32>(data, out offset, offset);          // uint32 offset
            byte dtype = Helper.ReadRosBaseType<Byte>(data, out offset, offset);        // uint8 datatype
            int count = Helper.ReadRosBaseType<Int32>(data, out offset, offset);        // uint32 count
            return (name, off, dtype, count);

        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            int offset = 0;
            return (T) (object) Deserialize(data, ref offset);
        }
    }
}

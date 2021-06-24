namespace TBD.Psi.RosBagStreamReader.Deserailizers
{
    using System;
    using Microsoft.Psi;

    public class StdMsgsColorRGBADeserializer : MsgDeserializer
    {
        public StdMsgsColorRGBADeserializer()
            : base(typeof(double[]).AssemblyQualifiedName, "std_msgs/ColorRGBA")
        {
        }

        public double[] Deserialize(byte[] msgByteArr, ref int offset)
        {
            var color = new double[4];
            color[0] = Helper.ReadRosBaseType<float>(msgByteArr, out offset, offset); // r
            color[1] = Helper.ReadRosBaseType<float>(msgByteArr, out offset, offset); // g
            color[2] = Helper.ReadRosBaseType<float>(msgByteArr, out offset, offset); // b
            color[3] = Helper.ReadRosBaseType<float>(msgByteArr, out offset, offset); // a
            return color;

        }

        public override T Deserialize<T>(byte[] msgByteArr, ref Envelope env)
        {
            var offset = 0;
            return (T)(object) this.Deserialize(msgByteArr, ref offset);
        }
    }
}
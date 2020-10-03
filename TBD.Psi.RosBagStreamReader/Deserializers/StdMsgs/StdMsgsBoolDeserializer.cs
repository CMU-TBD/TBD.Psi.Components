namespace TBD.Psi.RosBagStreamReader.Deserailizers
{
    using System;
    using Microsoft.Psi;

    public class StdMsgsBoolDeserializer : MsgDeserializer
    {
        public StdMsgsBoolDeserializer()
            : base(typeof(bool).AssemblyQualifiedName, "std_msgs/Bool")
        {
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // convert it
            return (T) (object) BitConverter.ToBoolean(data, 0);
        }
    }
}
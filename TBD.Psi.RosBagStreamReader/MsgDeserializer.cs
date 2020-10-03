namespace TBD.Psi.RosBagStreamReader
{
    using System;
    using Microsoft.Psi;
    public abstract class MsgDeserializer
    {
        protected bool useHeaderTimeAsOriginatingTime;

        public MsgDeserializer(string assemblyName, string messageName)
            : this(assemblyName, messageName, false)
        {
        }

        public MsgDeserializer(string assemblyName, string messageName, bool useHeaderTime)
        {
            this.useHeaderTimeAsOriginatingTime = useHeaderTime;
            this.AssemblyName = assemblyName;
            this.RosMessageTypeName = messageName;
        }

        protected void UpdateEnvelope(Envelope env, DateTime originTime)
        {
            if (this.useHeaderTimeAsOriginatingTime)
            {
                env.OriginatingTime = originTime;
            }
        }

        public abstract T Deserialize<T>(byte[] data, Envelope envelop);
        public string AssemblyName { get; private set; }
        public string RosMessageTypeName { get; private set; }
    }
}
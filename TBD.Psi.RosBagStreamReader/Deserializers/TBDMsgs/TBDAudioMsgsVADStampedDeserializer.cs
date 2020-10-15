namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System;
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Audio;
    class TBDAudioMsgsVADStampedDeserializer : MsgDeserializer
    {
        public TBDAudioMsgsVADStampedDeserializer(bool useHeader)
            : base(typeof(bool).AssemblyQualifiedName, "tbd_audio_msgs/VADStamped", useHeader)
        {
        }

        public override T Deserialize<T>(byte[] data, ref Envelope envelop)
        {
            // read the header and get location
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out var offset, 0);
            this.UpdateEnvelope(ref envelop, originTime);

            return (T)(object)BitConverter.ToBoolean(data, offset);
        }
    }
}

namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Language;
    class TBDAudioMsgsUtterancedDeserializer : MsgDeserializer
    {
        public TBDAudioMsgsUtterancedDeserializer(bool useHeader)
            : base(typeof(string).AssemblyQualifiedName, "tbd_audio_msgs/Utterance", useHeader)
        {
        }

        public override T Deserialize<T>(byte[] data, ref Envelope envelop)
        {
            // read the header and get location
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out var offset, 0);
            this.UpdateEnvelope(ref envelop, originTime);

            // return a string
            return (T)(object) Helper.ReadMsgString(data, offset);

        }
    }
}

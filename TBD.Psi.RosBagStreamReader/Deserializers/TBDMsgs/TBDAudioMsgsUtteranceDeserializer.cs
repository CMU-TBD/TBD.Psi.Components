namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System;
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Language;
    class TBDAudioMsgsUtterancedDeserializer : MsgDeserializer
    {
        public TBDAudioMsgsUtterancedDeserializer(bool useHeader)
            : base(typeof(Tuple<string, TimeSpan>).AssemblyQualifiedName, "tbd_audio_msgs/Utterance", useHeader)
        {
        }

        public override T Deserialize<T>(byte[] data, ref Envelope envelop)
        {
            // read the header and get location
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out var offset, 0);
            this.UpdateEnvelope(ref envelop, originTime);

            // get a string
            var text = Helper.ReadRosBaseType<string>(data, out offset, offset);

            // the next offset is the confidence
            var confidence = Helper.ReadRosBaseType<float>(data, out offset, offset);

            // the next is the end time
            var end_time = Helper.ReadRosBaseType<DateTime>(data, out offset, offset);

            return (T)(object)new Tuple<string, TimeSpan>(text, end_time - originTime);

        }
    }
}

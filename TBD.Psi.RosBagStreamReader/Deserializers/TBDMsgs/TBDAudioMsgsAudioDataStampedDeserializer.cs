namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Audio;
    class TBDAudioMsgsAudioDataStampedDeserializer : MsgDeserializer
    {
        public TBDAudioMsgsAudioDataStampedDeserializer(bool useHeader)
            : base(typeof(AudioBuffer).AssemblyQualifiedName, "tbd_audio_msgs/AudioDataStamped", useHeader)
        {
        }

        public override T Deserialize<T>(byte[] data, ref Envelope envelop)
        {
            // read the header and get location
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out var offset, 0);
            this.UpdateEnvelope(ref envelop, originTime);

            return (T)(object)new AudioBuffer(data.Skip(offset + 4).ToArray(), WaveFormat.Create16kHz1Channel16BitPcm());
        }
    }
}

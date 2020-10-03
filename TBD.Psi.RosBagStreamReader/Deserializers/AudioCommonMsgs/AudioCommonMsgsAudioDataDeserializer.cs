
namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Audio;
    class AudioCommonMsgsAudioDataDeserializer : MsgDeserializer
    {
        public AudioCommonMsgsAudioDataDeserializer()
            : base(typeof(AudioBuffer).AssemblyQualifiedName, "audio_common_msgs/AudioData")
        {
        }

        public override T Deserialize<T>(byte[] data, ref Envelope envelop)
        {
            return (T) (object) new AudioBuffer(data.Skip(4).ToArray(), WaveFormat.Create16kHz1Channel16BitPcm());
        }
    }
}

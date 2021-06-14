
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBD.Psi.RosSharpBridge.Messages
{
    using RosSharp.RosBridgeClient;
    using RosSharp.RosBridgeClient.MessageTypes.Std;

    public class AudioData : Message
    {
        public const string RosMessageName = "audio_common_msgs/AudioData";

        public byte[] data { get; set; }

        public AudioData()
        {
            this.data = new byte[0];
        }

        public AudioData(byte[] data)
        {
            this.data = data;
        }
    }

}

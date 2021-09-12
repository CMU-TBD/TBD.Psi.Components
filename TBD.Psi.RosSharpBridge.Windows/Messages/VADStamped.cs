
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBD.Psi.RosSharpBridge.Messages
{
    using RosSharp.RosBridgeClient;
    using RosSharp.RosBridgeClient.MessageTypes.Std;

    public class VADStamped : Message
    {
        public const string RosMessageName = "tbd_audio_msgs/VADStamped";

        public Header header { get; set; }
        public bool is_speech { get; set; }
      
        public VADStamped()
        {
            this.header = new Header();
            this.is_speech = false;
        }

        public VADStamped(Header header, bool is_speech)
        {
            this.header = header;
            this.is_speech = is_speech;
        }
    }

}

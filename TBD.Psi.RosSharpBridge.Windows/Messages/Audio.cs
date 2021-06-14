
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBD.Psi.RosSharpBridge.Messages
{
    using RosSharp.RosBridgeClient;
    using RosSharp.RosBridgeClient.MessageTypes.Std;

    public class Utterance : Message
    {
        public const string RosMessageName = "tbd_audio_msgs/Utterance";

        public Header header { get; set; }
        public string text { get; set; }
        public float confidence { get; set; }
        public Time end_time { get; set; }

        public string[] word_list { get; set; }
        public short[] timing_list { get; set; }

        public Utterance()
        {
            this.header = new Header();
            this.text = "";
            this.confidence = 0;
            this.end_time = new Time();
            this.word_list = new string[0];
            this.timing_list = new short[0];

        }

        public Utterance(Header header, string text, float confidence, Time end_time, string[] word_list, short[] timing_list)
        {
            this.header = header;
            this.text = text;
            this.confidence = confidence;
            this.end_time = end_time;
            this.word_list = word_list;
            this.timing_list = timing_list;
        }
    }

}

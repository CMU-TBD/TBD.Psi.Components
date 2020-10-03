using System.Text;
using System.Linq;
using Microsoft.Psi;

namespace TBD.Psi.RosBagStreamReader.Deserailizers
{
    public class StdMsgsStringDeserializer : MsgDeserializer
    {
        public StdMsgsStringDeserializer()
            : base(typeof(string).AssemblyQualifiedName, "std_msgs/String")
        {
        }

        public override T Deserialize<T>(byte[] data, Envelope env)
        {
            // convert it
            var output = Encoding.UTF8.GetString(data, 4, data.Length - 4);
            return (T) (object) output;
        }
    }
}
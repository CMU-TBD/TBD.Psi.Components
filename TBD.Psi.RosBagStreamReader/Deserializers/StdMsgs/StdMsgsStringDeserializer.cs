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

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // convert it
            return (T)(object)Helper.ReadRosBaseType<string>(data, out _, 0);
        }
    }
}
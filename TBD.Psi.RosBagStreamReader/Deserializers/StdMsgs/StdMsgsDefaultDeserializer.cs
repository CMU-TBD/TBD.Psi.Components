using System.Text;
using System.Linq;
using Microsoft.Psi;

namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    public class StdMsgsDefaultDeserializer<T> : MsgDeserializer
    {
        public StdMsgsDefaultDeserializer(string topicName)
            : base(typeof(T).AssemblyQualifiedName, topicName)
        {
        }

        public override K Deserialize<K>(byte[] data, ref Envelope env)
        {
            // convert it according to ROS Bag Type.
            return (K)(object)Helper.ReadRosBaseType<T>(data);
        }
    }
}
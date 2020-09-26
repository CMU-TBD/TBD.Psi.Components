using System.Text;
using System.Linq;

namespace TBD.Psi.RosBagStreamReader.Deserailizers
{
    public class StdMsgsStringDeserializer : IDeserializer
    {
        public T deserialize<T>(byte[] data)
        {
            // convert it
            var output = Encoding.UTF8.GetString(data, 4, data.Length - 4);
            return (T) (object) output;
        }
        public string getAssemblyName()
        {
            return typeof(string).AssemblyQualifiedName;
        }

        public string getMessageTypeName()
        {
            return "std_msgs/String";
        }
    }
}
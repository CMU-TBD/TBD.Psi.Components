
namespace TBD.Psi.RosBagStreamReader
{
    using Microsoft.Psi;
    public class RosBagStore
    {
        public static RosBagImporter Open(
            Pipeline pipeline,
            string name,
            string path)
        {
            return new RosBagImporter(pipeline, name, path);
        }
    }
}

namespace TBD.Psi.RosBagStreamReader
{
    using System.Collections.Generic;
    public interface IDeserializer
    {
        string getAssemblyName();
        string getMessageTypeName();
        T deserialize<T>(byte[] data);
    }
}
namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System;
    using System.Text;
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Imaging;
    using System.IO;
    using MathNet.Spatial.Euclidean;
    using MathNet.Numerics.LinearAlgebra;

    public class GeometrymsgsPoseStampedDeserializer : MsgDeserializer
    {
        public GeometrymsgsPoseStampedDeserializer(bool useHeaderTime)
            : base(typeof(CoordinateSystem).AssemblyQualifiedName, "geometry_msgs/PoseStamped", useHeaderTime)
        { 
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // read the header and get location
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out var offset);
            this.UpdateEnvelope(ref env, originTime);

            // convert to coordinate systems
            var cs = GeometrymsgsPoseDeserializer.Deserialize(data, ref offset);

            return (T)(object)cs;
        }
    }
}

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

    public class GeometrymsgsTransformDeserializer : MsgDeserializer
    {
        public GeometrymsgsTransformDeserializer()
            : base(typeof(CoordinateSystem).AssemblyQualifiedName, "geometry_msgs/Transform")
        { 
        }

        private static CoordinateSystem ConvertToCoordinateSystem(Quaternion q, Vector3D vec)
        {
            var mat = Matrix<double>.Build.DenseIdentity(4);
            mat[0, 3] = vec.X;
            mat[1, 3] = vec.Y;
            mat[2, 3] = vec.Z;

            q = q.Normalized;
            // convert quaternion to matrix
            mat[0, 0] = 1 - 2 * q.ImagY * q.ImagY - 2 * q.ImagZ * q.ImagZ;
            mat[0, 1] = 2 * q.ImagX * q.ImagY - 2 * q.ImagZ * q.Real;
            mat[0, 2] = 2 * q.ImagX * q.ImagZ + 2 * q.ImagY * q.Real;
            mat[1, 0] = 2 * q.ImagX * q.ImagY + 2 * q.ImagZ * q.Real;
            mat[1, 1] = 1 - 2 * q.ImagX * q.ImagX - 2 * q.ImagZ * q.ImagZ;
            mat[1, 2] = 2 * q.ImagY * q.ImagZ - 2 * q.ImagX * q.Real;
            mat[2, 0] = 2 * q.ImagX * q.ImagZ - 2 * q.ImagY * q.Real;
            mat[2, 1] = 2 * q.ImagY * q.ImagZ + 2 * q.ImagX * q.Real;
            mat[2, 2] = 1 - 2 * q.ImagX * q.ImagX - 2 * q.ImagY * q.ImagY;

            return new CoordinateSystem(mat);
        }

        public static CoordinateSystem Deserialize(byte[] data, ref int offset)
        {
            // get the translation vector
            var translation = GeometrymsgsVector3Deserializer.Deserialize(data, ref offset);
            // get the rotation quaterion
            var quaternion = GeometrymsgsQuaternionDeserializer.Deserialize(data, ref offset);
            // combine both into a coordinate system
            return ConvertToCoordinateSystem(quaternion, translation);
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // convert to coordinate systems
            int offset = 0;
            var cs = Deserialize(data, ref offset);

            return (T)(object)cs;
        }
    }
}

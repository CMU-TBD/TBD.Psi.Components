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

        private CoordinateSystem ConvertQuaternionToMatrix(Quaternion q, Point3D point)
        {
            var mat = Matrix<double>.Build.DenseIdentity(4);
            mat[0, 3] = point.X;
            mat[1, 3] = point.Y;
            mat[2, 3] = point.Z;

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

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // read the header and get location
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out var offset);
            this.UpdateEnvelope(ref env, originTime);

            // get Point
            var position_x = Helper.ReadMsgFloat64(data, out offset, offset);
            var position_y = Helper.ReadMsgFloat64(data, out offset, offset);
            var position_z = Helper.ReadMsgFloat64(data, out offset, offset);
            var point = new Point3D(position_x, position_y, position_z);
            // quaternion
            var quaternion_x = Helper.ReadMsgFloat64(data, out offset, offset);
            var quaternion_y = Helper.ReadMsgFloat64(data, out offset, offset);
            var quaternion_z = Helper.ReadMsgFloat64(data, out offset, offset);
            var quaternion_w = Helper.ReadMsgFloat64(data, out offset, offset);
            var quaternion = new Quaternion(quaternion_w, quaternion_x, quaternion_y, quaternion_z);

            return (T)(object)this.ConvertQuaternionToMatrix(quaternion, point);

        }
    }
}

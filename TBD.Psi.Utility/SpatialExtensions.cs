using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;
using System;

namespace TBD.Psi.Utility
{
    using MathNet.Numerics.LinearAlgebra;
    using System.Numerics;
    public static class SpatialExtensions
    {

        // Fixed Angle Rotation
        public static Quaternion ConstructQuaternion(double roll, double pitch, double yaw)
        {

            var rollQ = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), Convert.ToSingle(roll));
            var pitchQ = Quaternion.CreateFromAxisAngle(new Vector3(0,1,0), Convert.ToSingle(pitch));
            var yawQ = Quaternion.CreateFromAxisAngle(new Vector3(0,0,1), Convert.ToSingle(yaw));

            return Quaternion.Multiply(yawQ, Quaternion.Multiply(pitchQ, rollQ));
        }

        public static CoordinateSystem ConstructCoordinateSystem(double x, double y, double z, double roll, double pitch, double yaw)
        {
            return ConstructCoordinateSystem(x, y, z, ConstructQuaternion(roll, pitch, yaw));
        }

        public static CoordinateSystem ConstructCoordinateSystem(double x, double y, double z, Quaternion q)
        {
            return ConstructCoordinateSystem(new Vector3D(x, y, z), q);
        }

        public static CoordinateSystem ConstructCoordinateSystem(Vector3D origin, Quaternion q)
        {
            var transMat = Matrix<double>.Build.DenseIdentity(4, 4);
            double sqw = q.W * q.W;
            double sqx = q.X * q.X;
            double sqy = q.Y * q.Y;
            double sqz = q.Z * q.Z;

            transMat.At(0, 0, sqx - sqy - sqz + sqw);
            transMat.At(1, 1, -sqx + sqy - sqz + sqw);
            transMat.At(2, 2, -sqx - sqy + sqz + sqw);

            double tmp1 = q.X * q.Y;
            double tmp2 = q.Z * q.W;

            transMat.At(1, 0, (tmp1 + tmp2) * 2);
            transMat.At(0, 1, (tmp1 - tmp2) * 2);

            tmp1 = q.X * q.Z;
            tmp2 = q.Y * q.W;
            transMat.At(2, 0, 2 * (tmp1 - tmp2));
            transMat.At(0, 2, 2 * (tmp1 + tmp2));
            tmp1 = q.Y * q.Z;
            tmp2 = q.X * q.W;
            transMat.At(2, 1, 2 * (tmp1 + tmp2));
            transMat.At(1, 2, 2 * (tmp1 - tmp2));

            transMat.At(0, 3, origin.X);
            transMat.At(1, 3, origin.Y);
            transMat.At(2, 3, origin.Z);

            return new CoordinateSystem(transMat);
        }
    }
}

namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System;
    using System.Text;
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Imaging;
    using Microsoft.Psi.Calibration;
    using TBD.Psi.RosBagStreamReader.Deserializers.SensorMsgs;
    using MathNet.Numerics.LinearAlgebra;

    public class SensorMsgsCameraInfoDeserializer : MsgDeserializer
    {
        public SensorMsgsCameraInfoDeserializer(bool useHeaderTime)
            : base(typeof(CameraIntrinsics).AssemblyQualifiedName, "sensor_msgs/CameraInfo", useHeaderTime)
        {
        }

        public static CameraIntrinsics Deserialize(byte[] data, ref int offset)
        {
            int height = (int)BitConverter.ToInt32(data, offset);
            int width = (int)BitConverter.ToInt32(data, offset + 4);
            offset += 4 + 4;
            String distortion_model = Helper.ReadRosBaseType<String>(data, out offset, offset); // can either be "plumb_bob" or "rational_polynomial" or "equidistant"
            int D_length = (int)BitConverter.ToInt32(data, offset);
            offset += 4;
            float[] D = new float[D_length]; // distortion parameters, For "plumb_bob", the 5 parameters are: (k1, k2, t1, t2, k3)
            int i, k;
            for (i = 0; i < D_length; i++)
            {
                D[i] = Helper.ReadRosBaseType<float>(data, out offset, offset);
            }
            offset += 4; // the following array K has size 9, no need to read length
            double[,] transform = new double[3, 3]; // intrinsic camera matrix for the raw images
            for (i = 0, k = -1; i < 9; i++)
            {
                if (i % 3 == 0)
                {
                    k++;
                }
                float c = Helper.ReadRosBaseType<float>(data, out offset, offset);
                transform[k, i % 3] = (double)c;
            }
            var M = Matrix<double>.Build;
            Matrix<double> camera_matrix = M.DenseOfArray(transform);

            offset += 4; // the following array R has size 9, no need to read length
            float[] R = new float[9]; // Rectification matrix
            for (i = 0; i < 9; k++)
            {
                R[i] = Helper.ReadRosBaseType<float>(data, out offset, offset);
            }
            offset += 4; // the following array P has size 12, no need to read length
            float[] P = new float[12];
            for (i = 0; i < D_length; i++)
            {
                P[i] = Helper.ReadRosBaseType<float>(data, out offset, offset);
            }
            int binning_x = (int)BitConverter.ToInt32(data, offset);
            int binning_y = (int)BitConverter.ToInt32(data, offset + 4);
            offset += 4 + 4 + 4 + 4 + 4 + 4 + 1; // the above integers + RegionOfInterest roi: 4 int32s and 1 bool
            CameraIntrinsics cameraIntrinsics = new CameraIntrinsics(width, height, camera_matrix);
            return cameraIntrinsics;
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            int offset = 0;
            // read the header and get location
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out var infoIndex, 0);
            this.UpdateEnvelope(ref env, originTime);
            return (T)(object)Deserialize(data, ref offset);
        }
    }
}

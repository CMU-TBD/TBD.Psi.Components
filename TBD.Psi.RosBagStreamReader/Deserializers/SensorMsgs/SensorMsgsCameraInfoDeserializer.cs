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
            /*  The following deserializer extracts a CameraIntrinsics object from the available information
             *  in a given sensor_msgs/CameraInfo ROS message.
             */
            int height = Helper.ReadRosBaseType<Int32>(data, out offset, offset);   // uint32 height
            int width = Helper.ReadRosBaseType<Int32>(data, out offset, offset);    // uint32 width
            _ = Helper.ReadRosBaseType<string>(data, out offset, offset);           // string distortion_model

            // The distortion parameters, size depending on the distortion model
            // float64[] D
            int size = Helper.ReadRosBaseType<Int32>(data, out offset, offset);
            for (int i = 0; i < size; i++) {
                _ = Helper.ReadRosBaseType<float>(data, out offset, offset); // D[i]
            }

            // Intrinsic camera matrix for the raw (distorted) images.
            // float64[9] K
            size = Helper.ReadRosBaseType<Int32>(data, out offset, offset);
            double[,] transform = new double[3, 3]; 
            for (int i = 0, k = -1; i < size; i++) {
                if (i % 3 == 0) {
                    k++;
                }
                transform[k, i % 3] = (double)Helper.ReadRosBaseType<float>(data, out offset, offset);
            }
            Matrix<double> camera_matrix = Matrix<double>.Build.DenseOfArray(transform);

            // Rectification matrix (stereo cameras only)
            // float64[9] R
            size = Helper.ReadRosBaseType<Int32>(data, out offset, offset);
            for (int i = 0; i < 9; i++) {
                _ = Helper.ReadRosBaseType<float>(data, out offset, offset); // R[i]
            }

            // Projection/camera matrix
            // float64[12] P, 3 x 4 row-major matrix
            size = Helper.ReadRosBaseType<Int32>(data, out offset, offset);
            for (int i = 0; i < size; i++) {
                _ = Helper.ReadRosBaseType<float>(data, out offset, offset); // P[i]
            }
            _ = Helper.ReadRosBaseType<Int32>(data, out offset, offset);        // uint32 binning_x
            _ = Helper.ReadRosBaseType<Int32>(data, out offset, offset);        // uint32 binning_y

            // Region Of Interest
            _ = Helper.ReadRosBaseType<Int32>(data, out offset, offset);        // uint32 x_offset
            _ = Helper.ReadRosBaseType<Int32>(data, out offset, offset);        // uint32 y_offset
            _ = Helper.ReadRosBaseType<Int32>(data, out offset, offset);        // uint32 height
            _ = Helper.ReadRosBaseType<Int32>(data, out offset, offset);        // uint32 width
            _ = Helper.ReadRosBaseType<Boolean>(data, out offset, offset);      // bool do_rectify

            return new CameraIntrinsics(width, height, camera_matrix);
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

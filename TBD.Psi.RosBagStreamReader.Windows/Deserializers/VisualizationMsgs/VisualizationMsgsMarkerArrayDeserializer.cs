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
    using Microsoft.Psi.AzureKinect;
    using System.Diagnostics;
    using Microsoft.Azure.Kinect.BodyTracking;
    using System.Collections.Generic;

    public class VisualizationMsgsMarkerArrayDeserializer : MsgDeserializer
    {
        private static readonly CoordinateSystem KinectBasis = new CoordinateSystem(default, UnitVector3D.ZAxis, UnitVector3D.XAxis.Negate(), UnitVector3D.YAxis.Negate());
        private static readonly CoordinateSystem KinectBasisInverted = KinectBasis.Invert();

        public VisualizationMsgsMarkerArrayDeserializer()
            : base(typeof(List<AzureKinectBody>).AssemblyQualifiedName, "visualization_msgs/MarkerArray")
        { 
        }

        public static List<AzureKinectBody> Deserialize(byte[] data, ref int offset)
        {
            List<AzureKinectBody> bodies = new List<AzureKinectBody>();
            int num_bodies = (int)BitConverter.ToInt32(data, offset) / Skeleton.JointCount;
            offset += 4;
            for (int i = 0; i < num_bodies; i++) {
                AzureKinectBody body = new AzureKinectBody();
                for (int j = 0; j < Skeleton.JointCount; j++) {
                    var info = VisualizationMsgsMarkerDeserializer.Deserialize(data, ref offset);
                    int id = info.id;
                    CoordinateSystem pose = new CoordinateSystem(KinectBasisInverted * info.pose * KinectBasis);
                    body.Joints[(JointId)j] = (pose, JointConfidenceLevel.Medium);
                }
                bodies.Add(body);
            }
            return bodies;
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // convert to coordinate systems
            int offset = 0;
            return (T)(object)Deserialize(data, ref offset);
        }
    }
}

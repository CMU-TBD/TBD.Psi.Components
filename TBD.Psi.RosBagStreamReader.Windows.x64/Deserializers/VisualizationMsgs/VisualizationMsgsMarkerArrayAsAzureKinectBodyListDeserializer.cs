namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System;
    using Microsoft.Psi;
    using MathNet.Spatial.Euclidean;
    using Microsoft.Psi.AzureKinect;
    using System.Collections.Generic;
    using Microsoft.Azure.Kinect.BodyTracking;

    public class VisualizationMsgsMarkerArrayAsAzureKinectBodyListDeserializer : MsgDeserializer
    {
        private static readonly CoordinateSystem KinectBasis = new CoordinateSystem(default, UnitVector3D.ZAxis, UnitVector3D.XAxis.Negate(), UnitVector3D.YAxis.Negate());
        private static readonly CoordinateSystem KinectBasisInverted = KinectBasis.Invert();

        public VisualizationMsgsMarkerArrayAsAzureKinectBodyListDeserializer()
            : base(typeof(List<AzureKinectBody>).AssemblyQualifiedName, "visualization_msgs/MarkerArray")
        { 
        }

        public static List<AzureKinectBody> Deserialize(byte[] data, ref int offset)
        {
            /*  The following deserializer reads in a MarkerArray with a size of 32 * n, where n is the
             *  number of bodies captured by the Azure Kinect. It creates an AzureKinectBody object for each
             *  32 joint interval, correcting the orientation of each joint in the process.
             */
            int num_bodies = Helper.ReadRosBaseType<Int32>(data, out offset, offset) / Skeleton.JointCount;
            List<AzureKinectBody> bodies = new List<AzureKinectBody>(num_bodies);
            for (int i = 0; i < num_bodies; i++) {
                AzureKinectBody body = new AzureKinectBody();
                for (int j = 0; j < Skeleton.JointCount; j++) {
                    var info = VisualizationMsgsMarkerDeserializer.Deserialize(data, ref offset);
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

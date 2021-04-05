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
    using TBD.Psi.TransformationTree;
    using System.Collections.Generic;

    public class TFMessageDeserializer : MsgDeserializer
    {
        private TransformationTree<string> localTree = new TransformationTree<string>();
        private Dictionary<string, DateTime> lastUpdateList = new Dictionary<string, DateTime>();
        public TFMessageDeserializer()
            : base(typeof(TransformationTree<string>).AssemblyQualifiedName, "tf2_msgs/TFMessage")
        {
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // go through the array of it
            var offset = 0;
            var numOfTransform = BitConverter.ToInt32(data, offset);
            offset += 4;
            for (var i = 0; i < numOfTransform; i++)
            {
                // Deserialize like transform 
                (var time, var parentId, var childId, var transform) = GeometrymsgsTransformStampedDeserializer.Deserialize(data, ref offset);
                // TODO: The way TF is handled, we don't have gurantee that the message comes in sequential order. This create a problem
                // where we just have the most recent data.
                this.localTree.UpdateTransformation(parentId, childId, transform);
            }

            return (T)(object)this.localTree.DeepClone();
        }
    }
}

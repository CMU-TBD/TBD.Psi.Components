
namespace TBD.Psi.TransformTree
{
    using System.Linq;
    using System.Collections.Generic;
    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Spatial.Euclidean;
    using Microsoft.Psi;

    public class TransformationTree<T>
    {

        protected Dictionary<T, Dictionary<T, CoordinateSystem>> tree = new Dictionary<T, Dictionary<T, CoordinateSystem>>();

        protected TransformationTreeNode<T> rootNode;


        protected void traverseTree(T parent, CoordinateSystem transform, List<(T id, CoordinateSystem Pose)> frameList)
        {
            foreach(var child in this.tree[parent].Keys)
            {
                var childTransfrom = this.tree[parent][child].TransformBy(transform);
                frameList.Add((child, childTransfrom));
                if (this.tree.ContainsKey(child))
                {
                    this.traverseTree(child, childTransfrom, frameList);
                }
            }
        }


        public TransformationTree()
        {
        }

        public List<(T, CoordinateSystem)> TraverseTree(T root, CoordinateSystem rootPose)
        {
            var poseList = new List<(T Id, CoordinateSystem Pose)>();
            this.traverseTree(root, rootPose, poseList);
            return poseList;
        }

        public bool UpdateTransformation(T frameA, T frameB, double[,] mat)
        {
            return this.UpdateTransformation(frameA, frameB, new CoordinateSystem(Matrix<double>.Build.DenseOfArray(mat)));
        }

        public bool UpdateTransformation(T parentKey, T childKey, CoordinateSystem transform)
        {
            // if there is no root make one
            if (this.rootNode == null)
            {
                this.rootNode = new TransformationTreeNode<T>(parentKey, true);
                this.rootNode.AddChild(childKey, transform);
            }
            else
            {
                // if 

            }




            if (this.tree.ContainsKey(frameA) || this.tree.ContainsKey(frameB))
            {
                // if this exist
                if (this.tree.ContainsKey(frameA) && !this.tree.ContainsKey(frameB))
                {
                    this.tree[frameA][frameB] = transform.DeepClone();
                    this.tree[frameB] = new Dictionary<T, CoordinateSystem>();
                }
                else if (!this.tree.ContainsKey(frameA) && this.tree.ContainsKey(frameB))
                {
                    this.tree[frameB][frameA] = transform.Invert();
                    this.tree[frameA] = new Dictionary<T, CoordinateSystem>();
                }
                else
                {
                    // update the existing graph to prevent loop
                    if (this.tree[frameA].ContainsKey(frameB))
                    {
                        this.tree[frameA][frameB] = transform.DeepClone();
                    }
                    else if (this.tree[frameB].ContainsKey(frameA))
                    {
                        this.tree[frameB][frameA] = transform.Invert();
                    }
                }
            }
            else
            {
                // for all of them.
                this.tree[frameA] = new Dictionary<T, CoordinateSystem>();
                this.tree[frameB] = new Dictionary<T, CoordinateSystem>();
                this.tree[frameA][frameB] = transform.DeepClone();

            }
            return true;
        }

        protected CoordinateSystem recursiveSearchNode(T parent, T target)
        {
            // check end condition
            if (this.tree[parent].ContainsKey(target))
            {
                return this.tree[parent][target];
            }
            
            foreach(var child in this.tree[parent].Keys)
            {
                if(this.tree.ContainsKey(child))
                {
                    // search the children
                    var transform = this.recursiveSearchNode(child, target);
                    if (transform != null)
                    {
                        return transform.TransformBy(this.tree[parent][child]);
                    }
                }
            }
            return null;
        }

        public bool Contains(T frame)
        {
            return this.tree.ContainsKey(frame);
        }

        public bool Contains(IEnumerable<T> frames)
        {
            return this.tree.Keys.Where(m => frames.Contains(m)).Any();
        }

        public CoordinateSystem QueryTransformation(T parentFrameKey, T childFrameKey)
        {
            // check if both frames are in the tree
            if (!this.tree.ContainsKey(parentFrameKey) || !this.tree.ContainsKey(childFrameKey))
            {
                return null;
            }

            // if they are the same, return identity
            if (EqualityComparer<T>.Default.Equals(parentFrameKey, childFrameKey))
            {
                return new CoordinateSystem();
            }

            // start from A
            var transform = this.recursiveSearchNode(parentFrameKey, childFrameKey);
            if(transform == null)
            {
                // search the otherway in case they are above it
                transform = this.recursiveSearchNode(childFrameKey, parentFrameKey);
                if (transform == null)
                {
                    return null;
                }
                return transform.Invert();
            }
            return transform;
        }
    }
}


namespace TBD.Psi.TransformationTree
{
    using System.Linq;
    using System.Collections.Generic;
    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Spatial.Euclidean;
    using Microsoft.Psi;

    public class TransformationTree<T> : ITransformationTree<T>
    {

        protected Dictionary<T, Dictionary<T, CoordinateSystem>> tree = new Dictionary<T, Dictionary<T, CoordinateSystem>>();

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

        public List<(T, CoordinateSystem)> TraverseTree(T root, CoordinateSystem rootPose = null)
        {
            if (rootPose is null)
            {
                rootPose = new CoordinateSystem();
            }
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
            if (this.tree.ContainsKey(parentKey) || this.tree.ContainsKey(childKey))
            {
                // if only the parentKey exist, add the child key and copy it
                if (this.tree.ContainsKey(parentKey) && !this.tree.ContainsKey(childKey))
                {
                    this.tree[parentKey][childKey] = transform.DeepClone();
                    this.tree[childKey] = new Dictionary<T, CoordinateSystem>();
                }
                // if only the child key exist, then it is a reverse where we add parent as a child
                // but we flip the key
                else if (!this.tree.ContainsKey(parentKey) && this.tree.ContainsKey(childKey))
                {
                    this.tree[childKey][parentKey] = transform.Invert();
                    this.tree[parentKey] = new Dictionary<T, CoordinateSystem>();
                }
                // if both key exist
                else
                {
                    // first we check if they are already connected
                    if (this.tree[parentKey].ContainsKey(childKey))
                    {
                        this.tree[parentKey][childKey] = transform.DeepClone();
                    }
                    else if (this.tree[childKey].ContainsKey(parentKey))
                    {
                        this.tree[childKey][parentKey] = transform.Invert();
                    }
                    else
                    {
                        // this means they are currently disconnected. try to combine them
                        // TODO: In the future, we should check if there is any loop or problem.
                        this.tree[parentKey][childKey] = transform.DeepClone();
                    }
                }
            }
            else
            {
                // this is first time we see either keys
                this.tree[parentKey] = new Dictionary<T, CoordinateSystem>();
                this.tree[childKey] = new Dictionary<T, CoordinateSystem>();
                this.tree[parentKey][childKey] = transform.DeepClone();
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

        /// <summary>
        /// Whether the tree contains the identifier key.
        /// </summary>
        /// <param name="key">identifier key.</param>
        /// <returns></returns>
        public bool Contains(T key)
        {
            return this.tree.ContainsKey(key);
        }

        /// <summary>
        /// Whether the tree contains all of the given identifier keys.
        /// </summary>
        /// <param name="keys">identifier keys.</param>
        /// <returns></returns>
        public bool Contains(IEnumerable<T> keys)
        {
            return this.tree.Keys.Where(m => keys.Contains(m)).Any();
        }

        /// <summary>
        /// Find the coordinate system transformation from the parent identifier key
        /// to the child identifier key. null if there is no connection 
        /// </summary>
        /// <param name="parentKey">Parent identifier key.</param>
        /// <param name="childKey">Child identifier key.</param>
        /// <returns>The transformation or null if there is no connection</returns>
        public CoordinateSystem QueryTransformation(T parentKey, T childKey)
        {
            // check if both frames are in the tree
            if (!this.tree.ContainsKey(parentKey) || !this.tree.ContainsKey(childKey))
            {
                return null;
            }
            // if they are the same, return identity
            if (EqualityComparer<T>.Default.Equals(parentKey, childKey))
            {
                return new CoordinateSystem();
            }

            // start from the parent and recursively look for the key.
            var transform = this.recursiveSearchNode(parentKey, childKey);
            if(transform == null)
            {
                // search the otherway in case they are above it
                transform = this.recursiveSearchNode(childKey, parentKey);
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

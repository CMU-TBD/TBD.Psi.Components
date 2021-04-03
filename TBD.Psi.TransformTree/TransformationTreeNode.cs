
namespace TBD.Psi.TransformTree
{
    using MathNet.Spatial.Euclidean;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class TransformationTreeNode<T>
    {
        private bool root = false;
        public TransformationTreeNode(T key, bool isRoot = false)
        {
            this.Key = key;
            this.Transform = new CoordinateSystem();
            this.root = isRoot;
        }

        public TransformationTreeNode(T key, CoordinateSystem transform)
        {
            this.Key = key;
            this.Transform = transform;
        }

        public TransformationTreeNode(T key, TransformationTreeNode<T> parent, CoordinateSystem transform)
        {
            this.Key = key;
            this.Transform = transform;
            this.Parent = parent;
        }

        public void AddChild(T childKey, CoordinateSystem transform)
        {
            var newNode = new TransformationTreeNode<T>(childKey, this, transform);
            this.Children.Add(newNode);
        }

        public bool Contains(T key)
        {
            if (this.Key.Equals(key))
            {
                return true;
            }
            foreach(var child in this.Children)
            {
                if (child.Contains(key))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsRoot() => this.root;

        public T Key { get; set; }
        public TransformationTreeNode<T> Parent { get; set; }
        public List<TransformationTreeNode<T>> Children { get; private set; } = new List<TransformationTreeNode<T>>();
        public CoordinateSystem Transform { get; set; }
    }
}

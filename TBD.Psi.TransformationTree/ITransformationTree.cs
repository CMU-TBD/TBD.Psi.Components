

namespace TBD.Psi.TransformationTree
{
    using MathNet.Spatial.Euclidean;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface ITransformationTree<T>
    {
        /// <summary>
        /// Return the coordinate and key pairs for all the children from the root.
        /// </summary>
        /// <param name="root">Root key.</param>
        /// <param name="rootPose">Additional pose that all children pose are transformed by.</param>
        /// <returns></returns>
        List<(T, CoordinateSystem)> TraverseTree(T root, CoordinateSystem rootPose = null);

        /// <summary>
        /// Update the transformation between the parent and child. If doesn't exist,
        /// add them to the tree.
        /// </summary>
        /// <param name="parentKey">Identifier for the parent.</param>
        /// <param name="childKey">Identifier for the child.</param>
        /// <param name="transform">Transfrom from parent to child.</param>
        /// <returns></returns>
        bool UpdateTransformation(T parentKey, T childKey, CoordinateSystem transform);

        /// <summary>
        /// Whether the tree contains the identifier key.
        /// </summary>
        /// <param name="key">identifier key.</param>
        /// <returns></returns>
        bool Contains(T key);

        /// <summary>
        /// Whether the tree contains all of the given identifier keys.
        /// </summary>
        /// <param name="keys">identifier keys.</param>
        /// <returns></returns>
        bool Contains(IEnumerable<T> keys);

        /// <summary>
        /// Find the coordinate system transformation from the parent identifier key
        /// to the child identifier key. null if there is no connection 
        /// </summary>
        /// <param name="parentKey">Parent identifier key.</param>
        /// <param name="childKey">Child identifier key.</param>
        /// <returns>The transformation or null if there is no connection</returns>
        CoordinateSystem QueryTransformation(T parentKey, T childKey);

        /// <summary>
        /// Find a potential root (a key that has no parents) for the involved transformation.
        /// </summary>
        /// <returns>Key of the root if found</returns>
        /// <remarks>The code does not check if all transformations are connected. If disconnected,
        /// there is no gurantee on which graph's root it will return.</remarks>
        T FindRoot();

    }
}

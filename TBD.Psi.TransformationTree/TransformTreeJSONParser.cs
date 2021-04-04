
namespace TBD.Psi.TransformationTree
{
    using System;
    using System.IO;
    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Spatial.Euclidean;
    using Newtonsoft.Json;

    internal class TreeInnerJSONObject
    {
        [JsonProperty(PropertyName = "parent")]
        public string ParentName { get; set; }

        [JsonProperty(PropertyName = "child")]
        public string ChildName { get; set; }

        [JsonProperty(PropertyName = "matrix")]
        public double[,] TransformMatrix { get; set; }

    }

    internal class TreeJSONObject
    {
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "transformations")]
        public TreeInnerJSONObject[] Transformations { get; set; }
    }

    public class TransformationTreeJSONParser
    {
        private const string PARSERVERSION = "0.0.1";

        public static TransformationTree<string> ParseJSONFile(string pathToFile)
        {
            return ParseJSON(File.ReadAllText(pathToFile));
        }

        public static TransformationTree<string> ParseJSON(string text)
        {
            var treeObject = JsonConvert.DeserializeObject<TreeJSONObject>(text);
            // check version
            if (treeObject.Version != PARSERVERSION)
            {
                throw new NotSupportedException("Version Incorrect");
            }
            // create a new tree
            var tree = new TransformationTree<string>();
            foreach (var mapping in treeObject.Transformations)
            {
                var cs = new CoordinateSystem(Matrix<double>.Build.DenseOfArray(mapping.TransformMatrix));
                tree.UpdateTransformation(mapping.ParentName, mapping.ChildName, cs);
            }
            return tree;
        }
    }
}

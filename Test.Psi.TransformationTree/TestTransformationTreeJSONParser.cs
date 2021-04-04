
namespace Test.Psi.TransformationTree
{
    using MathNet.Spatial.Euclidean;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TBD.Psi.TransformationTree;

    [TestClass]
    public class TestTransformationTreeJSONParser
    {
        private string testText = @"{'version':'0.0.1',  
                'transformations':[{
                    'parent':'world',
                    'child':'mainCam',
                    'matrix':[
                        [1,0,0,0],
                        [0,1,0,0],
                        [0,0,1,1.5],
                        [0,0,0,1]
                        ]
                },
                {
                        'parent':'topCam',
                        'child':'mainCam',
                        'matrix':[
                            [-0.022947, -0.915372, -0.401954, 1.546493],
                            [0.999736, -0.020417, -0.010577, -2.961703],
                            [0.001475, -0.402090, 0.915599, 0.028506],
                            [0.000000, 0.000000, 0.000000, 1.000000]
                        ]
                }]}";
        [TestMethod]
        public void TestVersion()
        {
            var tree = TransformationTreeJSONParser.ParseJSON(testText);
        }

        [TestMethod]
        public void TestReadValue()
        {
            var tree = TransformationTreeJSONParser.ParseJSON(testText);
            tree.Contains("topCam");
            tree.Contains("mainCam");
            tree.Contains("world");
            var coor = tree.QueryTransformation("world", "topCam");
            Assert.AreEqual(2.9963635693064807, coor.Origin.X);
            Assert.AreEqual(1.3666093305863671, coor.Origin.Y);
            Assert.AreEqual(2.0641927410042449, coor.Origin.Z);
        }
    }
}

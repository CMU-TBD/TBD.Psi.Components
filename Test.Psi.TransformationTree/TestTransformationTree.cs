
namespace Test.Psi.TransformationTree
{
    using MathNet.Spatial.Euclidean;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TBD.Psi.TransformationTree;

    [TestClass]
    public class TestTransformationTree
    {

        [TestMethod]
        public void TestBasicCreation()
        {
            var originalCoordinate = new CoordinateSystem(new Point3D(1, 2, 3), UnitVector3D.XAxis, UnitVector3D.YAxis, UnitVector3D.ZAxis);
            var tree = new TransformationTree<string>();
            tree.UpdateTransformation("parent", "child", originalCoordinate);
            Assert.IsTrue(tree.Contains("parent"));
            Assert.IsTrue(tree.Contains("child"));
            // try quarying
            Assert.IsTrue(originalCoordinate.Equals(tree.QueryTransformation("parent", "child")));
            Assert.IsTrue(!originalCoordinate.Equals(tree.QueryTransformation("child", "parent")));
            Assert.IsTrue(originalCoordinate.Invert().Equals(tree.QueryTransformation("child", "parent")));
        }

        [TestMethod]
        public void TestInterface()
        {
            ITransformationTree<string> tree = new TransformationTree<string>();
            Assert.IsFalse(tree.Contains("hello"));
        }


        [TestMethod]
        public void TestUpdate()
        {
            var originalCoordinate = new CoordinateSystem(new Point3D(1, 2, 3), UnitVector3D.XAxis, UnitVector3D.YAxis, UnitVector3D.ZAxis);
            var tree = new TransformationTree<string>();
            tree.UpdateTransformation("parent", "child", originalCoordinate);
            var newCoordinate = new CoordinateSystem(new Point3D(6, 5, 4), UnitVector3D.ZAxis, UnitVector3D.XAxis, UnitVector3D.YAxis);
            tree.UpdateTransformation("parent", "child", newCoordinate);
            Assert.IsTrue(newCoordinate.Equals(tree.QueryTransformation("parent", "child")));
            Assert.IsFalse(originalCoordinate.Equals(tree.QueryTransformation("parent", "child")));
            // see if changing the item here, changes the value
            originalCoordinate.CopyTo(newCoordinate);
            // see whether the transform changed.
            Assert.IsFalse(originalCoordinate.Equals(tree.QueryTransformation("parent", "child")));
            var returnValue = tree.QueryTransformation("parent", "child");
            Assert.IsTrue(returnValue.Origin.X == 6);
            Assert.IsTrue(returnValue.Origin.Y == 5);
            Assert.IsTrue(returnValue.Origin.Z == 4);
        }

        [TestMethod]
        public void TestOwnership()
        {
            var originalCoordinate = new CoordinateSystem(new Point3D(6, 5, 4), UnitVector3D.ZAxis, UnitVector3D.XAxis, UnitVector3D.YAxis);
            var tree = new TransformationTree<string>();
            tree.UpdateTransformation("parent", "child", originalCoordinate);
            // see if changing the item here, changes the value
            var newCoordinate = new CoordinateSystem(new Point3D(1, 2, 3), UnitVector3D.XAxis, UnitVector3D.YAxis, UnitVector3D.ZAxis);
            newCoordinate.CopyTo(originalCoordinate);
            // see whether the transform changed.
            Assert.IsFalse(newCoordinate.Equals(tree.QueryTransformation("parent", "child")));
            var returnValue = tree.QueryTransformation("parent", "child");
            Assert.IsTrue(returnValue.Origin.X == 6);
            Assert.IsTrue(returnValue.Origin.Y == 5);
            Assert.IsTrue(returnValue.Origin.Z == 4);
        }

        [TestMethod]
        public void TestChaining()
        {
            var tree = new TransformationTree<string>();

            var rootToParent = new CoordinateSystem(new Point3D(5, 0, 0), UnitVector3D.XAxis, UnitVector3D.YAxis, UnitVector3D.ZAxis);
            var parentToChild = new CoordinateSystem(new Point3D(0, 0, 5), -1 * UnitVector3D.YAxis, 1 * UnitVector3D.XAxis, 1 * UnitVector3D.ZAxis);

            tree.UpdateTransformation("root", "parent", rootToParent);
            tree.UpdateTransformation("parent", "child", parentToChild);

            // try quarying
            Assert.IsTrue(rootToParent.Equals(tree.QueryTransformation("root", "parent")));
            Assert.IsTrue(parentToChild.Equals(tree.QueryTransformation("parent", "child")));
            // try the chaining
            Assert.IsTrue(parentToChild.TransformBy(rootToParent).Equals(tree.QueryTransformation("root", "child")));
            Assert.IsTrue(parentToChild.TransformBy(rootToParent).Invert().Equals(tree.QueryTransformation("child", "root")));
        }

        [TestMethod]
        public void TestChainingReverse()
        {
            var tree = new TransformationTree<string>();

            var rootToParent = new CoordinateSystem(new Point3D(5, 0, 0), UnitVector3D.XAxis, UnitVector3D.YAxis, UnitVector3D.ZAxis);
            var childToParent = new CoordinateSystem(new Point3D(0, 0, 5), -1 * UnitVector3D.YAxis, 1 * UnitVector3D.XAxis, 1 * UnitVector3D.ZAxis);

            tree.UpdateTransformation("root", "parent", rootToParent);
            tree.UpdateTransformation("child", "parent", childToParent);

            // try quarying
            Assert.IsTrue(rootToParent.Equals(tree.QueryTransformation("root", "parent")));
            Assert.IsTrue(childToParent.Equals(tree.QueryTransformation("child", "parent")));
            // try the chaining
            Assert.IsTrue(childToParent.Invert().TransformBy(rootToParent).Equals(tree.QueryTransformation("root", "child")));
            Assert.IsTrue(childToParent.Invert().TransformBy(rootToParent).Invert().Equals(tree.QueryTransformation("child", "root")));
        }

        [TestMethod]
        public void TestChainingAddMiddleLater()
        {
            var tree = new TransformationTree<string>();

            var rootToParent = new CoordinateSystem(new Point3D(5, 0, 0), UnitVector3D.XAxis, UnitVector3D.YAxis, UnitVector3D.ZAxis);
            var parentToChild = new CoordinateSystem(new Point3D(0, 0, 3), -1 * UnitVector3D.YAxis, 1 * UnitVector3D.XAxis, 1 * UnitVector3D.ZAxis);
            var childToGrand = new CoordinateSystem(new Point3D(1,0,0), -1 * UnitVector3D.YAxis, 1 * UnitVector3D.XAxis, 1 * UnitVector3D.ZAxis);

            tree.UpdateTransformation("root", "parent", rootToParent);
            tree.UpdateTransformation("child", "grand", childToGrand);
            tree.UpdateTransformation("parent", "child", parentToChild);

            // try quarying
            Assert.IsTrue(childToGrand.TransformBy(parentToChild).TransformBy(rootToParent).Equals(tree.QueryTransformation("root", "grand")));
        }

    }
}

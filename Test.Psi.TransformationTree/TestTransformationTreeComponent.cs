
namespace Test.Psi.TransformationTree
{
    using System;
    using MathNet.Spatial.Euclidean;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Psi;
    using TBD.Psi.TransformationTree;

    [TestClass]
    public class TestTransformationTreeComponent
    {
        [TestMethod]
        public void TestTreePublishing()
        {
            using(var p = Pipeline.Create())
            {
                var tree = new TransformationTreeComponent(p, 900);
                tree.UpdateTransformation("world", "map", new CoordinateSystem());
                int times = 0;
                DateTime startTime = DateTime.Now;
                tree.Do( (m,e) =>
                {
                    if (times == 0)
                    {
                        times++;
                        startTime = e.OriginatingTime;
                        Assert.IsTrue(m.Contains("world"));
                        Assert.IsTrue(m.Contains("map"));
                        Assert.IsFalse(m.Contains("robot"));
                        tree.UpdateTransformation("world", "robot", new CoordinateSystem());
                    }
                    else if (times == 1)
                    {

                        Assert.AreEqual(900, (e.OriginatingTime - startTime).Ticks * 0.0001, 10);
                        times++;
                        Assert.IsTrue(m.Contains("world"));
                        Assert.IsTrue(m.Contains("map"));
                        Assert.IsTrue(m.Contains("robot"));
                    }
                    else
                    {
                        Assert.Fail("TransformationComponent published more than expected.");
                    }
                });
                p.Run(new ReplayDescriptor(DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(2)));
            }
        }

        [TestMethod]
        public void TestTreeLinks()
        {
            using (var p = Pipeline.Create())
            {
                var tree = new TransformationTreeComponent(p, 500);

                var t1 = Generators.Range(p, 1, 2, TimeSpan.FromSeconds(900));
                var t2 = Generators.Range(p, 5, 2, TimeSpan.FromSeconds(900));
                var t1cs = t1.Delay(TimeSpan.FromMilliseconds(900)).Select(m =>
                {
                    var cs = new CoordinateSystem(new Point3D(m, 0, 0), UnitVector3D.XAxis, UnitVector3D.YAxis, UnitVector3D.ZAxis);
                    return ("map", "r1", cs);
                });
                var t2cs = t2.Delay(TimeSpan.FromMilliseconds(900)).Select(m =>
                {
                    var cs = new CoordinateSystem(new Point3D(0, m, 0), UnitVector3D.XAxis, UnitVector3D.YAxis, UnitVector3D.ZAxis);
                    return ("map", "r2", cs);
                });

                tree.AddTransformationUpdateLink(t1cs);
                tree.AddTransformationUpdateLink(t2cs);

                tree.Do((m, e) =>
                {
                   if ((p.GetCurrentTime() - p.StartTime) < TimeSpan.FromSeconds(0.7))
                   {
                       Assert.IsFalse(m.Contains("map"));
                       Assert.IsFalse(m.Contains("r1"));
                       Assert.IsFalse(m.Contains("r2"));
                   }
                   else if ((p.GetCurrentTime() - p.StartTime) < TimeSpan.FromSeconds(1.7))
                   {
                       Assert.IsTrue(m.Contains("map"));
                       Assert.IsTrue(m.Contains("r1"));
                       Assert.IsTrue(m.Contains("r2"));
                       var cs = m.QueryTransformation("map", "r1");
                       Assert.AreEqual(cs.Origin.X, 1);
                   }
                   else if ((p.GetCurrentTime() - p.StartTime) < TimeSpan.FromSeconds(2.5))
                   {
                       var cs = m.QueryTransformation("map", "r1");
                       Assert.AreEqual(cs.Origin.X, 2);
                       var cs2 = m.QueryTransformation("map", "r2");
                       Assert.AreEqual(cs.Origin.Y, 6);
                   }
                   else
                   {
                       Assert.Fail("TransformationComponent published more than expected.");
                   }
                });
                p.Run(new ReplayDescriptor(DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(2)));
            }
        }

    }
}
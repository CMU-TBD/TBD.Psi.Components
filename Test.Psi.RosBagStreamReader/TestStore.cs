namespace Test.Psi.RosBagStreamReader
{
    using Microsoft.Psi;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using TBD.Psi.RosBagStreamReader;

    [TestClass]
    public class TestStore
    {
        [TestMethod]
        public void TestSimpleRosBagMetaData()
        {
            using (var p = Pipeline.Create())
            {
                // open importer
                var store = RosBagStore.Open(p, "basic_string.bag", "TestBags");
                // validate store information
                Assert.AreEqual(2, store.AvailableStreams.Count());
                Assert.IsTrue(store.AvailableStreams.Select(s => s.Name).Contains("/text"));
            }
        }

        [TestMethod]
        public void TestSimpleRosBagRead()
        {
            using (var p = Pipeline.Create())
            {
                // open importer
                var store = RosBagStore.Open(p, "basic_string.bag", "TestBags");
                // read the data
                var stream = store.OpenStream<string>("/text");
                var i = 3;
                stream.Do(m =>
                {
                    Assert.AreEqual($"Hello {i++}", m);
                });
                p.Run(ReplayDescriptor.ReplayAll);
            }
        }
    }
}

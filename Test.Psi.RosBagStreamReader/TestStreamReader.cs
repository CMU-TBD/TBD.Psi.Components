namespace Test.Psi.RosBagStreamReader
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using TBD.Psi.RosBagStreamReader;

    [TestClass]
    public class TestStreamReader
    {
        [TestMethod]
        public void TestSimpleRosBag()
        {
            // open steam reader
            var streamReader = new RosBagStreamReader("basic_string.bag", "TestBags");
            // check if it knows there are two streams & names are correct
            Assert.AreEqual(streamReader.AvailableStreams.Count(), 1);
            string[] expectedTopicList = { "/text" };
            CollectionAssert.AreEqual( streamReader.AvailableStreams.Select(m => m.Name).ToList(), expectedTopicList);
        }

        [TestMethod]
        public void TestStreamOpen()
        {
            // open steam reader
            var streamReader = new RosBagStreamReader("basic_string.bag", "TestBags");
            streamReader.OpenStream<string>("/text", (s, e) =>
            {
                Assert.IsTrue(s == "hello");
            });
        }

        [TestMethod]
        public void TestStreamCount()
        {
            // open steam reader
            var streamReader = new RosBagStreamReader("basic_string.bag", "TestBags");
            Assert.AreEqual(streamReader.StreamCount, 1);
        }

        [TestMethod]
        public void TestStreamMessageCount()
        {
            // open steam reader
            var streamReader = new RosBagStreamReader("basic_string.bag", "TestBags");
            Assert.AreEqual(streamReader.GetStreamMetadata("/text").MessageCount, 56);
            Assert.AreEqual(streamReader.GetStreamMetadata("/text").AverageMessageSize, 0);
            Assert.AreEqual(streamReader.GetStreamMetadata("/text").AverageMessageLatencyMs, 0);
        }

        [TestMethod]
        public void TestReadingMultipleBags()
        {
            // open steam reader
            var streamReader = new RosBagStreamReader("sample_image_0.bag", "TestBags");
            Assert.AreEqual(streamReader.GetStreamMetadata("/usb_cam/image_raw/compressed").MessageCount, 24);
            Assert.AreEqual(streamReader.StreamTimeInterval.Span.TotalSeconds, 0.76, 0.1);
        }

    }
}

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
            string[] expectedTopicList = { "/rosout", "/text" };
            CollectionAssert.AreEquivalent(expectedTopicList, streamReader.AvailableStreams.Select(m => m.Name).ToList());
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
            Assert.AreEqual(2, streamReader.StreamCount);
        }

        [TestMethod]
        public void TestStreamMessageCount()
        {
            // open steam reader
            var streamReader = new RosBagStreamReader("basic_string.bag", "TestBags");
            Assert.AreEqual(56, streamReader.GetStreamMetadata("/text").MessageCount);
            Assert.AreEqual(0, streamReader.GetStreamMetadata("/text").AverageMessageSize);
            Assert.AreEqual(0, streamReader.GetStreamMetadata("/text").AverageMessageLatencyMs);
        }

        [TestMethod]
        public void TestReadingMultipleBags()
        {
            // open steam reader
            var streamReader = new RosBagStreamReader("sample_image_0.bag", "TestBags");
            Assert.AreEqual(24, streamReader.GetStreamMetadata("/usb_cam/image_raw/compressed").MessageCount);
            Assert.AreEqual(0.76, streamReader.StreamTimeInterval.Span.TotalSeconds, 0.1);
        }

    }
}

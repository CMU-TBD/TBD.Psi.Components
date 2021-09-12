
namespace Test.TBD.Psi.Imaging.Windows
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Psi.Imaging;
    using global::TBD.Psi.Imaging.Windows;
    using System;

    [TestClass]
    public class EncoderTests
    {
        private Image testImage;

        public EncoderTests()
        {
            this.testImage = Image.FromBitmap(Properties.Resources.test);
        }

        [TestMethod]
        public void TestJPEGTurboEncoding()
        {
            // encode into encoded image
            var encoder = new ImageToJpegTurboStreamEncoder();
            var encodedImage = this.testImage.Encode(encoder);
            var decodedImage = encodedImage.Decode(new ImageFromStreamDecoder());
            this.AssertAreImagesEqual(this.testImage, decodedImage);
        }

        [TestMethod]
        public void TestEncodingSpeed()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            // encode into encoded image
            for (var i = 0; i < 10; i++)
            {
                var encoder = new ImageToJpegTurboStreamEncoder();
                var encodedImage = this.testImage.Encode(encoder);
            }
            var t1 = watch.ElapsedMilliseconds;
            Console.WriteLine($"Turbo - Total:{watch.ElapsedMilliseconds} P/I:{watch.ElapsedMilliseconds / 10.0}");
            watch = System.Diagnostics.Stopwatch.StartNew();
            // encode into encoded image
            for (var i = 0; i < 10; i++)
            {
                var encoder = new ImageToJpegStreamEncoder();
                var encodedImage = this.testImage.Encode(encoder);
            }
            var t2 = watch.ElapsedMilliseconds;
            Console.WriteLine($"Original - Total:{watch.ElapsedMilliseconds} P/I:{watch.ElapsedMilliseconds / 10.0}");
            watch = System.Diagnostics.Stopwatch.StartNew();
            // encode into encoded image
            for (var i = 0; i < 10; i++)
            {
                var encoder = new ImageToJpegImageSharpStreamEncoder();
                var encodedImage = this.testImage.Encode(encoder);
            }
            var t3 = watch.ElapsedMilliseconds;
            Console.WriteLine($"Sharp - Total:{watch.ElapsedMilliseconds} P/I:{watch.ElapsedMilliseconds / 10.0}");
        }


        // This is copied from Microsoft/Psi. It was licensed under MIT.
        private void AssertAreImagesEqual(ImageBase referenceImage, ImageBase subjectImage, double tolerance = 6.0, double percentOutliersAllowed = 0.01)
        {
            ImageError err = new ImageError();
            Assert.AreEqual(referenceImage.Stride, subjectImage.Stride); // also check for consistency in the strides of allocated Images
            Assert.IsTrue(
                referenceImage.Compare(subjectImage, tolerance, percentOutliersAllowed, ref err),
                $"Max err: {err.MaxError}, Outliers: {err.NumberOutliers}");
        }
    }
}

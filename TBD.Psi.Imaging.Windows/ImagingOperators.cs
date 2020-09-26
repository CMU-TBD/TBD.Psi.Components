
namespace TBD.Psi.Imaging.Windows
{
    using Microsoft.Psi;
    using Microsoft.Psi.Imaging;
    using System;
    public static partial class ImagingOperators
    {
        /// <summary>
        /// Encodes an image to a JPEG format using libjpeg-Turbo
        /// </summary>
        /// <param name="source">A producer of images to encode.</param>
        /// <param name="quality">JPEG quality to use.</param>
        /// <param name="deliveryPolicy">An optional delivery policy.</param>
        /// <returns>A producer that generates the JPEG images.</returns>
        public static IProducer<Shared<EncodedImage>> EncodeJpegTurbo(this IProducer<Shared<Image>> source, int quality = 90, DeliveryPolicy<Shared<Image>> deliveryPolicy = null)
        {
            return source.Encode(new ImageToJpegStreamEncoder { QualityLevel = quality }, deliveryPolicy);
        }
    }
}

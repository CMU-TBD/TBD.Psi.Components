
namespace TBD.Psi.Imaging.Windows
{
    using Microsoft.Psi;
    using Microsoft.Psi.Imaging;
    using System;
    using System.Windows.Media.Imaging;
    using ImageSharpImage = SixLabors.ImageSharp.Image;
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
            return source.Encode(new ImageToJpegTruboStreamEncoder { QualityLevel = quality }, deliveryPolicy);
        }

        public static IProducer<Shared<EncodedImage>> EncodeJpegImageSharp(this IProducer<Shared<Image>> source, int quality = 90, DeliveryPolicy<Shared<Image>> deliveryPolicy = null)
        {
            return source.Encode(new ImageToJpegImageSharpStreamEncoder { QualityLevel = quality }, deliveryPolicy);
        }


        public static ImageSharpImage ToImageSharpImage(this Image image)
        {
            unsafe 
            {
                var dataSpan = new Span<byte>(image.ImageData.ToPointer(), image.Size);
                switch (image.PixelFormat)
                {
                    case PixelFormat.Gray_8bpp:
                        return ImageSharpImage.LoadPixelData<SixLabors.ImageSharp.PixelFormats.L8>(dataSpan, image.Width, image.Height);
                    case PixelFormat.Gray_16bpp:
                        return ImageSharpImage.LoadPixelData<SixLabors.ImageSharp.PixelFormats.L16>(dataSpan, image.Width, image.Height);
                    case PixelFormat.BGR_24bpp:
                        return ImageSharpImage.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Bgr24>(dataSpan, image.Width, image.Height);
                    case PixelFormat.BGRA_32bpp:
                        return ImageSharpImage.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Bgra32>(dataSpan, image.Width, image.Height);
                    case PixelFormat.BGRX_32bpp:
                        return ImageSharpImage.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Bgra32>(dataSpan, image.Width, image.Height);
                    case PixelFormat.RGBA_64bpp:
                        return ImageSharpImage.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Rgba64>(dataSpan, image.Width, image.Height);
                    default:
                        throw new BadImageFormatException("Cannot convert");
                        

                }
            }
        }
    }
}

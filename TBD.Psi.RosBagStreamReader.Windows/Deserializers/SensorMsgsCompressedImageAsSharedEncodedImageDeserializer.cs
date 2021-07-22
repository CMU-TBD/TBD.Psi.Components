namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System;
    using System.Text;
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Imaging;
    using System.IO;
    using TBD.Psi.RosBagStreamReader.Deserializers.SensorMsgs;

    public class SensorMsgsCompressedImageAsSharedEncodedImageDeserializer : MsgDeserializer
    {
        IImageToStreamEncoder pngEncoder;
        IImageToStreamEncoder jpegEncoder;
        public SensorMsgsCompressedImageAsSharedEncodedImageDeserializer(bool useHeaderTime)
            : base(typeof(Shared<EncodedImage>).AssemblyQualifiedName, "sensor_msgs/CompressedImage", useHeaderTime)
        {
            this.pngEncoder = new ImageToPngStreamEncoder();
            this.jpegEncoder = new ImageToJpegStreamEncoder();
        }

        private PixelFormat convertPixelFormat(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    return PixelFormat.Gray_8bpp;
                case System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:
                    return PixelFormat.Gray_16bpp;
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return PixelFormat.BGR_24bpp;
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return PixelFormat.BGRX_32bpp;
                default:
                    return PixelFormat.Undefined;
            }
        }

        private (PixelFormat, IImageToStreamEncoder) ParseEncodedFormat(string format)
        {
            // assuming it has the format "[ORIGINAL_ENCODING] ; [ENCODED_FORMAT] compressed"
            var originalFormat = format.Split(';')[0].ToLower();
            var encodedFormat = format.Split(';')[1].Trim().Split(' ')[0];
            IImageToStreamEncoder decoder = null;
            switch (encodedFormat.ToLower())
            {
                case "png":
                    decoder = this.pngEncoder;
                    break;
                case "jpeg":
                case "jpg":
                    decoder = this.jpegEncoder;
                    break;
                default:
                    throw new NotSupportedException($"Unsupported format {format}");
            }

            return (SensorMsgsHelper.EncodingToPsiPixelFormat(originalFormat), decoder);
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // read the header and get location
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out var offset, 0);
            this.UpdateEnvelope(ref env, originTime);

            var formatStrLength = (int)BitConverter.ToUInt32(data, offset);
            var format = Encoding.UTF8.GetString(data, offset + 4, formatStrLength);
            // parse the format
            (var originalFormat, var encoder) = this.ParseEncodedFormat(format);

            var dataArr = data.Skip(offset + formatStrLength + 4 + 4).ToArray();
            var imageMemoryStream = new MemoryStream(dataArr);
            using (var image = System.Drawing.Image.FromStream(imageMemoryStream))
            {
                var bitmap = new System.Drawing.Bitmap(image);
                using (var sharedImage = ImagePool.GetOrCreate(image.Width, image.Height, originalFormat))
                {
                    using (var encodedImage = EncodedImagePool.GetOrCreate(image.Width, image.Height, originalFormat))
                    {
                        //create the shared image
                        sharedImage.Resource.CopyFrom(bitmap);
                        encodedImage.Resource.EncodeFrom(sharedImage.Resource, encoder);
                        return (T)(object)encodedImage.AddRef();
                    }
                }
            }
        }
    }
}

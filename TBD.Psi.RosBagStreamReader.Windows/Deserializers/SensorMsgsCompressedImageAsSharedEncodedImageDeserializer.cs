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


        private (PixelFormat, IImageToStreamEncoder) ParseEncodedFormat(string format)
        {
            (var original, var encoded) = Windows.Deserializers.Helper.ParseRosCompressedFormatString(format);
            IImageToStreamEncoder decoder;
            switch (encoded.ToLower())
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

            return (SensorMsgsHelper.EncodingToPsiPixelFormat(original), decoder);
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // read the header and get location
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out var offset, 0);
            this.UpdateEnvelope(ref env, originTime);

            var format = Helper.ReadRosBaseType<string>(data, out offset, offset);
            // parse the format
            (var originalFormat, var encoder) = this.ParseEncodedFormat(format);
            // skip first 4 bytes which is the array length.
            var dataArr = data.Skip(offset + 4).ToArray();
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

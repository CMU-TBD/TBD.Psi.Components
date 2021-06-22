namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System;
    using System.Text;
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Imaging;
    using System.IO;

    public class SensorMsgsCompressedImageDeserializer : MsgDeserializer
    {
        public SensorMsgsCompressedImageDeserializer(bool useHeaderTime)
            : base(typeof(Shared<Image>).AssemblyQualifiedName, "sensor_msgs/CompressedImage", useHeaderTime)
        { 
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

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // read the header and get location
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out var offset, 0);
            this.UpdateEnvelope(ref env, originTime);

            var formatStrLength = (int)BitConverter.ToUInt32(data, offset);
            var format = Encoding.UTF8.GetString(data, offset + 4, formatStrLength);

            var dataArr = data.Skip(offset + formatStrLength + 4 + 4).ToArray();
            var imageMemoryStream = new MemoryStream(dataArr);
            using (var image = System.Drawing.Image.FromStream(imageMemoryStream))
            {
                var bitmap = new System.Drawing.Bitmap(image);
                using (var sharedImage = ImagePool.GetOrCreate(image.Width, image.Height, convertPixelFormat(image.PixelFormat)))
                {
                    sharedImage.Resource.CopyFrom(bitmap);
                    return (T) (object) sharedImage.AddRef();
                }
            }
        }
    }
}

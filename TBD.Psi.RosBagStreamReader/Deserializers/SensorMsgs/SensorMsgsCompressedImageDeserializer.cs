namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System;
    using System.Text;
    using System.IO;
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Imaging;
    using TBD.Psi.RosBagStreamReader.Deserializers.SensorMsgs;

    public class SensorMsgsCompressedImageDeserializer : MsgDeserializer
    {
        public SensorMsgsCompressedImageDeserializer(bool useHeaderTime)
            : base(typeof(Shared<Image>).AssemblyQualifiedName, "sensor_msgs/CompressedImage", useHeaderTime)
        { 
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
                using (var sharedImage = ImagePool.GetOrCreate(image.Width, image.Height, SensorMsgsHelper.SystemPixelFormatToPsiPixelFormat(image.PixelFormat)))
                {
                    sharedImage.Resource.CopyFrom(bitmap);
                    return (T) (object) sharedImage.AddRef();
                }
            }
        }
    }
}

namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System;
    using System.Text;
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Imaging;
    using System.IO;

    public class SensorMsgsCompressedImageDeserializer : IDeserializer
    {
        public T deserialize<T>(byte[] data)
        {
            var postHeaderPoint = Helper.PostHeaderPosition(data);

            var formatStrLength = (int)BitConverter.ToUInt32(data, postHeaderPoint);
            var format = Encoding.UTF8.GetString(data, postHeaderPoint + 4, formatStrLength);

            var dataArr = data.Skip(postHeaderPoint + formatStrLength + 4 + 4).ToArray();
            var imageMemoryStream = new MemoryStream(dataArr);
            using (var image = System.Drawing.Image.FromStream(imageMemoryStream))
            {
                var bitmap = new System.Drawing.Bitmap(image);
                using (var sharedImage = ImagePool.GetOrCreate(image.Width, image.Height, PixelFormat.BGR_24bpp))
                {
                    sharedImage.Resource.CopyFrom(bitmap);
                    return (T) (object) sharedImage.AddRef();
                }
            }
        }

        public string getAssemblyName()
        {
            return typeof(Shared<Image>).AssemblyQualifiedName;
        }

        public string getMessageTypeName()
        {
            return "sensor_msgs/CompressedImage";
        }
    }
}

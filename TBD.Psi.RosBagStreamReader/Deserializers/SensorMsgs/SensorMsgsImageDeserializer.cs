using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    public class SensorMsgsImageDeserializer : IDeserializer
    {
        private PixelFormat EncodingToPixelFormat(string encoding)
        {
            switch (encoding.ToUpper())
            {
                case ("BGR8"): return PixelFormat.BGR_24bpp;
                case ("RGB8"): return PixelFormat.BGR_24bpp;
                case ("BGRA8"): return PixelFormat.BGRA_32bpp;
                case ("MONO8"): return PixelFormat.Gray_8bpp;
                case ("MONO16"): return PixelFormat.Gray_16bpp;
                case ("RGBA16"): return PixelFormat.RGBA_64bpp;
                default: return PixelFormat.Undefined;
            }
        }

        public T deserialize<T>(byte[] data)
        {
            // get the location of the first byte after header
            var info_index = Helper.PostHeaderPosition(data);

            var height = (int) BitConverter.ToUInt32(data, info_index);
            var width = (int) BitConverter.ToUInt32(data, info_index + 4);
            var encodingStrLength = (int) BitConverter.ToUInt32(data, info_index + 8);
            var encoding = Encoding.UTF8.GetString(data, info_index + 12, encodingStrLength);
            // skip straight to the front of the array.
            var imgData = data.Skip(info_index + 12 + 1 + 4 + 4 + encodingStrLength).ToArray();

            var format = this.EncodingToPixelFormat(encoding);
            if (format == PixelFormat.Undefined)
            {
                Console.WriteLine($"Image Encoding Type {encoding} is not supported. Defaulting to writeout");
                throw new NotSupportedException($"Image Encoding Type {encoding} is not supported");
            }

            using (var sharedImage = ImagePool.GetOrCreate(width, height, format))
            {
                // skip the first 4 bytes because in ROS Message its a varied length array where the first 4 bytes tell us the length.
                sharedImage.Resource.CopyFrom(imgData);
                return (T) (Object) sharedImage.AddRef();
            }
        }

        public string getAssemblyName()
        {
            return typeof(Shared<Image>).AssemblyQualifiedName;
        }

        public string getMessageTypeName()
        {
            return "sensor_msgs/Image";
        }
    }
}

using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    public class SensorMsgsImageDeserializer : MsgDeserializer
    {
        public SensorMsgsImageDeserializer(bool useHeaderTime)
            : base(typeof(Shared<Image>).AssemblyQualifiedName, "sensor_msgs/Image", useHeaderTime)
        {
        }
        private PixelFormat EncodingToPixelFormat(string encoding)
        {
            switch (encoding.ToUpper())
            {
                case "BGR8": 
                    return PixelFormat.BGR_24bpp;
                case "RGB8": 
                    return PixelFormat.RGB_24bpp;
                case "BGRA8": 
                    return PixelFormat.BGRA_32bpp;
                case "MONO8":
                case "8UC1":
                    return PixelFormat.Gray_8bpp;
                case "16UC1":
                case "MONO16": 
                    return PixelFormat.Gray_16bpp;
                case "RGBA16": 
                    return PixelFormat.RGBA_64bpp;
                case "8UC3":
                    Console.WriteLine($"Image Encoding Type {encoding} has no defined RGB ordering. Defaulting to BGR");
                    return PixelFormat.BGR_24bpp;
                case "16UC4":
                    Console.WriteLine($"Image Encoding Type {encoding} has no defined RGBA ordering. Defaulting to BGRA");
                    return PixelFormat.BGRA_32bpp;
                default: 
                    return PixelFormat.Undefined;
            }
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {

            // read the header and get location
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out var infoIndex, 0);
            this.UpdateEnvelope(ref env, originTime);

            var height = (int) BitConverter.ToUInt32(data, infoIndex);
            var width = (int) BitConverter.ToUInt32(data, infoIndex + 4);
            var encodingStrLength = (int) BitConverter.ToUInt32(data, infoIndex + 8);
            var encoding = Encoding.UTF8.GetString(data, infoIndex + 12, encodingStrLength);
            // skip straight to the front of the array.
            var imgData = data.Skip(infoIndex + 12 + 1 + 4 + 4 + encodingStrLength).ToArray();

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
    }
}

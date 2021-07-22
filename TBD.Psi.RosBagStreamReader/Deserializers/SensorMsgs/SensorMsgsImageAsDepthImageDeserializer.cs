namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System;
    using System.Text;
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Imaging;
    using TBD.Psi.RosBagStreamReader.Deserializers.SensorMsgs;

    public class SensorMsgsImageAsDepthImageDeserializer : MsgDeserializer
    {
        public SensorMsgsImageAsDepthImageDeserializer(bool useHeaderTime)
            : base(typeof(Shared<DepthImage>).AssemblyQualifiedName, "sensor_msgs/Image", useHeaderTime)
        {
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

            var format = SensorMsgsHelper.EncodingToPsiPixelFormat(encoding);
            if (format == PixelFormat.Gray_8bpp)
            {
                // handle the image convert
                using (var image = ImagePool.GetOrCreate(width, height, format))
                {
                    image.Resource.CopyFrom(imgData);
                    var convertedImg = image.Resource.Convert(PixelFormat.Gray_16bpp);
                    using (var sharedDepthImage = DepthImagePool.GetOrCreate(width, height))
                    {
                        // skip the first 4 bytes because in ROS Message its a varied length array where the first 4 bytes tell us the length.
                        sharedDepthImage.Resource.CopyFrom(convertedImg);
                        return (T)(Object)sharedDepthImage.AddRef();
                    }
                }
            }
            if (format != PixelFormat.Gray_16bpp)
            {
                Console.WriteLine($"Image Encoding Type {encoding} is not supported. Defaulting to writeout");
                throw new NotSupportedException($"Image Encoding Type {encoding} is not supported");
            }

            using (var sharedDepthImage = DepthImagePool.GetOrCreate(width, height))
            {
                // skip the first 4 bytes because in ROS Message its a varied length array where the first 4 bytes tell us the length.
                sharedDepthImage.Resource.CopyFrom(imgData);
                return (T) (Object)sharedDepthImage.AddRef();
            }
        }
    }
}

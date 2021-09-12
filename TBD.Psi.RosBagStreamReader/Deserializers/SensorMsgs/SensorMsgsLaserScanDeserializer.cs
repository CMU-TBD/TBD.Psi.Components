namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System;
    using System.Text;
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Imaging;
    using TBD.Psi.RosBagStreamReader.Deserializers.SensorMsgs;
    using TBD.Psi.Sensors;

    public class SensorMsgsLaserScanDeserializer : MsgDeserializer
    {
        public SensorMsgsLaserScanDeserializer(bool useHeaderTime)
            : base(typeof(LaserScan2D).AssemblyQualifiedName, "sensor_msgs/LaserScan", useHeaderTime)
        {
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {

            // read the header and get location
            (_, var originTime, _) = Helper.ReadStdMsgsHeader(data, out var offset, 0);
            this.UpdateEnvelope(ref env, originTime);

            var angle_min = Helper.ReadRosBaseType<float>(data, out offset, offset);
            var angle_max = Helper.ReadRosBaseType<float>(data, out offset, offset);
            var angle_increment = Helper.ReadRosBaseType<float>(data, out offset, offset);
            var time_increment = Helper.ReadRosBaseType<float>(data, out offset, offset);
            var scan_time = Helper.ReadRosBaseType<float>(data, out offset, offset);
            var range_min = Helper.ReadRosBaseType<float>(data, out offset, offset);
            var range_max = Helper.ReadRosBaseType<float>(data, out offset, offset);
            var ranges = Helper.ReadRosBaseTypeArray<float>(data, out offset, offset);

            return (T)(Object)new LaserScan2D(ranges, angle_max, angle_min, angle_increment, TimeSpan.FromSeconds(scan_time), range_max, range_min);

        }
    }
}

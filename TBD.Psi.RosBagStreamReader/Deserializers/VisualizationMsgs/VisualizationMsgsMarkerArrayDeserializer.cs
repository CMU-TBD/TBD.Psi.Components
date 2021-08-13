namespace TBD.Psi.RosBagStreamReader.Deserializers
{
    using System;
    using System.Text;
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Imaging;
    using System.IO;
    using MathNet.Spatial.Euclidean;
    using MathNet.Numerics.LinearAlgebra;
    using System.Diagnostics;
    using System.Collections.Generic;

    public class VisualizationMsgsMarkerArrayDeserializer : MsgDeserializer
    {

        public VisualizationMsgsMarkerArrayDeserializer()
            : base(typeof(List<CoordinateSystem>).AssemblyQualifiedName, "visualization_msgs/MarkerArray")
        { 
        }

        public static List<CoordinateSystem> Deserialize(byte[] data, ref int offset)
        {
            /*  The following deserializer extracts a List of CoordinateSystems from the 
             *  visualization_msgs/MarkerArray ROS message type.
             */
            int size = Helper.ReadRosBaseType<Int32>(data, out offset, offset);
            List<CoordinateSystem> markers = new List<CoordinateSystem>(size);
            for (int i = 0; i < size; i++) {
                var marker = VisualizationMsgsMarkerDeserializer.Deserialize(data, ref offset);
                markers.Add(marker.pose);
            }
            return markers;
        }

        public override T Deserialize<T>(byte[] data, ref Envelope env)
        {
            // convert to coordinate systems
            int offset = 0;
            return (T)(object)Deserialize(data, ref offset);
        }
    }
}

// Copyright (c) Carnegie Mellon University. All rights reserved.
// Licensed under the MIT license.

namespace TBD.Psi.RosBagStreamReader.Windows.x64
{
    using TBD.Psi.RosBagStreamReader.Deserializers;

    public class RosBagReader: Windows.RosBagReader
    {
        public RosBagReader()
            : base()
        {
            // visualization_msgs
            this.AddDeserializer(new VisualizationMsgsMarkerArrayAsAzureKinectBodyListDeserializer(), "/body_tracking_data");
        }
    }
}



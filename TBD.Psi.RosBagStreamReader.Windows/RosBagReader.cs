// Copyright (c) Carnegie Mellon University. All rights reserved.
// Licensed under the MIT license.

namespace TBD.Psi.RosBagStreamReader.Windows
{
    using TBD.Psi.RosBagStreamReader.Deserializers;

    public class RosBagReader: TBD.Psi.RosBagStreamReader.RosBagReader
    {
        public RosBagReader()
            : base()
        {
            this.AddDeserializer(new SensorMsgsCompressedImageAsSharedEncodedImageDeserializer(true), "compressed");
        }
    }
}



// Copyright (c) Carnegie Mellon University. All rights reserved.
// Licensed under the MIT license.

namespace TBD.Psi.RosBagStreamReader
{
    using TBD.Psi.RosBagStreamReader.Deserializers;

    public class RosBagReaderWindows: RosBagReader
    {
        public RosBagReaderWindows()
            : base()
        {
            this.AddDeserializer(new SensorMsgsCompressedImageAsSharedEncodedImageDeserializer(true), "compressed");
        }
    }
}



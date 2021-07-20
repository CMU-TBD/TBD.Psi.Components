// Copyright (c) Carnegie Mellon University. All rights reserved.
// Licensed under the MIT license.

namespace TBD.Psi.RosBagStreamReader
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Psi;
    using Microsoft.Psi.Data;

    public sealed class RosBagImporter : Importer
    {
        public RosBagImporter(
            Pipeline pipeline,
            string name,
            string path,
            bool perStreamReader = false)
            : base(pipeline, new RosBagStreamReader(name, path, new RosBagReaderNET()), perStreamReader)
        {
        }

    }
}

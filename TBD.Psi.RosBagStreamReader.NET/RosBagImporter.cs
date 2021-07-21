// Copyright (c) Carnegie Mellon University. All rights reserved.
// Licensed under the MIT license.

namespace TBD.Psi.RosBagStreamReader
{
    using Microsoft.Psi;
    using Microsoft.Psi.Data;

    public sealed class RosBagImporter : Importer
    {
        public RosBagImporter(
            Pipeline pipeline,
            string name,
            string path,
            bool perStreamReader = false)
            : base(pipeline, new RosBagStreamReaderNET(name, path), perStreamReader)
        {
        }

    }
}

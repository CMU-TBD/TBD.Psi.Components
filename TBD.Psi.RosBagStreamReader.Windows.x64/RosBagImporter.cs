// Copyright (c) Carnegie Mellon University. All rights reserved.
// Licensed under the MIT license.

namespace TBD.Psi.RosBagStreamReader.Windows.x64
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
            : base(pipeline, new RosBagStreamReader(name, path), perStreamReader)
        {
        }
    }
}

// Copyright (c) Carnegie Mellon University. All rights reserved.
// Licensed under the MIT license.

namespace TBD.Psi.RosBagStreamReader.Windows.x64
{
    using Microsoft.Psi;
    public class RosBagStore
    {
        public static RosBagImporter Open(
            Pipeline pipeline,
            string name,
            string path)
        {
            return new RosBagImporter(pipeline, name, path);
        }
    }
}

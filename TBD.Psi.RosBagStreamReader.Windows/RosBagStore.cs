// Copyright (c) Carnegie Mellon University. All rights reserved.
// Licensed under the MIT license.

namespace TBD.Psi.RosBagStreamReader.Windows
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

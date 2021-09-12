// Copyright (c) Carnegie Mellon University. All rights reserved.
// Licensed under the MIT license.

namespace TBD.Psi.RosBagStreamReader.Windows
{
    using Microsoft.Psi.Data;

    [StreamReader("ROS Bags", ".bag")]
    public class RosBagStreamReader : TBD.Psi.RosBagStreamReader.RosBagStreamReader
    {
        public RosBagStreamReader(string name, string path)
            : base(name, path, new RosBagReader())
            {
            }
    }
}



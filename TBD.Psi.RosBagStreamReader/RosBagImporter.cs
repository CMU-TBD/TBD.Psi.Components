using System;
using System.Collections.Generic;
using System.Text;

namespace TBD.Psi.RosBagStreamReader
{
    using Microsoft.Psi;
    using Microsoft.Psi.Data;

    public sealed class RosBagImporter : Importer 
    {
        public RosBagImporter(
            Pipeline pipeline,
            string name,
            string path)
            :base (
                pipeline,
                new RosBagStreamReader(
                    name,
                    path))
        {
        }
                
    }
}

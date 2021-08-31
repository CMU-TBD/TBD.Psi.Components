using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBD.Psi.RosBagStreamReader.Windows.Deserializers
{
    public class Helper
    {
        public static (string original, string encoded) ParseRosCompressedFormatString(string input)
        {
            // assuming it has the format "[ORIGINAL_ENCODING] ; [ENCODED_FORMAT] compressed"
            var originalFormat = input.Split(';')[0].ToLower();
            var encodedFormat = input.Split(';')[1].Trim().Split(' ')[0];
            return (originalFormat, encodedFormat);
        }
    }
}

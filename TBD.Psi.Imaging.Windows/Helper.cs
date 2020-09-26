using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBD.Psi.Imaging.Windows
{
    using Microsoft.Psi.Imaging;
    using MediaPixelFormat = System.Drawing.Imaging.PixelFormat;
    public static class Helper
    {
        public static MediaPixelFormat ToSystemDrawingImagingPixelFormat(this PixelFormat pixelFormat)
        {
            if (pixelFormat == PixelFormat.Undefined)
            {
                throw new InvalidOperationException("Cannot convert the Undefined pixel format to a System.Drawing.Imaging format.");
            }
            else if (pixelFormat == PixelFormat.BGR_24bpp)
            {
                return MediaPixelFormat.Format24bppRgb;
            }
            else if (pixelFormat == PixelFormat.Gray_16bpp)
            {
                return System.Drawing.Imaging.PixelFormat.Format16bppGrayScale;
            }
            else if (pixelFormat == PixelFormat.Gray_8bpp)
            {
                return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
            }
            else
            {
                return MediaPixelFormat.Format32bppArgb;
            }
        }
    }
}

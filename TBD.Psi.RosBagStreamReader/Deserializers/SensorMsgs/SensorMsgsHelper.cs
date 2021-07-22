namespace TBD.Psi.RosBagStreamReader.Deserializers.SensorMsgs
{
    using System;
    using Microsoft.Psi.Imaging;

    public class SensorMsgsHelper
    {
        public static PixelFormat EncodingToPsiPixelFormat(string encoding)
        {
            switch (encoding.ToUpper())
            {
                case "BGR8":
                    return PixelFormat.BGR_24bpp;
                case "RGB8":
                    return PixelFormat.RGB_24bpp;
                case "BGRA8":
                    return PixelFormat.BGRA_32bpp;
                case "MONO8":
                case "8UC1":
                    return PixelFormat.Gray_8bpp;
                case "16UC1":
                case "MONO16":
                    return PixelFormat.Gray_16bpp;
                case "RGBA16":
                    return PixelFormat.RGBA_64bpp;
                case "8UC3":
                    Console.WriteLine($"Image Encoding Type {encoding} has no defined RGB ordering. Defaulting to BGR");
                    return PixelFormat.BGR_24bpp;
                case "16UC4":
                    Console.WriteLine($"Image Encoding Type {encoding} has no defined RGBA ordering. Defaulting to BGRA");
                    return PixelFormat.BGRA_32bpp;
                default:
                    return PixelFormat.Undefined;
            }
        }

        public static PixelFormat SystemPixelFormatToPsiPixelFormat(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    return PixelFormat.Gray_8bpp;
                case System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:
                    return PixelFormat.Gray_16bpp;
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return PixelFormat.BGR_24bpp;
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return PixelFormat.BGRX_32bpp;
                default:
                    return PixelFormat.Undefined;
            }
        }

    }
}

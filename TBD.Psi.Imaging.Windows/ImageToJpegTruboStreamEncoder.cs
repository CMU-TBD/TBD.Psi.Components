// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// Modified by Xiang Zhi Tan (CMU TBD Lab)

namespace TBD.Psi.Imaging.Windows
{
    using System.IO;
    using System.Drawing.Imaging;
    using Microsoft.Psi.Imaging;
    using System.Windows.Media.Imaging;
    using TurboJpegWrapper;

    /// <summary>
    /// Implements an image encoder for JPEG format.
    /// </summary>
    public class ImageToJpegTruboStreamEncoder : IImageToStreamEncoder
    {
        private TJCompressor compressor;

        public ImageToJpegTruboStreamEncoder()
        {
            this.compressor = new TJCompressor();
        }

        /// <summary>
        /// Gets or sets JPEG image quality (0-100).
        /// </summary>
        public int QualityLevel { get; set; } = 100;

        /// <inheritdoc/>
        public void EncodeToStream(Image image, Stream stream)
        {
            // compress
            var result = this.compressor.Compress(image.ImageData, image.Stride, image.Width, image.Height, 
                image.PixelFormat.ToSystemDrawingImagingPixelFormat(), TJSubsamplingOption.Chrominance444, this.QualityLevel, TJFlags.None);
            stream.Write(result, 0, result.Length);
        }
    }
}
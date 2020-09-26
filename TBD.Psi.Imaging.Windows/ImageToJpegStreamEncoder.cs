// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// Modified by Xiang Zhi Tan (CMU TBD Lab)

namespace TBD.Psi.Imaging.Windows
{
    using System.IO;
    using System.Windows.Media.Imaging;
    using Microsoft.Psi.Imaging;
    using TurboJpegWrapper;

    /// <summary>
    /// Implements an image encoder for JPEG format.
    /// </summary>
    public class ImageToJpegStreamEncoder : IImageToStreamEncoder
    {
        private TJCompressor compressor;
        private TJSubsamplingOption options;

        public ImageToJpegStreamEncoder()
        {
            this.compressor =  new TJCompressor();
        }

        /// <summary>
        /// Gets or sets JPEG image quality (0-100).
        /// </summary>
        public int QualityLevel { get; set; } = 100;

        /// <inheritdoc/>
        public void EncodeToStream(Image image, Stream stream)
        {
            var bitmapSource = BitmapSource.Create(
                image.Width,
                image.Height,
                96,
                96,
                image.PixelFormat.ToWindowsMediaPixelFormat(),
                null,
                image.ImageData,
                image.Stride * image.Height,
                image.Stride);
            
            // encode it
            var result = this.compressor.Compress(bitmapSource, this.options, this.QualityLevel, TJFlags.None);
            stream.Write(result, 0, result.Count());
        }
    }
}
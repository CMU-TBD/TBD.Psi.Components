// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// Modified by Xiang Zhi Tan (CMU TBD Lab)

namespace TBD.Psi.Imaging.Windows
{
    using System.IO;
    using Microsoft.Psi.Imaging;
    using SixLabors.ImageSharp.Formats.Jpeg;

    /// <summary>
    /// Implements an image encoder for JPEG format.
    /// </summary>
    public class ImageToJpegImageSharpStreamEncoder : IImageToStreamEncoder
    {
        private JpegEncoder encoder = new JpegEncoder();

        public ImageToJpegImageSharpStreamEncoder()
        {
        }

        /// <summary>
        /// Gets or sets JPEG image quality (0-100).
        /// </summary>
        public int QualityLevel { 
            get { return (int)this.encoder.Quality; } 
            set { this.encoder.Quality = value; } 
        }

        /// <inheritdoc/>
        public void EncodeToStream(Image image, Stream stream)
        {
            image.ToImageSharpImage().Save(stream, this.encoder);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ViDi2;

namespace ViDi2
{
    public class ByteImage : IImage
    {
        Byte[] data;
        int step;

        /// <summary>
        /// Initializes a newinstance with the given image dimensions and pixel-data
        /// </summary>
        /// <param name="width">the image width in pixel</param>
        /// <param name="height">the image height in pixel</param>
        /// <param name="channels">the number of color channels (1-4)</param>
        /// <param name="channelDepth">the bit-depth per channel (8 or 16)</param>
        /// <param name="data">pointer to the pixel data in unmanaged memory</param>
        /// <param name="step">the number of bytes per image row</param>
        public ByteImage(int width, int height, int channels,
                        ImageChannelDepth channelDepth,
                        Byte[] data, int step)
        {
            Width = width;
            Height = height;
            Channels = channels;
            ChannelDepth = channelDepth;

            this.data = data;
            this.step = step;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Channels { get; private set; }
        public ImageChannelDepth ChannelDepth { get; private set; }

        public System.Drawing.Bitmap Bitmap
        {
            get
            {
                throw new NotImplementedException("unsupported image format");
            }
        }

#if !__MonoCS__
        public System.Windows.Media.Imaging.BitmapSource BitmapSource
        {
            get
            {
                System.Windows.Media.PixelFormat pixFormat;

                if (Channels == 1)
                    pixFormat = ChannelDepth == ImageChannelDepth.Depth8 ?
                        System.Windows.Media.PixelFormats.Gray8 : System.Windows.Media.PixelFormats.Gray16;
                else if (Channels == 3)
                    pixFormat = ChannelDepth == ImageChannelDepth.Depth8 ?
                        System.Windows.Media.PixelFormats.Bgr24 : System.Windows.Media.PixelFormats.Rgb48;
                else if (Channels == 4)
                    pixFormat = ChannelDepth == ImageChannelDepth.Depth8 ?
                        System.Windows.Media.PixelFormats.Bgra32 : System.Windows.Media.PixelFormats.Rgba64;
                else
                    throw new NotSupportedException("unsupported image format");

                var  source =  System.Windows.Media.Imaging.BitmapSource.Create(Width, Height, 96.0, 96.0,
                                                 pixFormat, null, data, step);
                return source.Clone();

            }
        }
#endif

        class ImageLock : IImageLock
        {
            ByteImage parent;
            GCHandle pinnedArray;
            internal ImageLock(ByteImage parent) { this.parent = parent; }

            public int Step { get { return parent.step; } }

            public IntPtr PixelData
            {
                get
                {
                    pinnedArray = GCHandle.Alloc(parent.data, GCHandleType.Pinned);
                    IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                    return pointer;
                }
            }

            public void Dispose()
            {
                pinnedArray.Free();
            }
        }

        void IDisposable.Dispose() 
        {
            data = null;
        }

        public IImageLock Lock { get { return new ImageLock(this); } }

        public void Save(System.IO.Stream stream, ViDi2.ImageFormat imageFormat)
        {
            System.Windows.Media.Imaging.BitmapEncoder encoder;

            if (imageFormat == ImageFormat.JPEG)
                encoder = new System.Windows.Media.Imaging.JpegBitmapEncoder();
            else if (imageFormat == ImageFormat.PNG)
                encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            else if (imageFormat == ImageFormat.BMP)
                encoder = new System.Windows.Media.Imaging.BmpBitmapEncoder();
            else if (imageFormat == ImageFormat.BMP)
                encoder = new System.Windows.Media.Imaging.TiffBitmapEncoder();
            else if (imageFormat == ImageFormat.NATIVE)
            {
                stream.Write(data, 0, data.Length);
                return;
            }
            else
                throw new ArgumentException("unknown ImageFormat");

            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(BitmapSource));
            encoder.Save(stream);

        }
    }
}

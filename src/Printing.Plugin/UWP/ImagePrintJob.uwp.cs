#pragma warning disable IDE0007 // Use implicit type

using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Plugin.Printing
{
    /// <summary>
    /// UWP functionality related to printing an image
    /// </summary>
    /// <remarks>
    /// Based on code from bushbert
    /// See https://github.com/bushbert/XamarinPCLPrinting
    /// </remarks>
    internal sealed class ImagePrintJob : BasePrintJob
    {
        private readonly byte[] _imageData;
        private readonly Stream _imageStream;
        private readonly Windows.UI.Xaml.Controls.Image _nativeImage = new Windows.UI.Xaml.Controls.Image(); // TODO - rename this
        private bool _nativeImagePopulated;

        /// <summary>
        /// Test whether printing is supported on the current device
        /// </summary>
        public static bool ImagePrintingSupported => PrintingSupported;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="printJobConfiguration">Configuration information for the print job</param>
        /// <param name="imageData">Byte array containing image data</param>
        public ImagePrintJob(
            PrintJobConfiguration printJobConfiguration,
            byte[] imageData)
            : base(printJobConfiguration)
        {
            _imageData = imageData;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="printJobConfiguration">Configuration information for the print job</param>
        /// <param name="imageStream">Stream providing image data</param>
        public ImagePrintJob(
            PrintJobConfiguration printJobConfiguration,
            Stream imageStream)
            : base(printJobConfiguration)
        {
            _imageStream = imageStream;
        }

        /// <summary>
        /// Method that returns the UIElement for populating the specified preview page
        /// </summary>
        /// <param name="oneBasedPageNumber">The page number</param>
        /// <returns>UIElement used to populate the preview page</returns>
        public override Windows.UI.Xaml.UIElement GetNextPrintPreviewPage(int oneBasedPageNumber)
            => (!_nativeImagePopulated) || (oneBasedPageNumber != 1)
                ? null
                : _nativeImage;

        /// <summary>
        /// Get the number of pages to be printed
        /// </summary>
        /// <returns>The number of pages to be printed</returns>
        protected override int GetNumberOfPages() => _nativeImagePopulated ? 1 : 0;

        /// <summary>
        /// Perform the print
        /// </summary>
        /// <returns>Task that the caller is expected to await</returns>
        public override async Task PrintAsync()
        {
            if (_imageData != null)
            {
                BitmapImage biSource = new BitmapImage();
                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    await stream.WriteAsync(_imageData.AsBuffer());
                    stream.Seek(0);
                    await biSource.SetSourceAsync(stream);
                }

                _nativeImage.Source = biSource;
                _nativeImagePopulated = true;
                await ShowPrintApiAsync();
            }
            else if (_imageStream != null)
            {
                BitmapImage biSource = new BitmapImage();

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await _imageStream.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    await biSource.SetSourceAsync(memoryStream.AsRandomAccessStream());
                }

                _nativeImage.Source = biSource;
                _nativeImagePopulated = true;
                await ShowPrintApiAsync();
            }
        }

    } // internal sealed class ImagePrintJob : BasePrintJob

} // namespace Plugin.Printing

#pragma warning restore IDE0007 // Use implicit type

// eof

#pragma warning disable IDE0007 // Use implicit type

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.Globalization;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Plugin.Printing
{
    /// <summary>
    /// UWP functionality related to printing a PDF
    /// </summary>
    internal sealed class PdfPrintJob : BasePrintJob
    {
        private readonly Stream _inputStream;
        private int _pageCount;
        private global::Windows.Data.Pdf.PdfDocument _pdfDocument;
        private IRandomAccessStream _randomStream;
        private List<Windows.UI.Xaml.Controls.Image> _pageImages;

        /// <summary>
        /// Test whether printing is supported on the current device
        /// </summary>
        public static bool PdfPrintingSupported => PrintingSupported;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="printJobConfiguration">Configuration information for the print job</param>
        /// <param name="inputStream">Stream containing PDF data</param>
        public PdfPrintJob(
            PrintJobConfiguration printJobConfiguration,
            Stream inputStream)
            : base(printJobConfiguration)
        {
            _inputStream = inputStream ?? throw new PrintJobException("EXCEPTION:PRN007 Null stream", new ArgumentNullException(nameof(inputStream)));
        }

        /// <summary>
        /// Method that returns the UIElement for populating the specified preview page
        /// </summary>
        /// <param name="oneBasedPageNumber">The page number</param>
        /// <returns>UIElement used to populate the preview page</returns>
        public override Windows.UI.Xaml.UIElement GetNextPrintPreviewPage(int oneBasedPageNumber)
        {
            System.Diagnostics.Debug.Assert(oneBasedPageNumber >= 1);
            System.Diagnostics.Debug.Assert(!(_pageImages is null));
            System.Diagnostics.Debug.Assert(!(_pageImages[oneBasedPageNumber - 1] is null));
            return _pageImages[oneBasedPageNumber - 1];
        }

        /// <summary>
        /// Get the number of pages to be printed
        /// </summary>
        /// <returns>The number of pages to be printed</returns>
        protected override int GetNumberOfPages() => _pageCount;

        /// <summary>
        /// Perform the print
        /// </summary>
        /// <returns>Task that the caller is expected to await</returns>
        public override async Task PrintAsync()
        {
            if (!(_inputStream is null))
            {
                _inputStream.Position = 0;
                MemoryStream ms = new MemoryStream();
                _inputStream.CopyTo(ms);
                ms.Position = 0;

                _randomStream = await StreamHelper.ConvertToRandomAccessStream(ms);
                IAsyncOperation<Windows.Data.Pdf.PdfDocument> result = global::Windows.Data.Pdf.PdfDocument.LoadFromStreamAsync(_randomStream);
                result.AsTask().Wait();
                _pdfDocument = result.GetResults();
                //result = null;
                _pageCount = (int)_pdfDocument.PageCount;

                await GeneratePagesAsync();

                await base.ShowPrintApiAsync();
            }
        }

        /// <summary>
        /// Generate the pages to print
        /// </summary>
        /// <returns>Task that the caller is expected to await</returns>
        protected override async Task GeneratePagesAsync()
        {
            System.Diagnostics.Debug.Assert(_pageCount > 0);

            //StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
            await PrepareForPrintAsync(0, _pageCount /*, tempFolder*/);
        }

        /// <summary>
        /// Cache printable output
        /// </summary>
        /// <param name="p">Number of first page to print</param>
        /// <param name="count">Number of pages to print</param>
        /// <returns>Task that the caller is expected to await</returns>
        private async Task PrepareForPrintAsync(int p, int count /*, StorageFolder tempfolder*/)
        {
            if ((_pageImages is null) || (_pageImages.Count == 0))
            {
                ClearCachedPages();
                _pageImages = new List<Windows.UI.Xaml.Controls.Image>();
                _ = new Windows.UI.Xaml.Controls.Image();
                for (int i = p; i < count; i++)
                {
                    ApplicationLanguages.PrimaryLanguageOverride =
                        CultureInfo.InvariantCulture.TwoLetterISOLanguageName;
                    Windows.Data.Pdf.PdfPage pdfPage = _pdfDocument.GetPage(uint.Parse(i.ToString()));
                    double pdfPagePreferredZoom = pdfPage.PreferredZoom;
                    IRandomAccessStream randomStream = new InMemoryRandomAccessStream();
                    global::Windows.Data.Pdf.PdfPageRenderOptions pdfPageRenderOptions =
                        new global::Windows.Data.Pdf.PdfPageRenderOptions();
                    Windows.Foundation.Size pdfPageSize = pdfPage.Size;
                    pdfPageRenderOptions.DestinationHeight = (uint)(pdfPageSize.Height * pdfPagePreferredZoom);
                    pdfPageRenderOptions.DestinationWidth = (uint)(pdfPageSize.Width * pdfPagePreferredZoom);
                    await pdfPage.RenderToStreamAsync(randomStream, pdfPageRenderOptions);
                    Windows.UI.Xaml.Controls.Image imageCtrl = new Windows.UI.Xaml.Controls.Image();
                    BitmapImage src = new BitmapImage();
                    randomStream.Seek(0);
                    src.SetSource(randomStream);
                    imageCtrl.Source = src;
                    Windows.Graphics.Display.DisplayInformation DisplayInformation = Windows.Graphics.Display.DisplayInformation.GetForCurrentView();

                    float dpi = DisplayInformation.LogicalDpi / 96;
                    imageCtrl.Height = src.PixelHeight / dpi;
                    imageCtrl.Width = src.PixelWidth / dpi;
                    randomStream.Dispose();
                    pdfPage.Dispose();

                    _pageImages.Add(imageCtrl);
                    AddPageToCache(imageCtrl);
                }
            }
        }

    } // internal sealed class PdfPrintJob : BasePrintJob

} // namespace Plugin.Printing

#pragma warning restore IDE0007 // Use implicit type

// eof

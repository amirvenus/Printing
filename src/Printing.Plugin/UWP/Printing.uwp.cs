#pragma warning disable IDE0007 // Use implicit type

// This code based on https://docs.microsoft.com/en-us/windows/uwp/devices-sensors/print-from-your-app

using System;
using System.IO;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Plugin.Printing
{
    /// <summary>
    /// Interface for Printing
    /// </summary>
    internal class PrintingImplementation : BasePrintingImplementation
    {
        public override bool PrintFromURLAsyncSupported => true;
        public override bool PrintImageFromByteArrayAsyncSupported => ImagePrintJob.ImagePrintingSupported;
        public override bool PrintImageFromStreamAsyncSupported => ImagePrintJob.ImagePrintingSupported;
        public override bool PrintPdfFromStreamAsyncSupported => PdfPrintJob.PdfPrintingSupported;
        public override bool PrintWebViewAsyncSupported => WebViewPrintJob.WebViewPrintingSupported;

        public PrintingImplementation()
        {
            // no-op
        }

        public override async Task<bool> PrintImageFromByteArrayAsync(
            byte[] content,
            PrintJobConfiguration config)
        {
            bool result = false;
            try
            {
                if (PrintImageFromByteArrayAsyncSupported)
                {
                    ImagePrintJob printJob = new ImagePrintJob(config, content);
                    await printJob.PrintAsync();
                    result = true;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
                result = false;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return result;
        }

        public override async Task<bool> PrintImageFromStreamAsync(Stream stream, PrintJobConfiguration config)
        {
            bool result = false;
            try
            {
                if (PrintImageFromByteArrayAsyncSupported)
                {
                    ImagePrintJob printJob = new ImagePrintJob(config, stream);
                    await printJob.PrintAsync();
                    result = true;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
                result = false;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return result;
        }

        public override async Task<bool> PrintPdfFromStreamAsync(
            Stream inputStream,
            PrintJobConfiguration config)
        {
            bool result = false;
            try
            {
                if (PrintPdfFromStreamAsyncSupported)
                {
                    PdfPrintJob printJob = new PdfPrintJob(config, inputStream);
                    await printJob.PrintAsync();
                    result = true;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
                result = false;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return result;
        }

        public override async Task<bool> PrintWebViewAsync(
            Page page,
            WebView webView,
            IWebViewAdditionalFunctions webViewAdditionFunctions,
            PrintJobConfiguration config)
        {
            bool result = false;
            try
            {
                if (PrintWebViewAsyncSupported)
                {
                    if (!(webViewAdditionFunctions is null))
                    {
                        await webViewAdditionFunctions.WaitForViewToBeInNativeLayout(page, 10, 100);

                        WebViewPrintJob printJob = new WebViewPrintJob(config, webViewAdditionFunctions);
                        //WebViewPrintJob printJob = new WebViewPrintJob(config, html);
                        await printJob.PrintAsync();
                        result = true;
                    }
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
                result = false;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            // TODO - does this want to be here??? printJob?.UnwireAllEvents();

            return result;
        }

        public override async Task<bool> PrintFromURLAsync(string url, PrintJobConfiguration printJobConfiguration) // was string jobName, bool enablePrintPreview = true)
        {
            bool result = false;
            try
            {
                if (PrintFromURLAsyncSupported)
                {
                    if (!(url is null))
                    {
                        UriPrintJob printJob = new UriPrintJob(printJobConfiguration, new Uri(url));
                        await printJob.PrintAsync();
                        result = true;
                    }
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
                result = false;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            // TODO - does this want to be here??? printJob?.UnwireAllEvents();

            return result;
        }

    } // internal class PrintingImplementation : BasePrintingImplementation

} // namespace Plugin.Printing

#pragma warning restore IDE0007 // Use implicit type

// eof

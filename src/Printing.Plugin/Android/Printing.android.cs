// TODO - note that PDFs must be version 1.6+, otherwise Android says that they are malformed
// TODO - validate PDF version, or catch exception if possible

#pragma warning disable IDE0007 // Use implicit type

using System.IO;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Plugin.Printing
{
    /// <summary>
    /// Implementation of Printing for Android (build 19+)
    /// </summary>
    internal class PrintingImplementation : BasePrintingImplementation
    {
        public override bool PrintFromURLAsyncSupported => true;
        public override bool PrintImageFromByteArrayAsyncSupported => true;
        public override bool PrintImageFromStreamAsyncSupported => true;
        public override bool PrintPdfFromStreamAsyncSupported => true;
        public override bool PrintWebViewAsyncSupported => true;

        public override async Task<bool> PrintFromURLAsync(string url, PrintJobConfiguration printJobConfiguration)
        {
            bool result = false;

            try
            {
                if (PrintFromURLAsyncSupported)
                {
                    WebViewPrintClientSubclass webViewClientSubclass = new WebViewPrintClientSubclass(url, printJobConfiguration); // TODO - check that this is disposed
                    webViewClientSubclass.Run();
                    result = true;
                }
            }
            catch (Java.Lang.Exception jlex)
            {
                await PrintStatusReporting.ReportExceptionSilentlyAsync(jlex);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (System.Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return result;
        }

        public override async Task<bool> PrintImageFromByteArrayAsync(byte[] content, PrintJobConfiguration printJobConfiguration)
        {
            // based on https://github.com/bushbert/XamarinPCLPrinting/blob/master/PCLPrintExample/PCLPrintExample/PCLPrintExample.Android/Print.cs

            bool result = false;

            try
            {
                if (PrintImageFromByteArrayAsyncSupported)
                {
                    //using (
                    Stream inputStream = new MemoryStream(content); //)
                    {
                        PrintServiceAndroidHelper.PrintImageFromStream(inputStream, printJobConfiguration);
                        result = true;
                    }
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (System.Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return result;
        }

        public override async Task<bool> PrintImageFromStreamAsync(Stream inputStream, PrintJobConfiguration printJobConfiguration)
        {
            bool result = false;

            try
            {
                if (PrintImageFromStreamAsyncSupported)
                {
                    PrintServiceAndroidHelper.PrintImageFromStream(inputStream, printJobConfiguration);
                    result = true;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (System.Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return result;
        }

        public override async Task<bool> PrintPdfFromStreamAsync(Stream inputStream, PrintJobConfiguration printJobConfiguration)
        {
            bool result = false;

            try
            {
                if (PrintPdfFromStreamAsyncSupported)
                {
                    PrintServiceAndroidHelper.PrintFromStream(inputStream, printJobConfiguration, Android.Print.PrintContentType.Document);
                    result = true;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (System.Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return result;
        }

        public override async Task<bool> PrintWebViewAsync(
            Page parentPage,
            WebView webView,
            IWebViewAdditionalFunctions webViewAdditionFunctions,
            PrintJobConfiguration printJobConfiguration)
        {
            bool result = false;

            try
            {
                if (PrintWebViewAsyncSupported
                    && (webViewAdditionFunctions?.NativeControl is Android.Webkit.WebView nativeWebView))
                {
                    PrintServiceAndroidHelper.PrintFromWebView(nativeWebView, printJobConfiguration);
                    result = true;
                }
            }
            catch (Java.Lang.Exception jlex)
            {
                await PrintStatusReporting.ReportExceptionSilentlyAsync(jlex);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (System.Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return result;
        }

    } // internal class PrintingImplementation : BasePrintingImplementation

} // namespace Plugin.Printing

#pragma warning restore IDE0007 // Use implicit type

// eof

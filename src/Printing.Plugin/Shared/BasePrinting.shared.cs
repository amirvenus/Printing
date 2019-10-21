using System.IO;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Plugin.Printing
{
    public abstract class BasePrintingImplementation : IPrinting
    {
        public abstract bool PrintFromURLAsyncSupported { get; }
        public abstract bool PrintImageFromByteArrayAsyncSupported { get; }
        public abstract bool PrintImageFromStreamAsyncSupported { get; }
        public abstract bool PrintPdfFromStreamAsyncSupported { get; }
        public abstract bool PrintWebViewAsyncSupported { get; }

        public abstract Task<bool> PrintFromURLAsync(string url, PrintJobConfiguration config);
        public abstract Task<bool> PrintImageFromByteArrayAsync(byte[] content, PrintJobConfiguration config);
        public abstract Task<bool> PrintImageFromStreamAsync(Stream stream, PrintJobConfiguration config);
        public abstract Task<bool> PrintPdfFromStreamAsync(Stream stream, PrintJobConfiguration config);
        public abstract Task<bool> PrintWebViewAsync(Page parentPage, WebView webView, IWebViewAdditionalFunctions webViewAdditionFunctions, PrintJobConfiguration config);

    } // public abstract class BasePrintingImplementation : IPrinting

} // namespace Plugin.Printing

// eof

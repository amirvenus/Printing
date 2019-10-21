using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Plugin.Printing
{
    public interface IBasePrintingMessages
    {
        string AbandonedFormatString { get; }
        string AwaitingResponse { get; }
        string CanceledFormatString { get; }
        string DefaultFormatString { get; }
        string FailedFormatString { get; }
        string FailedToRegisterForPrinting { get; }
        string NullDescriptionString { get; }
        string PagesNotPresentErrorString { get; }
        string PagesNotPresentWarningText { get; }
        string PrinterServiceBusy { get; }
        string PrintInteractionCompleted { get; }
        string PrintInteractionError { get; }
        string SubmittedFormatString { get; }
        string UnableToPrintAtThisTime { get; }

    } // public interface IBasePrintingMessages

    // I am not providing an implementation of this in this NuGet. It is up to the user to provide their own implementation if they want to use WebView printing.
    // I might provide a sample implementation at a later date, but separate from this NuGet.
    public interface IWebViewAdditionalFunctions
    {
        object NativeControl { get; }

        Task<string> GetHtmlAsync();

        Task WaitForViewToBeInNativeLayout(
            Page page,
            int millisecondDelay,
            int maxCycles,
            CancellationToken? token = null);

    } // public interface IWebViewAdditionalFunctions

    public interface IPrinting
    {
        bool PrintFromURLAsyncSupported { get; }
        bool PrintImageFromByteArrayAsyncSupported { get; }
        bool PrintImageFromStreamAsyncSupported { get; }
        bool PrintPdfFromStreamAsyncSupported { get; }
        bool PrintWebViewAsyncSupported { get; }

        Task<bool> PrintFromURLAsync(string url, PrintJobConfiguration config);
        Task<bool> PrintImageFromByteArrayAsync(byte[] content, PrintJobConfiguration config);
        Task<bool> PrintImageFromStreamAsync(Stream stream, PrintJobConfiguration config);
        Task<bool> PrintPdfFromStreamAsync(Stream stream, PrintJobConfiguration config);
        Task<bool> PrintWebViewAsync(Page parentPage, WebView webView, IWebViewAdditionalFunctions webViewAdditionFunctions, PrintJobConfiguration config);

    } // public interface IPrinting

} // namespace Plugin.Printing

// eof

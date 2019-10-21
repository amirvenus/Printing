using System;
using System.Threading.Tasks;

namespace Plugin.Printing
{
    /// <summary>
    /// UWP functionality related to printing whatever is addressed by a URI.
    /// </summary>
    internal sealed class UriPrintJob : BasePrintJob
    {
        private readonly Windows.UI.Xaml.Controls.WebView _nativeWebView = new Windows.UI.Xaml.Controls.WebView();
        private readonly Uri _uri;

        /// <summary>
        /// Test whether printing is supported on the current device
        /// </summary>
        public static bool UriPrintingSupported => PrintingSupported;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="printJobConfiguration">Configuration information for the print job</param>
        /// <param name="uri">The URI addressing whatever is to be printed</param>
        public UriPrintJob(
            PrintJobConfiguration printJobConfiguration,
            Uri uri)
            : base(printJobConfiguration)
        {
            _uri = uri ?? throw new PrintJobException("EXCEPTION:PRN001 Invalid URL", new ArgumentNullException(nameof(uri)));
        }

        /// <summary>
        /// Method that returns the UIElement for populating the specified preview page
        /// </summary>
        /// <param name="oneBasedPageNumber">The page number</param>
        /// <returns>UIElement used to populate the preview page</returns>
        /// <remarks>This is currently a very simplistic implementation that only supports one page of output. This will hopefully be improved upon in future to support multiple pages</remarks>
        public override Windows.UI.Xaml.UIElement GetNextPrintPreviewPage(int oneBasedPageNumber)
        {
            if (oneBasedPageNumber != 1)
            {
                throw new PrintJobException(
                    $"EXCEPTION:PRN002 Unexpected page number ({oneBasedPageNumber}) requested");
            }

            return _nativeWebView;
        }

        /// <summary>
        /// Get the number of pages to be printed
        /// </summary>
        /// <returns>The number of pages to be printed</returns>
        /// <remarks>This is currently a very simplistic implementation that only supports one page of output. This will hopefully be improved upon in future to support multiple pages</remarks>
        protected override int GetNumberOfPages() => 1;

        /// <summary>
        /// Perform the print
        /// </summary>
        /// <returns>Task that the caller is expected to await</returns>
        public override Task PrintAsync()
        {
            Wire();

            _nativeWebView.Navigate(_uri);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Wire up event handlers
        /// </summary>
        private void Wire()
        {
            Unwire();
            _nativeWebView.DOMContentLoaded += NativeWebViewDOMContentLoaded;
            _nativeWebView.NavigationFailed += NativeWebViewNavigationFailed;
        }

        /// <summary>
        /// Unwire event handlers
        /// </summary>
        private void Unwire()
        {
            _nativeWebView.DOMContentLoaded -= NativeWebViewDOMContentLoaded;
            _nativeWebView.NavigationFailed -= NativeWebViewNavigationFailed;
        }

        /// <summary>
        /// Event handler that tidies up if navigation fails
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="args">The details of the navigation failure</param>
        private void NativeWebViewNavigationFailed(
#pragma warning disable IDE0060 // Remove unused parameter
            object sender,
            Windows.UI.Xaml.Controls.WebViewNavigationFailedEventArgs args)
#pragma warning restore IDE0060 // Remove unused parameter
            => Unwire();

        /// <summary>
        /// Event handler that shows the Print Preview dialog once the DOM content has been loaded for the URI
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="args">The event args related to the DOM content loading completion</param>
        private async void NativeWebViewDOMContentLoaded(
#pragma warning disable IDE0060 // Remove unused parameter
            Windows.UI.Xaml.Controls.WebView sender, 
            Windows.UI.Xaml.Controls.WebViewDOMContentLoadedEventArgs args)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            try
            {
                await ShowPrintApiAsync();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                Unwire();
            }
        }

    } // internal sealed class UriPrintJob : BasePrintJob

} // namespace Plugin.Printing

// eof

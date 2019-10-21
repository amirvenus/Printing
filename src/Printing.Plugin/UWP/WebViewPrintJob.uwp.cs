using System;
using System.Threading.Tasks;

namespace Plugin.Printing
{
    /// <summary>
    /// UWP functionality related to printing the content of a WebView
    /// </summary>
    internal sealed class WebViewPrintJob : BasePrintJob
    {
        private readonly Windows.UI.Xaml.Controls.WebView _nativeWebView;

        /// <summary>
        /// Test whether printing is supported on the current device
        /// </summary>
        public static bool WebViewPrintingSupported => PrintingSupported;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="printJobConfiguration">Configuration information for the print job</param>
        /// <param name="webViewAdditionalFunctions">Interface that provides access to functionality required by this class. Typically, this interface will be implemented by a subclass of Xamarin.Forms.WebView</param>
        public WebViewPrintJob(
            PrintJobConfiguration printJobConfiguration,
            IWebViewAdditionalFunctions webViewAdditionalFunctions)
            : base(printJobConfiguration)
        {
            if (webViewAdditionalFunctions is null)
            {
                throw new PrintJobException(
                    "EXCEPTION:PRN003 Invalid interface to native WebView",
                    new ArgumentNullException(nameof(webViewAdditionalFunctions)));
            }
            else if (webViewAdditionalFunctions.NativeControl is null)
            {
                throw new PrintJobException(
                    "EXCEPTION:PRN004 Invalid (null) native WebView");
            }
            else if (!(webViewAdditionalFunctions.NativeControl is Windows.UI.Xaml.Controls.WebView))
            {
                throw new PrintJobException(
                    "EXCEPTION:PRN006 Invalid native WebView");
            }

            _nativeWebView = (Windows.UI.Xaml.Controls.WebView)webViewAdditionalFunctions.NativeControl;
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
                    $"EXCEPTION:PRN005 Unexpected page number ({oneBasedPageNumber}) requested");
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
        public override async Task PrintAsync() =>
            // TODO - investigate options for scaling content of WebView to effectively
            // do a Fit to Page
            //
            // Possible option #1
            // Insert some JS/CSS/HTML into source of page
            // string updatedHtmlSource =
            //    await InsertAfterNodeAsync(
            //        _htmlSource,
            //        "head",
            //        ""); // TODO - work out what would need to be inserted
            //
            // Ruled out option #2
            // Wrapping the WebView in a ViewBox, doesn't work.
            // It scales the WebView, but not the content of the WebView
            //_viewBox = new Windows.UI.Xaml.Controls.Viewbox();
            //_viewBox.Child = _nativeWebView;
            //_viewBox.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
            //_viewBox.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;
            //
            // Ruled out option #3
            // Using Width, Scale, HorizontalAlignment and VerticalAlignment properties of the 
            // WebView, doesn't work. Scales the WebView, but not the content of the WebView
            //_nativeWebView.Width *= 2; 
            //_nativeWebView.Scale = new System.Numerics.Vector3((float) 0.2, (float) 0.2, 1);
            //_nativeWebView.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
            //_nativeWebView.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;
            //
            // Possible option #4
            // Put the WebView in a RichTextBlock, allowing the WebView to grow as big as 
            // required to show all of its content. Then use RichTextBlock printing code
            // based on https://github.com/XamlBrewer/XamlBrewer.Uwp.PrintService
            // This seems the most likely option.

            // Note that we are assuming that the WebView has already been rendered, so no need
            // to wait on the WebView.DOMContentLoaded event before calling ShowPrintApiAsync()

            await ShowPrintApiAsync();

        //        /// <summary>
        //        /// Insert the specified HTML after the specified node in the HTML source.
        //        /// </summary>
        //        /// <param name="htmlSource">The HTML to insert into.</param>
        //        /// <param name="nodeToAddAfter">The node to insert after.</param>
        //        /// <param name="htmlToInsert">The HTML to insert.</param>
        //        /// <returns>Resulting HTML if successful. Original HTML if error.</returns>
        //        /// <remarks>Note that this implementation is hacky and simplistic. It may
        //        /// need re-visiting at some point.</remarks>
        //        private async Task<string> InsertAfterNodeAsync(
        //            string htmlSource,
        //            string nodeToAddAfter,
        //            string htmlToInsert)
        //        {
        //            try
        //            {
        //                StringComparison stringComparison = StringComparison.Ordinal;

        //                int indexOfNode = htmlSource.IndexOf("<" + nodeToAddAfter, stringComparison);
        //                if (indexOfNode < 0)
        //                    return htmlSource;

        //                string afterNodeStart = htmlSource.Substring(indexOfNode + 1 + nodeToAddAfter.Length);
        //                int indexOfTagClose = afterNodeStart.IndexOf(">", stringComparison);
        //                if (indexOfTagClose < 0)
        //                    return htmlSource;

        //                string remainder = afterNodeStart.Substring(indexOfTagClose + 2);

        //                string result
        //                    = htmlSource.Substring(0, indexOfNode)
        //                      + "<"
        //                      + nodeToAddAfter
        //                      + afterNodeStart.Substring(0, indexOfTagClose + 1)
        //                      + htmlToInsert
        //                      + remainder;

        //                return result;
        //            }
        //#pragma warning disable CA1031 // Do not catch general exception types
        //            catch (Exception ex)
        //            {
        //                await PrintStatusReporting.ReportExceptionAsync(ex);
        //                return htmlSource;
        //            }
        //#pragma warning restore CA1031 // Do not catch general exception types
        //        }

    } // internal sealed class WebViewPrintJob : BasePrintJob

} // namespace Plugin.Printing

// eof

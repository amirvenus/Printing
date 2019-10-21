using System;

namespace Plugin.Printing
{
    /// <summary>
    /// Android WebViewClient used for printing purposes
    /// </summary>
    /// <see cref="https://developer.android.com/training/printing/html-docs"/>
    /// <remarks>
    /// Note that the documentation says:
    ///
    /// This needs INTERNET permission.
    ///
    /// When using WebView for creating print documents, you should be aware of the following limitations:
    ///  You cannot add headers or footers, including page numbers, to the document.
    ///  The printing options for the HTML document do not include the ability to print page ranges, for example: Printing page 2 to 4 of a 10 page HTML document is not supported.
    ///  An instance of WebView can only process one print job at a time.
    ///  An HTML document containing CSS print attributes, such as landscape properties, is not supported.
    ///  You cannot use JavaScript in a HTML document to trigger printing.
    /// </remarks>
    internal class WebViewPrintClientSubclass : Android.Webkit.WebViewClient
    {
        private PrintJobConfiguration _printJobConfiguration;
        private readonly string _url;
#pragma warning disable IDE0052 // Remove unread private members
        // this may not appear to be required, but documentation says to have this to prevent early garbage collection
        private Android.Webkit.WebView _webView;
#pragma warning restore IDE0052 // Remove unread private members

        public WebViewPrintClientSubclass(string url, PrintJobConfiguration printJobConfiguration)
        {
            _printJobConfiguration = printJobConfiguration;
            _url = url;
        }

        public void Run()
        {
            _webView = new Android.Webkit.WebView(PrintServiceAndroidHelper.ActivityInstance);
            _webView.LoadUrl(_url);
            PrintServiceAndroidHelper.PrintFromWebView(_webView, _printJobConfiguration);
        }

        [Obsolete("deprecated")]
        public override bool ShouldOverrideUrlLoading(Android.Webkit.WebView view, string url) => false;

        public override void OnPageFinished(Android.Webkit.WebView webView, string url)
        {
            // TODO - investigate whether even using OnPageFinished is enough to always avoid getting a blank page output
            PrintServiceAndroidHelper.PrintFromWebView(webView, _printJobConfiguration);
            _printJobConfiguration = null;
            _webView.Dispose();
            _webView = null;
            Dispose(); // is this legitimate?
        }

    } // internal class WebViewClientSubclass : Android.Webkit.WebViewClient

} // namespace Plugin.Printing

// eof

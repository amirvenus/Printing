#pragma warning disable IDE0007 // Use implicit type
#pragma warning disable IDE0063 // Use simple 'using' statement

using System;
using System.IO;
using System.Threading.Tasks;

using CoreGraphics;
using Foundation;
using UIKit;
using WebKit; // TODO - will need this when replaced deprecated UIWebView with WKWebView

using Xamarin.Forms;

namespace Plugin.Printing
{
    /// <summary>
    /// Implementation of Printing for iOS
    /// </summary>
    /// <remarks>
    /// Based on various sources, including GercoBrandwijk
    /// See https://forums.xamarin.com/discussion/42417/printing-on-ipad-from-webview
    /// </remarks>
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
                    using (NSUrl nsUrl = NSUrl.FromString(url))
                    {
                        result = await PrintObjectAsync(nsUrl, printJobConfiguration);
                    }
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return result;
        }

        public override async Task<bool> PrintImageFromByteArrayAsync(byte[] content, PrintJobConfiguration printJobConfiguration)
        {
            try
            {
                if (PrintImageFromByteArrayAsyncSupported)
                {
                    return await PrintImageFromData(
                        NSData.FromArray(content),
                        printJobConfiguration);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return false;
        }

        public override async Task<bool> PrintImageFromStreamAsync(Stream stream, PrintJobConfiguration printJobConfiguration)
        {
            try
            {
                if (PrintImageFromStreamAsyncSupported)
                {
                    return await PrintImageFromData(
                        NSData.FromStream(stream),
                        printJobConfiguration);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return false;
        }

        public override async Task<bool> PrintPdfFromStreamAsync(Stream inputStream, PrintJobConfiguration printJobConfiguration) 
            => PrintPdfFromStreamAsyncSupported
                && await PrintFileFromStreamAsync(inputStream, printJobConfiguration);

        /// <summary>
        /// Print the contents of a WebView
        /// </summary>
        /// <param name="parentPage">The page that the WebView is shown on.</param>
        /// <param name="webView">The Xamarin.Forms WebView.</param>
        /// <param name="webViewAdditionFunctions">Additional functions</param>
        /// <param name="printJobConfiguration">Configuration information for the print job.</param>
        /// <returns>true if attempting print, false otherwise.</returns>
        public override async Task<bool> PrintWebViewAsync(
            Page parentPage,
            WebView webView,
            IWebViewAdditionalFunctions webViewAdditionFunctions,
            PrintJobConfiguration printJobConfiguration)
        {
            bool result = false;

            try
            {
                // TODO - get rid of use of UIWebView as deprecated by Apple
                // See https://forums.xamarin.com/discussion/168620/itms-90338-non-public-api-usage-apple-rejected-my-app-xamarin#latest

                if (PrintWebViewAsyncSupported
                    && (webViewAdditionFunctions?.NativeControl is UIWebView nativeWebView))
                {
                    using (nativeWebView)
                    {
                        result = await PrintUIViewAsync(nativeWebView, printJobConfiguration);
                    }
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return result;
        }

        private static async Task<UIPrintInteractionController> PopulateCommonPrinterDetailsAsync(
            PrintJobConfiguration printJobConfiguration)
        {
            UIPrintInteractionController printer = UIPrintInteractionController.SharedPrintController;
            if (printer is null)
            {
                await PrintStatusReporting.ReportErrorAsync(
                    CrossPrinting.PrintingMessages.UnableToPrintAtThisTime);
            }
            else
            {
                printer.PrintInfo = UIPrintInfo.PrintInfo;

                if (!(printer.PrintInfo is null))
                {
                    if (!(printJobConfiguration is null))
                    {
                        switch (printJobConfiguration.DuplexPreference)
                        {
                            case PrintJobDuplexConfiguration.LongEdge:
                                printer.PrintInfo.Duplex = UIPrintInfoDuplex.LongEdge;
                                break;
                            case PrintJobDuplexConfiguration.ShortEdge:
                                printer.PrintInfo.Duplex = UIPrintInfoDuplex.ShortEdge;
                                break;
                            case PrintJobDuplexConfiguration.None:
                            default:
                                printer.PrintInfo.Duplex = UIPrintInfoDuplex.None;
                                break;
                        }

                        printer.PrintInfo.JobName = printJobConfiguration.JobName;

                        printer.PrintInfo.Orientation
                            = printJobConfiguration.PreferPortraitOrientation
                            ? UIPrintInfoOrientation.Portrait
                            : UIPrintInfoOrientation.Landscape;

                        printer.PrintInfo.OutputType
                            = printJobConfiguration.PreferColor
                            ? UIPrintInfoOutputType.General
                            : UIPrintInfoOutputType.Grayscale;

                    }
                }
                printer.ShowsPageRange = true;
            }

            return printer;
        }
        /// <summary>
        /// Callback method that is called when printing completes (successfully or as a result of failure).
        /// </summary>
        /// <param name="_">The Print Interaction controller</param>
        /// <param name="completed">Flag indicating if printing has completed.</param>
        /// <param name="nsError">NSError value used to report errors during printing. Null if no error.</param>
        private static async void PrintCompletion(
            UIPrintInteractionController _, 
            bool completed, 
            NSError nsError)
        {
            try
            {
                if (completed)
                {
                    await PrintStatusReporting.ReportInfoAsync(
                        CrossPrinting.PrintingMessages.PrintInteractionCompleted);
                }
                else if (!completed && nsError != null)
                {
                    await PrintStatusReporting.ReportInfoAsync(
                        CrossPrinting.PrintingMessages.PrintInteractionError);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types

        }

        private async Task<bool> PrintFileFromStreamAsync(Stream inputStream, PrintJobConfiguration printJobConfiguration)
        {
            bool result = false;

            try
            {
                string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string library = Path.Combine(documents, "..", "Library");
                string filePath = Path.Combine(library, printJobConfiguration.JobName);

                try
                {
                    File.Delete(filePath);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch
                {
                    // no-op
                }
#pragma warning restore CA1031 // Do not catch general exception types

                using (MemoryStream tempStream = new MemoryStream())
                {
                    inputStream.Position = 0;
                    inputStream.CopyTo(tempStream);
                    File.WriteAllBytes(filePath, tempStream.ToArray());
                }

                NSUrl url = NSUrl.FromFilename(filePath);
                using (url)
                {
                    result = await PrintObjectAsync(url, printJobConfiguration);
                }

            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return result;
        }
        private async Task<bool> PrintImageFromData(NSData data, PrintJobConfiguration printJobConfiguration)
        {
            bool result = false;

            using (data)
            {
                UIImage uiImage = UIImage.LoadFromData(data);
                using (uiImage)
                {
                    result = await PrintObjectAsync(uiImage, printJobConfiguration);
                }
            }

            return result;
        }

        private static async Task<bool> PrintObjectAsync(NSObject nsObject, PrintJobConfiguration printJobConfiguration)
        {
            bool result = false;

            UIPrintInteractionController printer = await PopulateCommonPrinterDetailsAsync(printJobConfiguration);
            if (!(printer is null))
            {
                printer.PrintingItem = nsObject;
                printer.Present(true, PrintCompletion);
                printer.Dispose();
                result = true;
            }

            return result;
        }

        private async Task<bool> PrintUIViewAsync(UIView uiView, PrintJobConfiguration printJobConfiguration)
        {
            bool result = false;

            UIPrintInteractionController printer = await PopulateCommonPrinterDetailsAsync(printJobConfiguration);
            if (!(printer is null))
            {
                printer.PrintPageRenderer = new UIPrintPageRenderer()
                {
                    HeaderHeight = 40,
                    FooterHeight = 40
                };
                printer.PrintPageRenderer.AddPrintFormatter(uiView.ViewPrintFormatter, 0);

                UIPrintInteractionResult interactionResult;

                if (Device.Idiom == TargetIdiom.Tablet) // TODO - move to ApplicationType
                {
                    interactionResult =
                        await printer.PresentFromRectInViewAsync(new CGRect(200, 200, 0, 0), uiView, true);
                }
                else
                {
                    interactionResult = await printer.PresentAsync(true);
                }

                PrintCompletion(
                    interactionResult.PrintInteractionController,
                    interactionResult.Completed,
                    null);
                result = interactionResult.Completed;
                printer.Dispose();
            }

            return result;
        }
    }

} // namespace Plugin.Printing

#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore IDE0007 // Use implicit type

// eof

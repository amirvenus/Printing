#pragma warning disable IDE0007 // Use implicit type
#pragma warning disable IDE0063 // Use simple 'using' statement

using System.IO;

using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Print;

namespace Plugin.Printing
{
    /// <summary>
    /// Helpers used when printing on Android
    /// </summary>
    public static class PrintServiceAndroidHelper
    {
        private static PrintManager _printManagerInstance;

        /// <summary>
        /// The instance of Activity providing the context for this code
        /// to operate in.
        ///
        /// Typically, this will be set from an app's MainActivity.cs
        /// The important thing is that it is set before attempting to
        /// use this printing code.
        /// </summary>
        public static Activity ActivityInstance { get; set; }

        /// <summary>
        /// The instance of the Android PrintManager to be used when
        /// printing on Android.
        ///
        /// Typically, the calling app does not need to utilise this,
        /// other than to Dispose the instance and to set it to null
        /// if cleanup is required.
        /// </summary>
        public static PrintManager PrintManagerInstance
        {
            get => _printManagerInstance ?? (_printManagerInstance =
                       (PrintManager)PrintServiceAndroidHelper.ActivityInstance.GetSystemService(Android.Content.Context.PrintService));
            set => _printManagerInstance = value;
        }

        /// <summary>
        /// Check whether the version of CreatePrintDocumentAdapter that accepts
        /// the job name as an argument is supported on this device.
        /// </summary>
        /// <returns></returns>
        internal static bool CreatePrintDocumentAdapterWithDocumentNameArgSupported()
            => ((Build.VERSION.SdkInt) >= BuildVersionCodes.Lollipop);

        /// <summary>
        /// Check whether printing is supported on this Android device.
        /// </summary>
        /// <returns>true if printing is supported on this Android device, false otherwise.</returns>
        internal static bool PrintingApiSupported() => ((Android.OS.Build.VERSION.SdkInt) >= Android.OS.BuildVersionCodes.Lollipop); // TODO - check if this should be KitKat

        /// <summary>
        /// Print from stream using configured preferences
        /// </summary>
        /// <param name="inputStream">Stream providing data to print</param>
        /// <param name="printJobConfiguration">Configuration for the print job</param>
        /// <param name="printContentType"></param>
        internal static void PrintFromStream(
            Stream inputStream,
            PrintJobConfiguration printJobConfiguration,
            PrintContentType printContentType = PrintContentType.Document)
        {
            string fileName = printJobConfiguration.JobName;
            if (inputStream.CanSeek)
                inputStream.Position = 0;

            string createdFilePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), fileName);
            try
            {
                System.IO.File.Delete(createdFilePath);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
            {
                // no-op
            }
#pragma warning restore CA1031 // Do not catch general exception types

            using (FileStream dest = System.IO.File.OpenWrite(createdFilePath))
                inputStream.CopyTo(dest);
            string filePath = createdFilePath;
            //using (
            PrintDocumentAdapter pda = new CustomPrintDocumentAdapter(filePath, printContentType);
            {
                PrintServiceAndroidHelper.PrintFromDocumentAdapter(pda, printJobConfiguration);
            }
        }

        /// <summary>
        /// Print from Android WebView using configured preferences
        /// </summary>
        /// <param name="webView">The Android WebView to print</param>
        /// <param name="printJobConfiguration">Configuration for the print job</param>
        internal static void PrintFromWebView(
            Android.Webkit.WebView webView,
            PrintJobConfiguration printJobConfiguration)
        {
            if (PrintServiceAndroidHelper.PrintingApiSupported())
            {
#pragma warning disable CS0618 // Type or member is obsolete
#if ANDROIDAPI21PLUS
                bool printJobNameSupported = CreatePrintDocumentAdapterWithDocumentNameArgSupported();
#endif
                PrintDocumentAdapter adapter = // TODO - dispose?
#if ANDROIDAPI21PLUS
                    printJobNameSupported
                        ? webView.CreatePrintDocumentAdapter(printJobConfiguration?.JobName)
                        :
#endif
                        webView.CreatePrintDocumentAdapter();
            }
        }

        /// <summary>
        /// Print image from stream using configured preferences
        /// </summary>
        /// <param name="inputStream">The stream providing the image data</param>
        /// <param name="printJobConfiguration">Configuration for the print job</param>
        /// <remarks>Based on FormsPrintSample from Pujolsluis</remarks>
        /// <see cref="https://github.com/Pujolsluis/FormsPrintSample"/>
        internal static void PrintImageFromStream(
            Stream inputStream, 
            PrintJobConfiguration printJobConfiguration)
        {
            using (Android.Support.V4.Print.PrintHelper photoPrinter
                = new Android.Support.V4.Print.PrintHelper(ActivityInstance)
            {
                ColorMode
                    = printJobConfiguration.PreferColor
                    ? Android.Support.V4.Print.PrintHelper.ColorModeColor
                    : Android.Support.V4.Print.PrintHelper.ColorModeMonochrome,
                Orientation
                    = printJobConfiguration.PreferPortraitOrientation
                    ? Android.Support.V4.Print.PrintHelper.OrientationPortrait
                    : Android.Support.V4.Print.PrintHelper.OrientationLandscape,
                ScaleMode
                    = printJobConfiguration.PreferFitToPage
                    ? Android.Support.V4.Print.PrintHelper.ScaleModeFit
                    : Android.Support.V4.Print.PrintHelper.ScaleModeFill,
            })
            {
                Bitmap bitmap = BitmapFactory.DecodeStream(inputStream);
                // could add a callback handler to the following, but not going to for the time being as not much value in it
                photoPrinter.PrintBitmap(printJobConfiguration?.JobName, bitmap);
            }
        }

        /// <summary>
        /// Print using configured preferences
        /// </summary>
        /// <param name="printDocumentAdapter">The Android PrintDocumentAdapter</param>
        /// <param name="printJobConfiguration">Configuration for the print job</param>
        private static void PrintFromDocumentAdapter(
            PrintDocumentAdapter printDocumentAdapter,
            PrintJobConfiguration printJobConfiguration)
        {
            using (PrintAttributes.Builder builder = new PrintAttributes.Builder())
            {
                builder.SetColorMode(printJobConfiguration.PreferColor
                    ? PrintColorMode.Color
                    : PrintColorMode.Monochrome);
                switch (printJobConfiguration.DuplexPreference)
                {
                    case PrintJobDuplexConfiguration.None:
                        builder.SetDuplexMode(DuplexMode.None);
                        break;
                    case PrintJobDuplexConfiguration.LongEdge:
                        builder.SetDuplexMode(DuplexMode.LongEdge);
                        break;
                    case PrintJobDuplexConfiguration.ShortEdge:
                        builder.SetDuplexMode(DuplexMode.ShortEdge);
                        break;
                }

                builder.SetMediaSize(printJobConfiguration.PreferPortraitOrientation
                    ? PrintAttributes.MediaSize.UnknownPortrait
                    : PrintAttributes.MediaSize.UnknownLandscape);

                builder.SetMinMargins(PrintAttributes.Margins.NoMargins);

                using (PrintAttributes printAttributes = builder.Build())
                {
                    PrintServiceAndroidHelper.PrintManagerInstance.Print(printJobConfiguration.JobName, printDocumentAdapter, printAttributes);
                }
            }
        }

    } // public static class PrintServiceAndroidHelper

} // namespace Plugin.Printing

#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore IDE0007 // Use implicit type

// eof

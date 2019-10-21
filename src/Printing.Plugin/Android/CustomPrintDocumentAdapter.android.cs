#pragma warning disable IDE0007 // Use implicit type
#pragma warning disable IDE0063 // Use simple 'using' statement

using Android.OS;
using Android.Print;

using Java.IO;

namespace Plugin.Printing
{
    /// <summary>
    /// Android PrintDocumentAdapter, used for printing from a stream
    /// </summary>
    /// <remarks>Based on earlier work by bushbert</remarks>
    /// <see cref="https://github.com/bushbert/XamarinPCLPrinting/blob/master/PCLPrintExample/PCLPrintExample/PCLPrintExample.Android/Print.cs"/>
    internal class CustomPrintDocumentAdapter : PrintDocumentAdapter, Android.Support.V4.Print.PrintHelper.IOnPrintFinishCallback
    {
        private InputStream _input;
        private OutputStream _output;

        internal string FileToPrint { get; set; }
        internal Android.Print.PrintContentType ContentType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileDesc"></param>
        /// <param name="contentType"></param>
        internal CustomPrintDocumentAdapter(string fileDesc, PrintContentType contentType = Android.Print.PrintContentType.Document)
        {
            FileToPrint = fileDesc;
            ContentType = contentType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldAttributes"></param>
        /// <param name="newAttributes"></param>
        /// <param name="cancellationSignal"></param>
        /// <param name="callback"></param>
        /// <param name="extras"></param>
        public override void OnLayout(PrintAttributes oldAttributes, PrintAttributes newAttributes, CancellationSignal cancellationSignal, LayoutResultCallback callback, Bundle extras)
        {
            if (cancellationSignal.IsCanceled)
            {
                callback.OnLayoutCancelled();
                return;
            }

            using (PrintDocumentInfo.Builder builder = new PrintDocumentInfo.Builder(FileToPrint))
            {
                //using (
                PrintDocumentInfo pdi = builder.SetContentType(ContentType).Build(); //)
                {
                    callback.OnLayoutFinished(pdi, true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pages"></param>
        /// <param name="destination"></param>
        /// <param name="cancellationSignal"></param>
        /// <param name="callback"></param>
        public override async void OnWrite(PageRange[] pages, ParcelFileDescriptor destination, CancellationSignal cancellationSignal, WriteResultCallback callback)
        {
            _input = null;
            _output = null;

            try
            {
                _input = new FileInputStream(FileToPrint); // if we use using on this, we get an ObjectDisposedException // TODO - investigate further
                _output = new FileOutputStream(destination.FileDescriptor); // if we use using on this, we get an ObjectDisposedException // TODO - investigate further

                byte[] buf = new byte[1024];
                int bytesRead;

                while ((bytesRead = _input.Read(buf)) > 0)
                    _output.Write(buf, 0, bytesRead);

                callback.OnWriteFinished(new PageRange[] { PageRange.AllPages });
            }
            catch (Java.IO.FileNotFoundException ee)
            {
                await PrintStatusReporting.ReportExceptionSilentlyAsync(ee);
            }
            catch (Java.Lang.Exception jlex)
            {
                await PrintStatusReporting.ReportExceptionSilentlyAsync(jlex);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (System.Exception e)
            {
                await PrintStatusReporting.ReportExceptionSilentlyAsync(e);
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                try
                {
                    if (!(_input is null))
                    {
                        _input.Close();
                        _input = null;
                    }

                    if (!(_output is null))
                    {
                        _output.Close();
                        _output = null;
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (System.Exception e)
                {
                    await PrintStatusReporting.ReportExceptionSilentlyAsync(e);
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }
        }

    } // internal class CustomPrintDocumentAdapter : PrintDocumentAdapter, Android.Support.V4.Print.PrintHelper.IOnPrintFinishCallback

} // namespace Plugin.Printing

#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore IDE0007 // Use implicit type

// eof

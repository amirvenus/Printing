using System;

namespace Plugin.Printing
{
    internal static class SafeBlockHelper
    {
        internal delegate void BlockDelegate();

        /// <summary>
        /// Wraps a function in a try/catch block, reporting back any
        /// exception on the UI thread through the PrintStatusReporting class.
        /// </summary>
        /// <param name="func"></param>
        internal static void SafeBlock(
            BlockDelegate func)
        {
            try
            {
                func();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
                {
                    await PrintStatusReporting.ReportExceptionAsync(ex);
                });
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

    } // internal static class SafeBlockHelper

} // namespace Plugin.Printing

// eof

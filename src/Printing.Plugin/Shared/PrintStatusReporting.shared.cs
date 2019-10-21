using System;
using System.Threading.Tasks;

namespace Plugin.Printing
{
    public delegate Task ReportErrorDelegateAsync(string messageText);

    public delegate Task ReportExceptionDelegateAsync( // can be called from a non-UI thread
        Exception ex,
        string memberName,
        string sourceFilePath,
        int sourceLineNumber);

    public delegate Task ReportInfoDelegateAsync(string messageText);

    public sealed class PrintStatusReporting
    {
        private static ReportErrorDelegateAsync _reportErrorDelegateAsync;
        private static ReportExceptionDelegateAsync _reportExceptionDelegateAsync;
        private static ReportExceptionDelegateAsync _reportExceptionSilentlyDelegateAsync;
        private static ReportInfoDelegateAsync _reportInfoDelegateAsync;

        public static void SetPrintStatusReporting(
            ReportErrorDelegateAsync reportErrorDelegateAsync,
            ReportExceptionDelegateAsync reportExceptionDelegate, // can be called from a non-UI thread
            ReportExceptionDelegateAsync reportExceptionSilentlyDelegate, // can be called from a non-UI thread
            ReportInfoDelegateAsync reportInfoDelegateAsync
        )
        {
            _reportErrorDelegateAsync = reportErrorDelegateAsync;
            _reportExceptionDelegateAsync = reportExceptionDelegate;
            _reportExceptionSilentlyDelegateAsync = reportExceptionSilentlyDelegate;
            _reportInfoDelegateAsync = reportInfoDelegateAsync;
        }

        public static async Task ReportErrorAsync(string messageText)
        {
            if (_reportErrorDelegateAsync != null)
                await _reportErrorDelegateAsync.Invoke(messageText);
        }

        public static async Task ReportInfoAsync(string messageText)
        {
            if (_reportInfoDelegateAsync != null)
                await _reportInfoDelegateAsync.Invoke(messageText);
        }

        public static async Task ReportExceptionAsync(
            Exception ex,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (_reportExceptionDelegateAsync != null)
                await _reportExceptionDelegateAsync.Invoke(ex, memberName, sourceFilePath, sourceLineNumber);
        }

        public static async Task ReportExceptionSilentlyAsync(
            Exception ex,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (_reportExceptionSilentlyDelegateAsync != null)
                await _reportExceptionSilentlyDelegateAsync.Invoke(ex, memberName, sourceFilePath, sourceLineNumber);
        }

    } // public sealed class PrintStatusReporting

} // namespace Plugin.Printing

// eof


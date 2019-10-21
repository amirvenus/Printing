#pragma warning disable IDE0007 // Use implicit type

using System;

namespace Plugin.Printing
{
    /// <summary>
    /// Cross Printing
    /// </summary>
    public static class CrossPrinting
    {
#pragma warning disable IDE1006 // Naming Styles
        private static IBasePrintingMessages _basePrintingMessages;
#pragma warning restore IDE1006 // Naming Styles

        static readonly Lazy<IPrinting> implementation = new Lazy<IPrinting>(() => CreatePrinting(),
            System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Gets if the plugin is supported on the current platform.
        /// </summary>
        public static bool IsSupported => implementation.Value != null;

        /// <summary>
        /// Current plugin implementation to use
        /// </summary>
        public static IPrinting Current
        {
            get
            {
                IPrinting ret = implementation.Value;
                if (ret == null)
                {
                    throw NotImplementedInReferenceAssembly();
                }

                return ret;
            }
        }

#pragma warning disable IDE1006 // Naming Styles
        static IPrinting CreatePrinting() =>
#pragma warning restore IDE1006 // Naming Styles
#if NETSTANDARD1_0 || NETSTANDARD2_0
            null;
#else
#pragma warning disable IDE0022 // Use expression body for methods
            new PrintingImplementation();
#pragma warning restore IDE0022 // Use expression body for methods
#endif

        internal static Exception NotImplementedInReferenceAssembly() =>
            new NotImplementedException(
                "This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");

        ///// <summary>
        ///// Dispose of everything 
        ///// </summary>
        //public static void Dispose()
        //{
        //    if (implementation != null && implementation.IsValueCreated)
        //    {
        //        implementation.Value.Dispose();
        //        implementation = new Lazy<IPrinting>(() => CreatePrinting(), System.Threading.LazyThreadSafetyMode.PublicationOnly);
        //    }
        //}

        /// <summary>
        /// Property that specifies the HelpLink used when populating PrintJobException instances
        /// </summary>
        public static string PresetHelpLink
        {
            get => PrintJobException.PresetHelpLink;
            set => PrintJobException.PresetHelpLink = value;
        }

        /// <summary>
        /// Set delegates that are called by the printing code
        /// when exceptions occur or when there is error or info
        /// status information to report back to the code using
        /// this printing library/NuGet.
        /// </summary>
        /// <param name="reportErrorDelegateAsync">Delegate that is called when an error occurs</param>
        /// <param name="reportExceptionDelegate">Delegate that is called when an exception occurs that does want to be notified immediately to the user</param>
        /// <param name="reportExceptionSilentlyDelegate">Delegate that is called when an exception occurs that doesn't want to be notified immediately to the user</param>
        /// <param name="reportInfoDelegateAsync">Delegate that is called when there is information to report</param>
        public static void SetPrintStatusReporting(
            ReportErrorDelegateAsync reportErrorDelegateAsync,
            ReportExceptionDelegateAsync reportExceptionDelegate, // Note that ReportExceptionDelegateAsync can be called from a non-UI thread
            ReportExceptionDelegateAsync reportExceptionSilentlyDelegate, // Note that ReportExceptionDelegateAsync can be called from a non-UI thread
            ReportInfoDelegateAsync reportInfoDelegateAsync) => PrintStatusReporting.SetPrintStatusReporting(
                reportErrorDelegateAsync,
                reportExceptionDelegate,
                reportExceptionSilentlyDelegate,
                reportInfoDelegateAsync);

        /// <summary>
        /// Property that allows the default messages (in English) to be
        /// replaced with messages in other languages
        /// </summary>
        public static IBasePrintingMessages PrintingMessages
        {
            get => _basePrintingMessages ?? (_basePrintingMessages = new DefaultPrintingMessages());
            set => _basePrintingMessages = value;
        }

    } // public static class CrossPrinting

} // namespace Plugin.Printing

#pragma warning restore IDE0007 // Use implicit type

// eof

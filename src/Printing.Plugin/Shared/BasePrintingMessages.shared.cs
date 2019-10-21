namespace Plugin.Printing
{
    public class BasePrintingMessages : IBasePrintingMessages
    {
        public string AbandonedFormatString { get; set; }
        public string AwaitingResponse { get; set; }
        public string CanceledFormatString { get; set; }
        public string DefaultFormatString { get; set; }
        public string FailedFormatString { get; set; }
        public string FailedToRegisterForPrinting { get; set; }
        public string NullDescriptionString { get; set; }
        public string PagesNotPresentErrorString { get; set; }
        public string PagesNotPresentWarningText { get; set; }
        public string PrinterServiceBusy { get; set; }
        public string PrintInteractionCompleted { get; set; }
        public string PrintInteractionError { get; set; }
        public string SubmittedFormatString { get; set; }
        public string UnableToPrintAtThisTime { get; set; }

    } // public class BasePrintingMessages : IBasePrintingMessages

    internal sealed class DefaultPrintingMessages : BasePrintingMessages
    {
        internal DefaultPrintingMessages()
        {
            AbandonedFormatString = "Print job '{0}' abandoned";
            AwaitingResponse = "Awaiting response from printer service";
            CanceledFormatString = "Print job '{0}' canceled";
            DefaultFormatString = "Print job '{0}' completed with unknown status";
            FailedFormatString = "Print job '{0}' failed";
            FailedToRegisterForPrinting = "Failed to register for printing";
            NullDescriptionString = "{null}";
            PagesNotPresentErrorString = "Those pages are not present in the document";
            PagesNotPresentWarningText = "One of the ranges contains pages that are not present in the document";
            PrinterServiceBusy = "Print service busy. Please try again shortly";
            PrintInteractionCompleted = "Print completed";
            PrintInteractionError = "Error printing";
            SubmittedFormatString = "Print job '{0}' submitted";
            UnableToPrintAtThisTime = "Unable to print at this time";
        }

    } // internal sealed class DefaultPrintingMessages : BasePrintingMessages

} // namespace Plugin.Printing

// eof

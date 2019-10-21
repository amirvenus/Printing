namespace Plugin.Printing
{
    public enum PrintJobDuplexConfiguration
    {
        None = 0,
        LongEdge = 1,
        ShortEdge = 2
    }

    public class PrintJobConfiguration
    {
        public PrintJobConfiguration(string jobName, bool preferPrintPreview = true)
        {
            JobName = jobName;
            PreferPrintPreview = preferPrintPreview;
        }

        public PrintJobConfiguration(string fileName, string jobName, bool preferPrintPreview = true)
        {
            FileName = fileName;
            JobName = jobName;
            PreferPrintPreview = preferPrintPreview;
        }

        public bool AllowPageRangeSpecification { get; set; }

        public PrintJobDuplexConfiguration DuplexPreference { get; set; } =
            PrintJobDuplexConfiguration.LongEdge;

        public string FileName { get; set; }
        public string JobName { get; set; }
        public bool PreferColor { get; set; } = true;
        public bool PreferFitToPage { get; set; } = true;
        public bool PreferPortraitOrientation { get; set; } = true;
        public bool PreferPrintPreview { get; set; }

    } // public class PrintJobConfiguration

} // namespace Plugin.Printing

// eof

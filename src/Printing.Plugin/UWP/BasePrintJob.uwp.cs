#pragma warning disable IDE0007 // Use implicit type

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Windows.Graphics.Printing;

namespace Plugin.Printing
{
    /// <summary>
    /// UWP printing functionality
    /// </summary>
    /// <remarks>
    /// This code is based on Microsoft documentation and sample code
    /// See https://docs.microsoft.com/en-us/windows/uwp/devices-sensors/print-from-your-app
    /// </remarks>
    internal abstract class BasePrintJob
    {
#if DEBUG
        // The stuff in this DEBUG block is to help check 
        // that event handlers are cleaned up reliably

        private int _eventHandlersPaginate = 0;
        private int _eventHandlersGetPreviewPage = 0;
        private int _eventHandlersAddPages = 0;
        private int _eventHandlersOptionChanged = 0;
        private int _eventHandlersCompleted = 0;

        private void DecrementPaginateCount() => System.Diagnostics.Debug.WriteLine(
                $"EventHandlersPaginate = {--_eventHandlersPaginate}");

        private void DecrementGetPreviewPageCount() => System.Diagnostics.Debug.WriteLine(
                $"EventHandlersGetPreviewPage = {--_eventHandlersGetPreviewPage}");

        private void DecrementAddPagesCount() => System.Diagnostics.Debug.WriteLine(
                $"EventHandlersAddPages = {--_eventHandlersAddPages}");

        private void DecrementOptionChangedCount() => System.Diagnostics.Debug.WriteLine(
                $"EventHandlersOptionChanged = {--_eventHandlersOptionChanged}");
        private void DecrementCompletedCount() => System.Diagnostics.Debug.WriteLine(
                $"EventHandlersCompleted = {--_eventHandlersCompleted}");

        private void IncrementPaginateCount() => System.Diagnostics.Debug.WriteLine(
                $"EventHandlersPaginate = {++_eventHandlersPaginate}");

        private void IncrementGetPreviewPageCount() => System.Diagnostics.Debug.WriteLine(
                $"EventHandlersGetPreviewPage = {++_eventHandlersGetPreviewPage}");

        private void IncrementAddPagesCount() => System.Diagnostics.Debug.WriteLine(
                $"EventHandlersAddPages = {++_eventHandlersAddPages}");

        private void IncrementOptionChangedCount() => System.Diagnostics.Debug.WriteLine(
                $"EventHandlersOptionChanged = {++_eventHandlersOptionChanged}");
        private void IncrementCompletedCount() => System.Diagnostics.Debug.WriteLine(
                $"EventHandlersCompleted = {++_eventHandlersCompleted}");
#endif

        protected Windows.UI.Xaml.Printing.PrintDocument _printDocument;
        protected Windows.Graphics.Printing.IPrintDocumentSource _printDocumentSource;
        protected PrintJobConfiguration _printJobConfiguration;
        private Windows.Graphics.Printing.PrintManager _printManager;
        private readonly List<Windows.UI.Xaml.UIElement> _printPreviewPages
            = new List<Windows.UI.Xaml.UIElement>();
        private Windows.Graphics.Printing.PrintTask _printTask;

        /// <summary>
        /// Test whether printing is supported on the current device
        /// </summary>
        internal static bool PrintingSupported
            => Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent(
                   "Windows.Foundation.FoundationContract", 1, 0)
               && PrintManager.IsSupported();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="printJobConfiguration">Configuration information for the print job</param>
        protected BasePrintJob(PrintJobConfiguration printJobConfiguration)
        {
            _printJobConfiguration = printJobConfiguration;
        }

        /// <summary>
        /// Add the specified page to the page cache
        /// </summary>
        /// <param name="page">Page to add</param>
        protected void AddPageToCache(Windows.UI.Xaml.UIElement page) => _printPreviewPages.Add(page);

        protected void ClearCachedPages() => _printPreviewPages?.Clear();

        /// <summary>
        /// Generate and cache pages to print
        /// </summary>
        /// <returns>Task that the caller is expected to await</returns>
        protected virtual Task GeneratePagesAsync()
        {
            ClearCachedPages();

            Windows.UI.Xaml.UIElement printPreviewPage;
            int numberOfPages = GetNumberOfPages();
            for (int pageNumber = 1; pageNumber <= numberOfPages; ++pageNumber)
            {
                if ((printPreviewPage = GetNextPrintPreviewPage(pageNumber)) != null)
                {
                    AddPageToCache(printPreviewPage);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Get the specified preview page
        /// </summary>
        /// <param name="oneBasedPageNumber">The page number (counting from one)</param>
        /// <returns>Specified preview page</returns>
        public abstract Windows.UI.Xaml.UIElement GetNextPrintPreviewPage(int oneBasedPageNumber);

        /// <summary>
        /// Get number of pages to print
        /// </summary>
        /// <returns>The number of pages to print</returns>
        protected abstract int GetNumberOfPages();

        /// <summary>
        /// Method that when called performs the appropriate form of printing
        /// </summary>
        /// <returns></returns>
        public abstract Task PrintAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if no immediate errors detected, false if an immediate error detected. Note that a true return value does not mean that printing completed successfully, just that it didn't fail immediately</returns>
        protected async Task<bool> RegisterForPrintingAsync()
        {
            try
            {
                _printDocument = new Windows.UI.Xaml.Printing.PrintDocument();
                _printDocumentSource = _printDocument.DocumentSource;

                _printDocument.Paginate += CreatePrintPreviewPages;
                _printDocument.GetPreviewPage += GetPrintPreviewPage;
                _printDocument.AddPages += AddPrintPages;
#if DEBUG
                IncrementAddPagesCount();
                IncrementGetPreviewPageCount();
                IncrementPaginateCount();
#endif

                _printManager = Windows.Graphics.Printing.PrintManager.GetForCurrentView();
                if (!(_printManager is null))
                {
                    PrintManagerHelper.Instance.SetPrintTaskRequestedHandler(
                        _printManager,
                        PrintTaskRequested);
                    return true;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            if (!(_printDocument is null))
            {
                _printDocument.Paginate -= CreatePrintPreviewPages;
                _printDocument.GetPreviewPage -= GetPrintPreviewPage;
                _printDocument.AddPages -= AddPrintPages;
            }

#if DEBUG
            DecrementPaginateCount();
            DecrementGetPreviewPageCount();
            DecrementAddPagesCount();
#endif

            await PrintStatusReporting.ReportErrorAsync(
                CrossPrinting.PrintingMessages.FailedToRegisterForPrinting);

            return false;
        }

        /// <summary>
        /// Show Print Preview UI
        /// </summary>
        /// <returns>Task that the caller is expected to await</returns>
        protected async Task ShowPrintApiAsync()
        {
            try
            {
                if (await RegisterForPrintingAsync())
                {
                    bool showprint = await Windows.Graphics.Printing.PrintManager.ShowPrintUIAsync();
                }
                else
                {
                    UnwireAllEvents();
                }
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (System.IO.FileNotFoundException fnfex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                UnwireAllEvents();
                await PrintStatusReporting.ReportInfoAsync(
                    CrossPrinting.PrintingMessages.PrinterServiceBusy);
                return;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                if ((ex.Message != null) && (ex.Message.Contains("0x80010115")))
                {
                    // {"OLE has sent a request and is waiting for a reply. (Exception from HRESULT: 0x80010115)"}

                    await PrintStatusReporting.ReportInfoAsync(
                        CrossPrinting.PrintingMessages.AwaitingResponse);
                    UnwireAllEvents();
                    return;
                }
                else
                {
                    await PrintStatusReporting.ReportExceptionAsync(ex);
                    UnwireAllEvents();
                    return;
                }
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        /// <summary>
        /// Unwire all events as part of cleanup.
        /// </summary>
        public virtual void UnwireAllEvents() => Xamarin.Forms.Device.BeginInvokeOnMainThread(
                async () =>
                {
                    try
                    {
                        if (_printManager != null)
                        {
                            PrintManagerHelper.Instance.ClearPrintTaskRequestedHandler(_printManager);
                            //_printManager.PrintTaskRequested -= PrintTaskRequested;
                            _printManager = null;
                        }

                        if (_printTask != null)
                        {
                            _printTask.Completed -= PrintTaskCompleted;
                            //_printTask.Previewing -= PrintTask_Previewing;
                            //_printTask.Progressing -= PrintTask_Progressing;
                            //_printTask.Submitting -= PrintTask_Submitting;

#if DEBUG
                            DecrementCompletedCount();
                            //DecrementPreviewing();
                            //DecrementProgressing();
                            //DecrementSubmitting();
#endif

                            Windows.Graphics.Printing.OptionDetails.PrintTaskOptionDetails printDetailedOptions
                                = Windows.Graphics.Printing.OptionDetails.PrintTaskOptionDetails.GetFromPrintTaskOptions(_printTask.Options);

                            if (printDetailedOptions != null)
                            {
                                printDetailedOptions.OptionChanged -= PrintDetailedOptions_OptionChanged;
#if DEBUG
                                DecrementOptionChangedCount();
#endif
                            }
                        }

                        if (_printDocument != null)
                        {
                            _printDocument.GetPreviewPage -= GetPrintPreviewPage;
                            _printDocument.Paginate -= CreatePrintPreviewPages;
                            _printDocument.AddPages -= AddPrintPages;

#if DEBUG
                            DecrementGetPreviewPageCount();
                            DecrementPaginateCount();
                            DecrementAddPagesCount();
#endif
                        }
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception ex)
                    {
                        await PrintStatusReporting.ReportExceptionSilentlyAsync(ex);
                    }
#pragma warning restore CA1031 // Do not catch general exception types
                });

        /// <summary>
        /// AddPages event handler
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">AddPages event args</param>
        private async void AddPrintPages(
            object sender,
            Windows.UI.Xaml.Printing.AddPagesEventArgs e)
        {
            try
            {
                // based on https://docs.microsoft.com/en-us/windows/uwp/devices-sensors/print-from-your-app

                if ((!_printJobConfiguration.AllowPageRangeSpecification)
                    || (e.PrintTaskOptions.CustomPageRanges.Count == 0))
                {
                    if (!_printJobConfiguration.PreferPrintPreview)
                        await GeneratePagesAsync();

                    // Loop over all of the preview pages and add each one to add each page to be printed
                    if ((sender is Windows.UI.Xaml.Printing.PrintDocument printDoc)
                        && (_printPreviewPages != null))
                    {
                        // Note that _printDocument.Equals(printDoc)
                        // so possible optimisation/simplification here

                        foreach (Windows.UI.Xaml.UIElement previewPage in _printPreviewPages)
                        {
                            _printDocument.AddPage(previewPage);
                        }

                        _printDocument.AddPagesComplete();
                    }
                }
                else
                {
                    // Print only the pages chosen by the user.
                    // 
                    // The "Current page" option is a special case of "Custom set of pages".
                    // In case the user selects the "Current page" option, the PrintDialog
                    // will turn that into a CustomPageRanges containing the page that the user was looking at.
                    // If the user typed in an indefinite range such as "6-", the LastPageNumber value
                    // will be whatever this sample app last passed into the PrintDocument.SetPreviewPageCount API.
                    IList<Windows.Graphics.Printing.PrintPageRange> customPageRanges = e.PrintTaskOptions.CustomPageRanges;
                    foreach (Windows.Graphics.Printing.PrintPageRange pageRange in customPageRanges)
                    {
                        // The user may type in a page number that is not present in the document.
                        // In this case, we just ignore those pages, hence the checks
                        // (pageRange.FirstPageNumber <= printPreviewPages.Count) and (i <= printPreviewPages.Count).
                        //
                        // If the user types the same page multiple times, it will be printed multiple times
                        // (e.g 3-4;1;1 will print pages 3 and 4 followed by two copies of page 1)
                        if (pageRange.FirstPageNumber <= _printPreviewPages.Count)
                        {
                            for (int i = pageRange.FirstPageNumber; (i <= pageRange.LastPageNumber) && (i <= _printPreviewPages.Count); i++)
                            {
                                // Subtract 1 because page numbers are 1-based, but our list is 0-based.
                                _printDocument.AddPage(_printPreviewPages[i - 1]);
                            }
                        }
                    }

                    _printDocument.AddPagesComplete();
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                if (sender is Windows.UI.Xaml.Printing.PrintDocument printDoc)
                    printDoc.InvalidatePreview();

                PrintManagerHelper.Instance.ClearPrintTaskRequestedHandler(_printManager);
                //_printManager.PrintTaskRequested -= PrintTaskRequested;
                await PrintStatusReporting.ReportExceptionAsync(ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        /// <summary>
        /// Create print preview pages
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Pagination event arguments</param>
        private void CreatePrintPreviewPages(
            object sender,
            Windows.UI.Xaml.Printing.PaginateEventArgs e) => SafeBlockHelper.SafeBlock(
                async () =>
            {
                if (_printJobConfiguration.AllowPageRangeSpecification)
                {
                    Windows.Graphics.Printing.OptionDetails.PrintTaskOptionDetails printDetailedOptions
                        = Windows.Graphics.Printing.OptionDetails.PrintTaskOptionDetails.GetFromPrintTaskOptions(e.PrintTaskOptions);
                    Windows.Graphics.Printing.OptionDetails.PrintPageRangeOptionDetails pageRangeOption
                        = (Windows.Graphics.Printing.OptionDetails.PrintPageRangeOptionDetails)printDetailedOptions.Options[Windows.Graphics.Printing.StandardPrintTaskOptions.CustomPageRanges];

                    // The number of pages may have been changed, so validate the Page Ranges again
                    ValidatePageRangeOption(pageRangeOption);
                }

                Windows.Graphics.Printing.PrintTaskOptions printTaskOptions = e.PrintTaskOptions;

                SetDisplayedOptions(printTaskOptions.DisplayedOptions, printTaskOptions);
                SetPrintOptions(printTaskOptions, _printJobConfiguration);

                if (sender is Windows.UI.Xaml.Printing.PrintDocument printDoc)
                {
                    await GeneratePagesAsync();

                    if (!(_printPreviewPages is null))
                    {
                        // Note that _printDocument.Equals(printDoc)
                        // so possible optimisation/simplification here

                        _printDocument.SetPreviewPageCount(0, Windows.UI.Xaml.Printing.PreviewPageCountType.Intermediate);

                        int pageNumber = 0;
                        foreach (Windows.UI.Xaml.UIElement previewPage in _printPreviewPages)
                        {
                            _printDocument.SetPreviewPage(++pageNumber, previewPage);
                        }

                        printDoc?.SetPreviewPageCount(pageNumber, Windows.UI.Xaml.Printing.PreviewPageCountType.Final);
                    }
                }
            });

        /// <summary>
        /// Handler for GetPreviewPage event
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Preview page event args</param>
        private async void GetPrintPreviewPage(
            object sender,
            Windows.UI.Xaml.Printing.GetPreviewPageEventArgs e)
        {
            try
            {
                if ((e != null) && (e.PageNumber > 0) && (_printPreviewPages.Count >= e.PageNumber))
                {
                    Windows.UI.Xaml.Printing.PrintDocument printDoc = sender as Windows.UI.Xaml.Printing.PrintDocument;
                    printDoc?.SetPreviewPage(e.PageNumber, _printPreviewPages[e.PageNumber - 1]);
                }
            }
            catch (ArgumentException aex)
            {
                await PrintStatusReporting.ReportExceptionSilentlyAsync(aex);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        /// <summary>
        /// Option change event handler
        /// </summary>
        /// <param name="sender">PrintTaskOptionsDetails object</param>
        /// <param name="args">the event arguments containing the changed option id</param>
        private void PrintDetailedOptions_OptionChanged(
            Windows.Graphics.Printing.OptionDetails.PrintTaskOptionDetails sender,
            Windows.Graphics.Printing.OptionDetails.PrintTaskOptionChangedEventArgs args)
        {
            if (_printJobConfiguration.AllowPageRangeSpecification)
            {
                string optionId = args.OptionId as string;
                if (string.IsNullOrEmpty(optionId))
                {
                    return;
                }

                // Handle change in Page Range Option
                if (optionId == Windows.Graphics.Printing.StandardPrintTaskOptions.CustomPageRanges)
                {
                    Windows.Graphics.Printing.OptionDetails.PrintPageRangeOptionDetails pageRangeOption
                        = (Windows.Graphics.Printing.OptionDetails.PrintPageRangeOptionDetails)sender.Options[optionId];
                    ValidatePageRangeOption(pageRangeOption);
                }
            }
        }

        /// <summary>
        /// PrintTaskCompleted event handler
        /// </summary>
        /// <param name="_">The sender</param>
        /// <param name="args">PrintTaskCompleted event args</param>
        /// <remarks>Note that the Completed handler is called from a thread other than the UI thread</remarks>
        private void PrintTaskCompleted(
            Windows.Graphics.Printing.PrintTask _,
            Windows.Graphics.Printing.PrintTaskCompletedEventArgs args) => Xamarin.Forms.Device.BeginInvokeOnMainThread(
                async () =>
                {
                    try
                    {
                        UnwireAllEvents();

                        bool gotError = true;
                        string message;

                        switch (args.Completion)
                        {
                            case Windows.Graphics.Printing.PrintTaskCompletion.Abandoned:
                                message = string.Format(
                                    CrossPrinting.PrintingMessages.AbandonedFormatString,
                                    (_printJobConfiguration.JobName ?? CrossPrinting.PrintingMessages.NullDescriptionString));
                                break;
                            case Windows.Graphics.Printing.PrintTaskCompletion.Canceled:
                                message = string.Format(
                                    CrossPrinting.PrintingMessages.CanceledFormatString,
                                    (_printJobConfiguration.JobName ?? CrossPrinting.PrintingMessages.NullDescriptionString));
                                gotError = false;
                                await PrintStatusReporting.ReportInfoAsync(
                                    message);
                                break;
                            case Windows.Graphics.Printing.PrintTaskCompletion.Failed:
                                message = string.Format(
                                    CrossPrinting.PrintingMessages.FailedFormatString,
                                    (_printJobConfiguration.JobName ?? CrossPrinting.PrintingMessages.NullDescriptionString));
                                break;
                            case Windows.Graphics.Printing.PrintTaskCompletion.Submitted:
                                message = string.Format(
                                    CrossPrinting.PrintingMessages.SubmittedFormatString,
                                    (_printJobConfiguration.JobName ?? CrossPrinting.PrintingMessages.NullDescriptionString));
                                gotError = false;
                                await PrintStatusReporting.ReportInfoAsync(
                                    message);
                                break;
                            default:
                                message = string.Format(
                                    CrossPrinting.PrintingMessages.DefaultFormatString,
                                    (_printJobConfiguration.JobName ?? CrossPrinting.PrintingMessages.NullDescriptionString));
                                break;
                        }

                        if (gotError)
                        {
                            await PrintStatusReporting.ReportErrorAsync(
                                message);
                        }
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception ex)
                    {
                        await PrintStatusReporting.ReportExceptionAsync(ex);
                    }
#pragma warning restore CA1031 // Do not catch general exception types
                });

        /// <summary>
        /// PrintTaskRequested event handler
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">PrintTaskRequested event args</param>
        private async void PrintTaskRequested(
            Windows.Graphics.Printing.PrintManager sender,
            Windows.Graphics.Printing.PrintTaskRequestedEventArgs e)
        {
            try
            {
                //var deff = e?.Request?.GetDeferral();
                //if (deff != null)
                if (e?.Request != null)
                {
                    _printTask = e.Request.CreatePrintTask(
                        _printJobConfiguration.JobName,
                        sourceRequestArgs =>
                        {
                            Windows.Graphics.Printing.OptionDetails.PrintTaskOptionDetails printDetailedOptions
                                = Windows.Graphics.Printing.OptionDetails.PrintTaskOptionDetails.GetFromPrintTaskOptions(_printTask.Options);

                            SetDisplayedOptions(printDetailedOptions.DisplayedOptions, _printTask?.Options);

                            // Register the handler for the option change event
                            printDetailedOptions.OptionChanged += PrintDetailedOptions_OptionChanged;

                            SetPrintOptions(_printTask.Options, _printJobConfiguration);

                            _printTask.Completed += PrintTaskCompleted;

#if DEBUG
                            IncrementOptionChangedCount();
                            IncrementCompletedCount();
#endif

                            if (!(sourceRequestArgs is null))
                                sourceRequestArgs.SetSource(_printDocumentSource);

                        });

                    _printTask.IsPreviewEnabled = _printJobConfiguration.PreferPrintPreview;

                    //deff.Complete();
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                await PrintStatusReporting.ReportExceptionAsync(ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        /// <summary>
        /// Create list of options that we want available on the Print Preview page
        /// </summary>
        /// <param name="displayedOptions">The list of options in text form to update</param>
        /// <param name="printTaskOptions">The print task options object to update</param>
        private void SetDisplayedOptions(
            IList<string> displayedOptions,
            Windows.Graphics.Printing.PrintTaskOptions printTaskOptions)
        {
            {
                // Choose the printer options to be shown.
                // The order in which the options are appended determines the order in which they appear in the UI
                displayedOptions.Clear();

                displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.Copies);

                if (_printJobConfiguration.AllowPageRangeSpecification)
                {
                    displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.CustomPageRanges);

                    if (printTaskOptions?.PageRangeOptions != null)
                    {
                        printTaskOptions.PageRangeOptions.AllowCurrentPage = false;
                        printTaskOptions.PageRangeOptions.AllowAllPages = true;
                        printTaskOptions.PageRangeOptions.AllowCustomSetOfPages = true;
                    }
                }

                displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.Orientation);
                //displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.MediaSize);
                //displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.Collation);
                displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.Duplex);
                //displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.Binding);
                //displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.Bordering);
                //displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.ColorMode);
                displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.HolePunch);
                displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.InputBin);
                displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.MediaType);
                //displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.NUp);
                displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.PrintQuality);
                displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.Staple);
            }
        }

        /// <summary>
        /// Set print task options based on the content of the print configuration
        /// </summary>
        /// <param name="printTaskOptions">PrintTaskOptions to change</param>
        /// <param name="printJobConfiguration">Configuration information to use as basis of changes to make to PrintTaskOptions</param>
        private static void SetPrintOptions(
            Windows.Graphics.Printing.PrintTaskOptions printTaskOptions,
            PrintJobConfiguration printJobConfiguration)
        {
            // TODO - investigate why setting these seems to have no effect on what defaults show in the print preview dialog etc

            printTaskOptions.ColorMode
                = printJobConfiguration.PreferColor
                    ? PrintColorMode.Color
                    : PrintColorMode.Grayscale;

            switch (printJobConfiguration.DuplexPreference)
            {
                case PrintJobDuplexConfiguration.None:
                    printTaskOptions.Duplex = PrintDuplex.OneSided;
                    break;
                case PrintJobDuplexConfiguration.LongEdge:
                    printTaskOptions.Duplex = PrintDuplex.TwoSidedLongEdge;
                    break;
                case PrintJobDuplexConfiguration.ShortEdge:
                    printTaskOptions.Duplex = PrintDuplex.TwoSidedShortEdge;
                    break;
            }

            printTaskOptions.Orientation
                = printJobConfiguration.PreferPortraitOrientation
                    ? PrintOrientation.Portrait
                    : PrintOrientation.Landscape;
        }

        /// <summary>
        /// Deals with validating that the Page Ranges only contain pages that are present in the document.
        /// 
        /// This is not necessary and some apps don't do this validation. Instead, they just ignore the pages
        /// that are not present in the document and simply don't print them.
        /// </summary>
        /// <param name="pageRangeOption">The PrintPageRangeOptionDetails option</param>
        private void ValidatePageRangeOption(
            Windows.Graphics.Printing.OptionDetails.PrintPageRangeOptionDetails pageRangeOption)
        {
            if (_printJobConfiguration.AllowPageRangeSpecification)
            {
                IList<Windows.Graphics.Printing.PrintPageRange> pageRanges
                    = ((IList<Windows.Graphics.Printing.PrintPageRange>)pageRangeOption.Value).ToImmutableList();

                pageRangeOption.WarningText = "";
                pageRangeOption.ErrorText = "";

                // An empty list means AllPages, in which case we don't need to check the ranges
                if (pageRanges.Count > 0)
                {
                    lock (_printPreviewPages)
                    {
                        // Verify that the page ranges contain at least one printable page
                        bool containsAtLeastOnePrintablePage = false;
                        foreach (Windows.Graphics.Printing.PrintPageRange pageRange in pageRanges)
                        {
                            if ((pageRange.FirstPageNumber <= _printPreviewPages.Count) || (pageRange.LastPageNumber <= _printPreviewPages.Count))
                            {
                                containsAtLeastOnePrintablePage = true;
                                break;
                            }
                        }

                        if (containsAtLeastOnePrintablePage)
                        {
                            // Warn the user in case one of the page ranges contains pages that will not printed. That way, s/he
                            // can fix the page numbers in case they were mistyped.
                            foreach (Windows.Graphics.Printing.PrintPageRange pageRange in pageRanges)
                            {
                                if ((pageRange.FirstPageNumber > _printPreviewPages.Count) || (pageRange.LastPageNumber > _printPreviewPages.Count))
                                {
                                    pageRangeOption.WarningText = CrossPrinting.PrintingMessages.PagesNotPresentWarningText;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            pageRangeOption.ErrorText = CrossPrinting.PrintingMessages.PagesNotPresentErrorString;
                        }
                    }
                }
            }
        }

        //private void OnPrintTaskSourceRequested(Windows.Graphics.Printing.PrintTaskSourceRequestedArgs args)
        //{
        //    _printTask.Completed += PrintTaskCompleted;
        //    if (!(args is null))
        //        args.SetSource(_printDocumentSource);

        //    //await InsightsWrapper.ExecuteSafeTask(OnPrintTaskSourceRequestedAsync(args));
        //}


        //private void PrintTask_Previewing(
        //    Windows.Graphics.Printing.PrintTask sender,
        //    object args)
        //{
        //    // no-op
        //}

        //private void PrintTask_Progressing(
        //    Windows.Graphics.Printing.PrintTask sender,
        //    Windows.Graphics.Printing.PrintTaskProgressingEventArgs args)
        //{
        //    // no-op
        //}

        //private void PrintTask_Submitting(
        //    Windows.Graphics.Printing.PrintTask sender,
        //    object args)
        //{
        //    // no-op
        //}

    } // internal abstract class BasePrintJob

} // namespace Plugin.Printing

#pragma warning restore IDE0007 // Use implicit type

// eof

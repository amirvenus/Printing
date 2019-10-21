#pragma warning disable IDE0007 // Use implicit type

using System.Collections.Generic;
using System.Linq;

using Windows.Foundation;
using Windows.Graphics.Printing;

namespace Plugin.Printing
{
    internal sealed class PrintManagerHelper
    {
        private static PrintManagerHelper _instance;
        public static PrintManagerHelper Instance
            => _instance ?? (_instance = new PrintManagerHelper());

        private readonly Dictionary<PrintManager, TypedEventHandler<PrintManager, PrintTaskRequestedEventArgs>> _printManagerDictionary
            = new Dictionary<PrintManager, TypedEventHandler<PrintManager, PrintTaskRequestedEventArgs>>();

        // TODO - change to have a dictionary of these, keyed by PrintManager
        //private TypedEventHandler<PrintManager, PrintTaskRequestedEventArgs> _printTaskRequestedHandler;

        /// <summary>
        /// Constructor
        /// </summary>
        private PrintManagerHelper()
        {
            // no-op
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="printManager"></param>
        /// <param name="printTaskRequested"></param>
        public void SetPrintTaskRequestedHandler(
            PrintManager printManager,
            TypedEventHandler<PrintManager, PrintTaskRequestedEventArgs> printTaskRequested)
        {
            if (!(printManager is null))
            {
                ClearPrintTaskRequestedHandler(printManager);

                _printManagerDictionary[printManager] = printTaskRequested;
                //_printTaskRequestedHandler = printTaskRequested;
                printManager.PrintTaskRequested += printTaskRequested;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="printManager"></param>
        public void ClearPrintTaskRequestedHandler(
            PrintManager printManager)
        {
            if (!(printManager is null))
            {
                if (_printManagerDictionary.ContainsKey(printManager))
                {
                    TypedEventHandler<PrintManager, PrintTaskRequestedEventArgs> printTaskRequestedHandler
                        = _printManagerDictionary[printManager];

                    if (!(printTaskRequestedHandler is null))
                    {
                        printManager.PrintTaskRequested -= printTaskRequestedHandler;
                    }

                    _printManagerDictionary.Remove(printManager);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void TearDown()
        {
            while (Instance._printManagerDictionary.Count > 0)
            {
                Instance.ClearPrintTaskRequestedHandler(Instance._printManagerDictionary.Keys.First());
            }
        }

    } // internal sealed class PrintManagerHelper

} // namespace Plugin.Printing

#pragma warning restore IDE0007 // Use implicit type

// eof

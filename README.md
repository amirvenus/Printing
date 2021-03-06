## Printing Plugin for Xamarin.Forms (Android, iOS and UWP)

Simple cross platform plugin to provide printing functionality for Xamarin.Forms projects.

### Acknowledgements

In developing this NuGet, code snippets have been used from various sources, including:

* https://github.com/bushbert/XamarinPCLPrinting/blob/master/PCLPrintExample/PCLPrintExample/PCLPrintExample.Android/Print.cs
* https://github.com/Pujolsluis/FormsPrintSample
* https://developer.android.com/training/printing/html-docs
* https://forums.xamarin.com/discussion/42417/printing-on-ipad-from-webview
* https://docs.microsoft.com/en-us/windows/uwp/devices-sensors/print-from-your-app

If the authors of those sources believe that this NuGet has infringed on any intellectual property rights, please let me know so that a resolution can be found. As far as I am aware, no infringements exist.

### Setup

* Available on NuGet: http://www.nuget.org/packages/Xam.Plugin.Printing  [![NuGet](https://img.shields.io/nuget/v/Xam.Plugin.Printing.svg?label=NuGet)](https://www.nuget.org/packages/Xam.Plugin.Printing/)

### Build Status

This NuGet plugin has been built using VS2017. 

Attempting to use VS2019 (v16.3.1) to do the same does not work out-of-the-box. This seems to be a known issue in VS2019 for which there are fixes reported online. I haven't put one of those fixes in yet, so have been using VS2017 thus far.

Note that this does not affect using the NuGet, just building the NuGet.


## Platform Support

|Platform|Version|More Detail|
| -------------------  | :------------------: | :------------------: |
|Xamarin.iOS|Any?|Tested on 12+|
|Xamarin.Android|5.0+|(Lollipop, version 21+)|
|Windows 10 UWP|Build 1903+||


## Known Issues / Work-In-Progress

### General
* It may be possible to build/use this NuGet to work with target platform versions lower than those shown above (particularly for UWP). I haven't had the time to do that yet.

### Android
* When used on Android, this NuGet will print PDFs that are version 1.6+ . PDFs below version 1.6 will result in an exception reporting them as malformed.

### iOS
* The iOS implementation currently uses UIWebView, which is deprecated. This needs to be replaced by WKWebView to avoid potential issues when submitting apps to the App Store. See https://forums.xamarin.com/discussion/168620/itms-90338-non-public-api-usage-apple-rejected-my-app-xamarin#latest

### UWP
* ```PrintJobConfiguration``` values are not currently taking effect, neither showing as the defaults on the Print Preview page, nor in controlling the printer.
* WebView and URL printing is not currently scaling the output to fit one page correctly, nor allowing output to be more than one page in length.


## License
Under MIT, see LICENSE file.


## API Usage

### Android

#### Activity
On Android, the printing functionality needs access to the main Activity of the using application.
To provide the Activity, simply do the following in your Android project's Activity subclass, typically in MainActivity.
```
Plugin.Printing.PrintServiceAndroidHelper.ActivityInstance = this;
```

#### WebView Printing
On Android, for requirements regarding permissions, and details of any constraints, see the Android developer documentation at https://developer.android.com/training/printing/html-docs

### Internationalisation
Messages generated by the printing functionality are, by default, in UK English.
If you wish to replace these messages with your own, whether in another language or not, provide an implementation of ```IBasePrintingMessages``` and then utilise as follows:
```
Plugin.Printing.CrossPrinting.PrintingMessages = new YourMessages(); // where YourMessages implements Plugin.Printing.IBasePrintingMessages
```

### Exceptions
Exceptions raised by the NuGet code have their PresetHelpLink property populated.
If you wish to replace the PresetHelpLink with your own, simply do:
```
Plugin.Printing.CrossPrinting.PresetHelpLink = "{your URL goes here}";
```

### Exception and Status Reporting
The printing functionality can report exception and status information back to the calling app for it to handle, whether hooking into App Center, popping up toasts etc.
To wire up the necessary delegates/callbacks, call SetPrintStatusReporting with four delegates. An example follows:
```
Plugin.Printing.PrintStatusReporting.SetPrintStatusReporting(
    DisplayErrorToastThatCanBeAcceptedAsync,
    ReportExceptionAsync,
    ReportSilentExceptionAsync,
    DisplayInfoToastThatCanBeAcceptedAsync);
```
where the method signatures are as follows:
```
private static async Task ReportExceptionAsync( // Note that ReportExceptionDelegateAsync can be called from a non-UI thread
    Exception ex,
    string memberName,
    string sourceFilePath,
    int sourceLineNumber)
{
    // your code goes here
}

private static async Task ReportSilentExceptionAsync( // Note that ReportExceptionDelegateAsync can be called from a non-UI thread
    Exception ex,
    string memberName,
    string sourceFilePath,
    int sourceLineNumber)
{
    // your code goes here
}

private static async Task DisplayErrorToastThatCanBeAcceptedAsync(
    string messageText)
{
    // your code goes here
}

public static async Task DisplayInfoToastThatCanBeAcceptedAsync(
    string messageText)
{
    // your code goes here
}
```

### Print Job Configuration
The ```PrintJobConfiguration``` class is used to provide cross-platform configuration of print jobs. Because of how the different platforms work, some of the print job configuration should be considered as hints/preferences to the operating system, rather than being absolute requirements.
For example, setting ```PreferPortraitOrientation``` to true will provide Portrait orientation on most platforms, but might be overriden on another platform based on the type of thing being printed.
Similarly, setting ```PreferPrintPreview``` to false might disable print preview on one platform, but not another.

To specify print job configuration, instantiate a PrintJobConfiguration object, providing a jobName as the first argument.
It is recommended that jobName be kept simple, as it may be used as part of a file name on each platform. Therefore, stick to names that will be acceptable as filenames on all target platforms (typically no white space, lower case only, limited length).
The constructor signature is as follows:
```
public PrintJobConfiguration(string jobName, bool preferPrintPreview = true)
```

The other properties of ```PrintJobConfiguration``` are as follows:
```
public bool AllowPageRangeSpecification { get; set; }
public PrintJobDuplexConfiguration DuplexPreference { get; set; } = PrintJobDuplexConfiguration.LongEdge;
public bool PreferColor { get; set; } = true;
public bool PreferFitToPage { get; set; } = true;
public bool PreferPortraitOrientation { get; set; } = true;
public bool PreferPrintPreview { get; set; }
```

### Testing which APIs are present on a platform
All of these return true on Android, iOS and UWP. However, you may still wish to check these properties in case this NuGet is extended in future to include other platforms where not all printing options are available.
```
Plugin.Printing.CrossPrinting.Current.PrintFromURLAsyncSupported
Plugin.Printing.CrossPrinting.Current.PrintImageFromByteArrayAsyncSupported
Plugin.Printing.CrossPrinting.Current.PrintImageFromStreamAsyncSupported
Plugin.Printing.CrossPrinting.Current.PrintPdfFromStreamAsyncSupported
Plugin.Printing.CrossPrinting.Current.PrintWebViewAsyncSupported
```

### Printing a PDF from a stream
```
await Plugin.Printing.CrossPrinting.Current.PrintPdfFromStreamAsync(
    printStream,
    new Plugin.Printing.PrintJobConfiguration("jobname", true));
```

### Printing an image from a byte array
```
await Plugin.Printing.CrossPrinting.Current.PrintImageFromByteArrayAsync(
    imageBytes, 
    new Plugin.Printing.PrintJobConfiguration("jobname", true));
```

### Printing an image from a stream
```
await Plugin.Printing.CrossPrinting.Current.PrintImageFromStreamAsync(
    stream, 
    new Plugin.Printing.PrintJobConfiguration("jobname", true));
```

### Printing from a URL
```
await Plugin.Printing.CrossPrinting.Current.PrintFromURLAsync(
    url,
    new Plugin.Printing.PrintJobConfiguration("jobname", true));
```

### Printing a WebView
Note that this requires an implementation of an interface as the third argument.
Implementing that interface requires further work on the part of the caller, so some users may wish to avoid this API for the time being.
```
await Plugin.Printing.CrossPrinting.Current.PrintWebViewAsync(
    page,
    webView, 
    additionalFunctions,
    new Plugin.Printing.PrintJobConfiguration("jobname", true));
```

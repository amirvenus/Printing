#pragma warning disable IDE0007 // Use implicit type
#pragma warning disable IDE0063 // Use simple 'using' statement

using System;
using System.IO;
using System.Threading.Tasks;

using Windows.Storage.Streams;

namespace Plugin.Printing
{
    internal static class StreamHelper
    {
        /// <summary>
        /// Convert the given MemoryStream to a stream that allows random access
        /// </summary>
        /// <param name="memoryStream">The MemoryStream to convert</param>
        /// <returns>Stream that allows random access</returns>
        public static async Task<IRandomAccessStream> ConvertToRandomAccessStream(MemoryStream memoryStream)
        {
            InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
            MemoryStream contentStream = new MemoryStream();
            memoryStream.CopyTo(contentStream);
            using (IOutputStream outputStream = randomAccessStream.GetOutputStreamAt(0))
            {
                using (DataWriter dw = new DataWriter(outputStream))
                {
                    Task task = new Task(() => dw.WriteBytes(contentStream.ToArray()));
                    task.Start();

                    await task;
                    await dw.StoreAsync();

                    await outputStream.FlushAsync();
                    await dw.FlushAsync();
                    outputStream.Dispose();
                    dw.DetachStream();
                    dw.Dispose();
                }
            }
            return randomAccessStream;
        }

    } // internal static class StreamHelper

} // namespace Plugin.Printing

#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore IDE0007 // Use implicit type

// eof

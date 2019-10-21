using System;
using System.Runtime.Serialization;

namespace Plugin.Printing
{
    /// <summary>
    /// Exception used when reporting exceptions during print operations
    /// </summary>
    [Serializable]
    public sealed class PrintJobException : Exception
    {
        /// <summary>
        /// Property that specifies the HelpLink to use when populating new PrintJobException instances
        /// </summary>
        public static string PresetHelpLink { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public PrintJobException()
            : base()
        {
            HelpLink = PresetHelpLink;
        }

        /// <summary>
        /// Constructor with message text
        /// </summary>
        /// <param name="message">Message text</param>
        public PrintJobException(string message)
            : base(message)
        {
            HelpLink = PresetHelpLink;
        }

        /// <summary>
        /// Constructor used when wrapping another exception
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="innerException">The exception to be wrapped</param>
        public PrintJobException(string message, Exception innerException)
            : base(message, innerException)
        {
            HelpLink = PresetHelpLink;
        }

        /// <summary>
        /// Constructor used during serialization
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        private PrintJobException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            HelpLink = PresetHelpLink;
        }

    } // public sealed class PrintJobException : Exception

} // namespace Plugin.Printing

// eof

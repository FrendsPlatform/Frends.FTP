using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Core;
using Serilog.Events;

namespace Frends.FTP.UploadFiles.Logging
{
    /// <summary>
    /// Sink that is used to store messages and events from seriolog Logger
    /// </summary>
    internal class TransferLogSink : ILogEventSink
    {
        /// <summary>
        ///     Always store some initial log messages first
        /// </summary>
        private const int DefaultInitialLogMessages = 20;

        private readonly List<Tuple<DateTimeOffset, string>> allMsgsBuffer;
        private readonly CircularBuffer<Tuple<DateTimeOffset, string>> circularBuffer;

        private readonly List<Tuple<DateTimeOffset, string>> initialLogMessages;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransferLogSink"/> class.
        /// Constructor
        /// </summary>
        /// <param name="maxLogEntries">Maximum number of log entries to keep, or null to keep all entries.</param>
        public TransferLogSink(int? maxLogEntries)
        {
            if (maxLogEntries != null)
            {
                initialLogMessages = new List<Tuple<DateTimeOffset, string>>(DefaultInitialLogMessages);
                circularBuffer = new CircularBuffer<Tuple<DateTimeOffset, string>>(maxLogEntries.Value);
            }
            else
            {
                allMsgsBuffer = new List<Tuple<DateTimeOffset, string>>();
            }
        }

        /// <summary>
        /// Emits new messages to the sink
        /// </summary>
        /// <param name="logEvent">Log event to emit.</param>
        public void Emit(LogEvent logEvent)
        {
            if (allMsgsBuffer != null)
            {
                allMsgsBuffer.Add(new Tuple<DateTimeOffset, string>(logEvent.Timestamp, logEvent.RenderMessage()));
            }
            else
            {
                if (initialLogMessages.Count < DefaultInitialLogMessages)
                {
                    initialLogMessages.Add(
                        new Tuple<DateTimeOffset, string>(logEvent.Timestamp, logEvent.RenderMessage()));
                }
                else
                {
                    circularBuffer.Add(
                        new Tuple<DateTimeOffset, string>(logEvent.Timestamp, logEvent.RenderMessage()));
                }
            }
        }

        /// <summary>
        /// Gets the log messages from sink
        /// </summary>
        /// <returns>The buffered log messages.</returns>
        public IList<Tuple<DateTimeOffset, string>> GetBufferedLogMessages()
        {
            if (allMsgsBuffer != null) return allMsgsBuffer;

            var bufferedMessages = circularBuffer.Latest();
            if (bufferedMessages.Any())
            {
                return initialLogMessages
                    .Concat(new[] { Tuple.Create(DateTimeOffset.MinValue, "...") })
                    .Concat(bufferedMessages).ToList();
            }

            return initialLogMessages;
        }
    }
}

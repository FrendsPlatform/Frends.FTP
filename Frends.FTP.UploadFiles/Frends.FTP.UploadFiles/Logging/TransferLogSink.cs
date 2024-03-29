﻿using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private readonly IList<Tuple<DateTimeOffset, string>> _allMsgsBuffer;
        private readonly CircularBuffer<Tuple<DateTimeOffset, string>> _circularBuffer;

        private readonly IList<Tuple<DateTimeOffset, string>> _initialLogMessages;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxLogEntries"></param>
        public TransferLogSink(int? maxLogEntries)
        {
            if (maxLogEntries != null)
            {
                _initialLogMessages = new List<Tuple<DateTimeOffset, string>>(DefaultInitialLogMessages);
                _circularBuffer = new CircularBuffer<Tuple<DateTimeOffset, string>>(maxLogEntries.Value);
            }
            else
            {
                _allMsgsBuffer = new List<Tuple<DateTimeOffset, string>>();
            }
        }

        /// <summary>
        /// Emits new messages to the sink
        /// </summary>
        /// <param name="logEvent"></param>
        public void Emit(LogEvent logEvent)
        {
            if (_allMsgsBuffer != null)
            {
                _allMsgsBuffer.Add(new Tuple<DateTimeOffset, string>(logEvent.Timestamp, logEvent.RenderMessage()));
            }
            else
            {
                if (_initialLogMessages.Count < DefaultInitialLogMessages)
                    _initialLogMessages.Add(
                        new Tuple<DateTimeOffset, string>(logEvent.Timestamp, logEvent.RenderMessage()));
                else
                    _circularBuffer.Add(
                        new Tuple<DateTimeOffset, string>(logEvent.Timestamp, logEvent.RenderMessage()));
            }
        }

        /// <summary>
        /// Gets the log messages from sink
        /// </summary>
        /// <returns></returns>
        public IList<Tuple<DateTimeOffset, string>> GetBufferedLogMessages()
        {
            if (_allMsgsBuffer != null) return _allMsgsBuffer;

            var bufferedMessages = _circularBuffer.Latest();
            if (bufferedMessages.Any())
                return _initialLogMessages
                    .Concat(new[] { Tuple.Create(DateTimeOffset.MinValue, "...") })
                    .Concat(bufferedMessages).ToList();

            return _initialLogMessages;
        }
    }
}
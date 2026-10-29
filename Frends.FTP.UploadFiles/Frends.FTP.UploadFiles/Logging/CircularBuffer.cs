using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Frends.FTP.UploadFiles.Logging
{
    /// <summary>
    /// Circular buffer impl, original from https://codereview.stackexchange.com/a/134147
    /// </summary>
    /// <typeparam name="T">Type of the buffered items.</typeparam>
    internal class CircularBuffer<T>
    {
        private readonly ConcurrentQueue<T> data;
        private readonly ReaderWriterLockSlim @lock = new ReaderWriterLockSlim();
        private readonly int size;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class.
        /// Constructor
        /// </summary>
        /// <param name="size">Maximum number of items kept in the buffer.</param>
        /// <exception cref="ArgumentException">Thrown when the size is less than one.</exception>
        public CircularBuffer(int size)
        {
            if (size < 1) throw new ArgumentException($"{nameof(size)} cannot be negative or zero");
            data = new ConcurrentQueue<T>();
            this.size = size;
        }

        /// <summary>
        /// Get latest data in a readonly list
        /// </summary>
        /// <returns>The latest items in the buffer.</returns>
        public IReadOnlyList<T> Latest()
        {
            return data.ToArray();
        }

        /// <summary>
        /// Add data to the buffer.
        /// </summary>
        /// <param name="t">Item to add to the buffer.</param>
        public void Add(T t)
        {
            @lock.EnterWriteLock();
            try
            {
                if (data.Count == size)
                {
                    T value;
                    data.TryDequeue(out value);
                }

                data.Enqueue(t);
            }
            finally
            {
                @lock.ExitWriteLock();
            }
        }
    }
}

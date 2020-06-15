using System;
using System.Collections.Generic;
using System.Text;

namespace Oceana.Core
{
    /// <summary>
    /// A simple circular buffer.
    /// </summary>
    /// <typeparam name="T">Type of data to be stored in the buffer.</typeparam>
    public class CircularBuffer<T>
    {
        private readonly T[] Buffer;
        private readonly object Lock;

        private int WriteHead;
        private int ReadHead;

        /// <summary>
        /// Initialises a new instance of the <see cref="CircularBuffer{T}"/> class.
        /// </summary>
        /// <param name="size">The requested size of the buffer.</param>
        public CircularBuffer(int size)
        {
            Buffer = new T[size];
            Lock = new object();

            WriteHead = 0;
            ReadHead = 0;
            ItemsAvailable = 0;
        }

        /// <summary>
        /// Gets the maximum size of the buffer.
        /// </summary>
        public int Length => Buffer.Length;

        /// <summary>
        /// Gets the number of items currently stored in the buffer.
        /// </summary>
        public int ItemsAvailable { get; private set; }

        private int SpaceLeft => Buffer.Length - ItemsAvailable;

        /// <summary>
        /// Writes data to the buffer.
        /// </summary>
        /// <param name="data">Data to be written to the buffer.</param>
        /// <returns>The number of items written.</returns>
        public int Write(T[] data)
        {
            _ = data ?? throw new ArgumentNullException(nameof(data));

            var count = data.Length;

            lock (Lock)
            {
                if (count > SpaceLeft)
                {
                    count = SpaceLeft;
                }

                // Write to end
                var itemsToEnd = Math.Min(Buffer.Length - WriteHead, count);
                Array.Copy(data, 0, Buffer, WriteHead, itemsToEnd);
                WriteHead += itemsToEnd;

                if (itemsToEnd != count)
                {
                    WriteHead = 0;
                    Array.Copy(data, count - itemsToEnd, Buffer, WriteHead, count - itemsToEnd);
                    WriteHead += count - itemsToEnd;
                }

                WriteHead %= Buffer.Length;
                ItemsAvailable += count;
            }

            return count;
        }

        /// <summary>
        /// Reads data from the buffer.
        /// </summary>
        /// <param name="requestedItems">The requested number of items to
        /// retrieve.</param>
        /// <returns>The data read from the buffer.</returns>
        public T[] Read(int requestedItems)
        {
            T[] result;
            var count = requestedItems;

            lock (Lock)
            {
                if (count > ItemsAvailable)
                {
                    count = ItemsAvailable;
                }

                result = new T[count];

                // Read to the end
                var itemsToEnd = Math.Min(Buffer.Length - ReadHead, count);
                Array.Copy(Buffer, ReadHead, result, 0, itemsToEnd);
                ReadHead += itemsToEnd;

                if (itemsToEnd != count)
                {
                    ReadHead = 0;
                    Array.Copy(Buffer, ReadHead, result, count - itemsToEnd, count - itemsToEnd);
                    ReadHead += count - itemsToEnd;
                }

                ReadHead %= Buffer.Length;
                ItemsAvailable -= count;
            }

            return result;
        }
    }
}

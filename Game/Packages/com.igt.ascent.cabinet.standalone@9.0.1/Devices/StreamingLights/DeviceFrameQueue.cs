//-----------------------------------------------------------------------
// <copyright file = "DeviceFrameQueue.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.StreamingLights
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Presentation.PeripheralLights.Streaming;

    /// <summary>
    /// Manages the software queue for streaming frames to a light device.
    /// </summary>
    internal class DeviceFrameQueue
    {
        /// <summary>
        /// The default display time for a frame.
        /// </summary>
        private const ushort DefaultFrameDisplayTime = 33;

        /// <summary>
        /// The maximum amount of time (in milliseconds) that can be queued up at once. Higher values
        /// increase the reaction time of the device.
        /// </summary>
        private const uint MaximumDisplayTimeInQueue = 200;

        private readonly Queue<Chunk> inputQueue = new Queue<Chunk>();

        /// <summary>
        /// Construct a new instance using the device queue size.
        /// </summary>
        /// <param name="queueSize">The size of the frame queue on the device.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="queueSize"/> is zero.
        /// </exception>
        public DeviceFrameQueue(ushort queueSize)
        {
            if(queueSize == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(queueSize), "The queue size must be larger than zero.");
            }

            QueueSize = queueSize;
            // By default make the threshold half the size of the queue. If the queue can only
            // hold one frame then that is the threshold.
            FrameDisplayThreshold = (ushort)((queueSize > 1 ? queueSize / 2 : 1) * DefaultFrameDisplayTime);
            FrameQueue = new Queue<Frame>(queueSize);
            QueueLock = new object();
        }

        #region Properties

        /// <summary>
        /// Gets if the input queue is empty or not.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                lock(QueueLock)
                {
                    return inputQueue.Count == 0;
                }
            }
        }

        /// <summary>
        /// The size of the frame queue on the device.
        /// </summary>
        public ushort QueueSize
        {
            get;
        }

        /// <summary>
        /// How much display time should be left in the device
        /// before it asks for more frames. Expressed in milliseconds.
        /// </summary>
        public ushort FrameDisplayThreshold
        {
            get;
            private set;
        }

        /// <summary>
        /// The queue of frames that can be sent to the device.
        /// </summary>
        private Queue<Frame> FrameQueue
        {
            get;
        }

        /// <summary>
        /// The lock object to use when accessing the queues.
        /// </summary>
        private object QueueLock { get; }

        /// <summary>
        /// Gets or sets the current index into the top chunk in the input queue.
        /// </summary>
        private int FrameIndex { get; set; }

        #endregion

        #region Get Frames

        /// <summary>
        /// Gets the maximum number of frames the device can support or
        /// are in the light sequence, whichever is greater.
        /// </summary>
        /// <param name="numberOfFramesReturned">
        /// The number of frames represented in the byte array returned.
        /// </param>
        /// <returns>The serialized light frames.</returns>
        public byte[] GetFrames(out ushort numberOfFramesReturned)
        {
            return GetFrames(QueueSize, out numberOfFramesReturned);
        }

        /// <summary>
        /// Gets light frames from the queue.
        /// </summary>
        /// <param name="numberOfFramesRequested">
        /// The number of frames to pull from the queue.
        /// </param>
        /// <param name="numberOfFramesReturned">
        /// The number of frames represented in the byte array returned.
        /// </param>
        /// <returns>The serialized light frames.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if either <paramref name="numberOfFramesRequested"/> is zero or
        /// if <paramref name="numberOfFramesRequested"/> is larger than <see cref="QueueSize"/>.
        /// </exception>
        public byte[] GetFrames(ushort numberOfFramesRequested, out ushort numberOfFramesReturned)
        {
            if(numberOfFramesRequested > QueueSize)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfFramesRequested),
                    $"The number of frames requested cannot be larger than the queue size. Requested: {numberOfFramesRequested} Maximum Allowed: {QueueSize}");
            }

            if(numberOfFramesRequested == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfFramesRequested),
                                                      "Cannot request zero frames.");
            }

            numberOfFramesReturned = 0;
            byte[] frameData;
            using(var stream = new MemoryStream())
            {
                lock(QueueLock)
                {
                    while(numberOfFramesReturned <= numberOfFramesRequested &&
                          FrameQueue.Count > 0)
                    {
                        var frame = FrameQueue.Dequeue();
                        frame.SerializeByVersion(stream, 1);
                        numberOfFramesReturned++;
                    }
                }

                stream.Seek(0, SeekOrigin.Begin);
                frameData = new byte[stream.Length];
                stream.Read(frameData, 0, frameData.Length);
            }

            FillQueue();

            return frameData;
        }

        #endregion

        /// <summary>
        /// Adds a chunk to the input queue. If the play mode is restart, anything already in the
        /// input queue is cleared. For queue, the chunk is enqueued into the input queue.
        /// </summary>
        /// <param name="chunk">The chunk to add.</param>
        /// <param name="playMode">The play mode to use.</param>
        /// <param name="identifier">The identifier of the chunk being added.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="chunk"/> is null.
        /// </exception>
        public void AddChunk(IList<Frame> chunk, StreamingLightsPlayMode playMode, byte identifier)
        {
            if(chunk == null)
            {
                throw new ArgumentNullException(nameof(chunk));
            }

            if(chunk.Count == 0)
            {
                throw new ArgumentException("The chunk must contain at least one frame.", nameof(chunk));
            }

            switch(playMode)
            {
                case StreamingLightsPlayMode.Restart:
                    lock(QueueLock)
                    {
                        FrameQueue.Clear();
                        inputQueue.Clear();
                        inputQueue.Enqueue(new Chunk(identifier, chunk));
                        FrameIndex = 0;
                        FillQueue();
                    }
                    break;
                case StreamingLightsPlayMode.Continue:
                    lock(QueueLock)
                    {
                        if(inputQueue.Count > 0)
                        {
                            var queuedChunk = inputQueue.FirstOrDefault(item => item.Identifier == identifier);
                            if(queuedChunk != null)
                            {
                                queuedChunk.Data = chunk;
                            }
                        }
                    }
                    break;
                case StreamingLightsPlayMode.Queue:
                    lock(QueueLock)
                    {
                        inputQueue.Enqueue(new Chunk(identifier, chunk));
                    }
                    break;
            }
        }

        /// <summary>
        /// Fills the queue with frames from the sequence.
        /// </summary>
        private void FillQueue()
        {
            lock(QueueLock)
            {
                if(inputQueue.Count == 0)
                {
                    return;
                }

                uint displayTime = 0;
                // Fill the queue while it is smaller than the queue size on the device
                // and the input queue still has chunks in it.
                while(FrameQueue.Count < QueueSize &&
                      inputQueue.Count > 0)
                {
                    var currentChunk = inputQueue.Peek().Data;
                    FrameQueue.Enqueue(currentChunk[FrameIndex]);
                    displayTime += currentChunk[FrameIndex].TotalDisplayTime;
                    FrameIndex++;

                    if(FrameIndex >= currentChunk.Count)
                    {
                        FrameIndex = 0;
                        inputQueue.Dequeue();
                    }

                    if(displayTime > MaximumDisplayTimeInQueue)
                    {
                        break;
                    }
                }

                CalculateFrameThreshold();
            }
        }

        /// <summary>
        /// Calculates the frame threshold for the device queue.
        /// </summary>
        private void CalculateFrameThreshold()
        {
            // If there are more than two frames take the total and cut it in half.
            if(FrameQueue.Count > 2)
            {
                var queueTotalTime = FrameQueue.Sum(frame => frame.DisplayTime);
                FrameDisplayThreshold = (ushort)(queueTotalTime / 2);
            }
            else
            {
                // If there is only 1 or 2 frames in the queue then use the duration
                // of the first frame to trigger a refill. This is done so looping
                // segments with 1 or 2 frames work correctly.
                FrameDisplayThreshold = FrameQueue.Peek().DisplayTime;
            }
        }

        /// <summary>
        /// Represents a stored chunk in the device queue.
        /// </summary>
        private class Chunk
        {
            /// <summary>
            /// Construct a new instance given the identifier and chunk data.
            /// </summary>
            /// <param name="identifier">The chunk identifier.</param>
            /// <param name="data">The chunk frame data.</param>
            public Chunk(byte identifier, IList<Frame> data)
            {
                Identifier = identifier;
                Data = data;
            }

            /// <summary>
            /// Gets the chunk identifier.
            /// </summary>
            public byte Identifier { get; }

            /// <summary>
            /// Gets or sets the frame data.
            /// </summary>
            public IList<Frame> Data { get; set; }
        }
    }
}

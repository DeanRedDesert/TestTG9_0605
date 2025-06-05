// -----------------------------------------------------------------------
//  <copyright file = "UtpDiagnostics.cs" company = "IGT">
//      Copyright (c) 2017 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
namespace IGT.Game.Utp.Modules.ModuleComponents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Profiling;

    /// <summary>
    /// MonoBehaviour used for modules to obtain diagnostic information.
    /// </summary>
    public class UtpDiagnostics : MonoBehaviour
    {
        #region Private fields

        private int frames;
        private float accumulatedTime;
        private Queue<float> frameRates;
        private const int frameRateSamples = 40;

        #endregion

        #region Public fields

        /// <summary>
        /// Frequency (in sec) to poll.
        /// </summary>
        public float UpdateFrequency = 0.2f;

        /// <summary>
        /// Low framerate threshold. If FPS drops below this value an event will be fired.
        /// </summary>
        public float LowFrameRateThreshold = 30.0f;

        /// <summary>
        /// Low system memory threshold. If system memory drops below this value an event will be fired.
        /// </summary>
        public float SystemMemoryThreshold = 1000.0f;

        #endregion

        #region Public properties

        /// <summary>
        /// Get the total RAM allocated by application expressed as megabytes.
        /// </summary>
        public float TotalAllocatedMemory
        {
            get { return (float)(Profiler.GetTotalAllocatedMemoryLong()) / 1024 / 1024; }
        }

        /// <summary>
        /// Get the total managed RAM allocated by application expressed as megabytes.
        /// A number that is the best available approximation of the number of bytes currently allocated in managed memory.
        /// </summary>
        public float TotalAllocatedManagedMemory
        {
            get { return (float)GC.GetTotalMemory(false) / 1024 / 1024; }
        }

        /// <summary>
        /// Get the total RAM reserved by application expressed as megabytes.
        /// </summary>
        public float TotalReservedMemory
        {
            get { return (float)(Profiler.GetTotalReservedMemoryLong()) / 1024 / 1024; }
        }

        /// <summary>
        /// Get the current frames per second value.
        /// </summary>
        public float FramesPerSecond { get; private set; }

        /// <summary>
        /// Get the average frames per second value.
        /// </summary>
        public float AverageFramesPerSecond
        {
            get
            {
                if(frameRates.Count > 0)
                {
                    return frameRates.Average();
                }
                return 0f;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Occurs when [update available].
        /// </summary>
        public virtual event EventHandler UpdateAvailable;

        /// <summary>
        /// Event that is triggered when the framerate is lower than a certain threshold.
        /// </summary>
        public virtual event EventHandler LowFrameRate;

        /// <summary>
        /// Event that is triggered when allocated memory has exceeded threshold.
        /// </summary>
        public virtual event EventHandler LowSystemMemory;

        /// <summary>
        /// Raises the <see cref="E:UpdateAvailable" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnUpdateAvailable(EventArgs e)
        {
            EventHandler handler = UpdateAvailable;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Informs that the frame rate is lower than a certain threshold.
        /// </summary>
        /// <param name="e">Event arguments for the low framerate.</param>
        protected virtual void OnLowFramerate(EventArgs e)
        {
            EventHandler handler = LowFrameRate;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Informs that the system memory has exceeded threshold.
        /// </summary>
        /// <param name="e">Event arguments for the exceeded system memory.</param>
        protected virtual void OnSystemMemoryThresholdExceeded(EventArgs e)
        {
            EventHandler handler = LowSystemMemory;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        #region MonoBehaviour Methods

        // Use this for initialization
        private void Start()
        {
            frameRates = new Queue<float>();

            // Disable update loop by default
            enabled = false;
        }

        // Update is called once per frame
        private void Update()
        {
            ++frames;
            accumulatedTime += Time.deltaTime;
            if(accumulatedTime > UpdateFrequency)
            {
                GetFrameRate();
                MemoryCheck();

                OnUpdateAvailable(EventArgs.Empty);
            }
        }

        #endregion MonoBehaviour Methods

        #region Private Methods

        private void MemoryCheck()
        {
            if(TotalAllocatedMemory > SystemMemoryThreshold)
            {
                OnSystemMemoryThresholdExceeded(EventArgs.Empty);
            }
        }

        private void GetFrameRate()
        {
            FramesPerSecond = frames / accumulatedTime;
            frames = 0;
            accumulatedTime = 0.0F;

            frameRates.Enqueue(FramesPerSecond);
            while(frameRates.Count > frameRateSamples)
            {
                frameRates.Dequeue();
            }

            if(FramesPerSecond < LowFrameRateThreshold)
            {
                OnLowFramerate(EventArgs.Empty);
            }
        }

        #endregion Private Methods
    }
}
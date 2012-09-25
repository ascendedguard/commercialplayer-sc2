// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MonitorStatusEventArgs.cs" company="AscendTV">
//   Copyright © 2012 All Rights Reserved
// </copyright>
// <summary>
//   Event arguments for when a monitor changes its running status.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TwitchCommercialSC2
{
    using System;

    /// <summary>
    /// Event arguments for when a monitor changes its running status.
    /// </summary>
    public class MonitorStatusEventArgs : EventArgs
    {
        /// <summary> Initializes a new instance of the <see cref="MonitorStatusEventArgs"/> class. </summary>
        /// <param name="isRunning"> Whether the monitor is currently running. </param>
        public MonitorStatusEventArgs(bool isRunning)
        {
            this.IsRunning = isRunning;
        }

        /// <summary>
        /// Gets a value indicating whether the monitor is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }
    }
}

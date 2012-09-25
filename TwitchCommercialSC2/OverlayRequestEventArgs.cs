// -----------------------------------------------------------------------
// <copyright file="OverlayRequestEventArgs.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace TwitchCommercialSC2
{
    using System;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class OverlayRequestEventArgs : EventArgs
    {
        public OverlayRequestEventArgs(int delay, int seconds)
        {
            this.Delay = delay;
            this.CommercialSeconds = seconds;
        }

        public int Delay { get; private set; }

        public int CommercialSeconds { get; private set; }
    }
}

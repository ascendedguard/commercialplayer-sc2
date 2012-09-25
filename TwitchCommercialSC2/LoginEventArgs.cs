// -----------------------------------------------------------------------
// <copyright file="LoginEventArgs.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace TwitchCommercialSC2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class LoginEventArgs : EventArgs
    {
        public LoginEventArgs(string token)
        {
            this.Token = token;
        }

        public string Token { get; private set; }
    }
}

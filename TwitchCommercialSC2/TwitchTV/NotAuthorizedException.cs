// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotAuthorizedException.cs" company="AscendTV">
//   Copyright © 2012 All Rights Reserved
// </copyright>
// <summary>
//   Indicates an exception occured where the user was not authorized to take an action.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TwitchCommercialSC2.TwitchTV
{
    using System;

    /// <summary> Indicates an exception occured where the user was not authorized to take an action.
    /// </summary>
    public class NotAuthorizedException : Exception
    {
        /// <summary> Initializes a new instance of the <see cref="NotAuthorizedException"/> class. </summary>
        /// <param name="message"> The message. </param>
        public NotAuthorizedException(string message) : base(message)
        {
        }
    }
}

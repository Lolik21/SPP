// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IpContainer.cs" company="Ilya's company">
//   All rights reserved
// </copyright>
// <summary>
//   The internet address container.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Logger.Helpers
{
    using System;

    /// <summary>
    /// The internet address container.
    /// </summary>
    public class IpContainer
    {
        /// <summary>
        /// Gets or sets the internet address.
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Gets or sets the time.
        /// </summary>
        public TimeSpan Time { get; set; }

        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        public int Count { get; set; }
    }
}
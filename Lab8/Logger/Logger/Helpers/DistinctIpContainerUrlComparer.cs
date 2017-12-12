// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DistinctIpContainerUrlComparer.cs" company="Ilya's company">
//   All rights are reserved
// </copyright>
// <summary>
//   Defines the DistinctIpContainerUrlComparer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Logger.Helpers
{
    using System.Collections.Generic;

    /// <summary>
    /// The distinct internet address container url comparer.
    /// </summary>
    public class DistinctIpContainerUrlComparer : IEqualityComparer<IpContainer>
    {
        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <param name="y">
        /// The y.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Equals(IpContainer x, IpContainer y)
        {
            return y != null && (x != null && x.Ip == y.Ip);
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <param name="obj">
        /// The object.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int GetHashCode(IpContainer obj)
        {
            return 1;
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DistinctContainerUrlComparer.cs" company="Ilya's company">
//   All rights are reserved
// </copyright>
// <summary>
//   Defines the DistinctContainerUrlComparer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Logger.Helpers
{
    using System.Collections.Generic;

    /// <summary>
    /// The distinct container url comparer.
    /// </summary>
    public class DistinctContainerUrlComparer : IEqualityComparer<Container>
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
        public bool Equals(Container x, Container y)
        {
            return y != null && (x != null && x.Log.Url == y.Log.Url);
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
        public int GetHashCode(Container obj)
        {
            return 1;
        }
    }
}
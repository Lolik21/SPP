// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IpSource.cs" company="Ilya's company">
//   All rights are reserved
// </copyright>
// <summary>
//   The ip source.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Logger.Helpers
{
    using System;
    using System.Collections.Generic;
    using DataGenerator;
    using DataGenerator.Sources;

    /// <summary>
    /// The internet address source.
    /// </summary>
    public class IpSource : DataSourcePropertyType
    {
        /// <summary>
        /// The random.
        /// </summary>
        private readonly Random random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="IpSource"/> class.
        /// </summary>
        public IpSource()
            : base(new Type[1])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IpSource"/> class.
        /// </summary>
        /// <param name="types">
        /// The types.
        /// </param>
        public IpSource(IEnumerable<Type> types)
            : base(types)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IpSource"/> class.
        /// </summary>
        /// <param name="priority">
        /// The priority.
        /// </param>
        /// <param name="types">
        /// The types.
        /// </param>
        public IpSource(int priority, IEnumerable<Type> types)
            : base(priority, types)
        {
        }

        /// <summary>
        /// The next value.
        /// </summary>
        /// <param name="generateContext">
        /// The generate context.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public override object NextValue(IGenerateContext generateContext)
        {
            return this.GetRandomIpAddress();
        }

        /// <summary>
        /// The get random internet address address.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetRandomIpAddress()
        {
            return $"{this.random.Next(1, 255)}.{this.random.Next(0, 255)}.{this.random.Next(0, 255)}.{this.random.Next(0, 255)}";
        }
    }
}
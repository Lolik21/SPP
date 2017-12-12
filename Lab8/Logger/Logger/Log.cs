namespace Logger
{
    using System;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// The log.
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [BsonId]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the internet address.
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the time stamp.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the time spent.
        /// </summary>
        public TimeSpan TimeSpent { get; set; }
    }
}
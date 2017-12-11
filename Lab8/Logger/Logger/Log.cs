namespace Logger
{
    using System;
    using MongoDB.Bson.Serialization.Attributes;

    public class Log
    {
        [BsonId]
        public string Id { get; set; } 

        public string Ip { get; set; }

        public string Url { get; set; }

        public DateTime TimeStamp { get; set; }

        public TimeSpan TimeSpent { get; set; }
    }
}
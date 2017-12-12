// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Logger.cs" company="Ilya's company">
//   All rights are reserved
// </copyright>
// <summary>
//   The logger.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Logger
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CsvHelper;
    using DataGenerator;
    using DataGenerator.Sources;
    using global::Logger.Helpers;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    /// <summary>
    /// The logger.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// The database name.
        /// </summary>
        private static readonly string DB_NAME = "Server";

        /// <summary>
        /// The collection name.
        /// </summary>
        private static readonly string COLLECTION_NAME = "Log";

        /// <summary>
        /// The connection string.
        /// </summary>
        private static readonly string CONNECTION_STR = "mongodb://Lolik21:123qWe123@servercluster-shard-00-00-ohx52."
                                + "mongodb.net:27017,servercluster-shard-00-01-ohx52.mongodb.net:"
                                + "27017,servercluster-shard-00-02-ohx52.mongodb.net:27017/test?"
                                + "ssl=true&replicaSet=ServerCluster-shard-0&authSource=admin";

        /// <summary>
        /// The map reduce by internet address.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static IEnumerable<IpContainer> MapReduceByIp()
        {
            MongoClient client = new MongoClient(CONNECTION_STR);
            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<Log>(COLLECTION_NAME);
            var enumCollection = collection.AsQueryable().AsEnumerable();
            var results = enumCollection.Select(
                    log =>
                        {
                            TimeSpan resultSpan2 = TimeSpan.Zero;
                            var resultSpan = enumCollection.Aggregate(
                                0,
                                (span, log1) =>
                                    {
                                        if (log1.Ip == log.Ip)
                                        {
                                            resultSpan2 += log1.TimeSpent;
                                            return ++span;                                   
                                        }
                                        return span;
                                    });
                            return new IpContainer { Count = resultSpan, Time = resultSpan2, Ip = log.Ip };
                        }).Distinct(new DistinctIpContainerUrlComparer())
                        .OrderByDescending(container => container.Ip)
                        .ThenByDescending(container => container.Count)
                        .ThenByDescending(container => container.Time);
            return results;
        }

        /// <summary>
        /// The map reduce by time.
        /// </summary>
        /// <param name="min">
        /// The min.
        /// </param>
        /// <param name="max">
        /// The max.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static IEnumerable<Container> MapReduceByTime(DateTime min, DateTime max)
        {
            MongoClient client = new MongoClient(CONNECTION_STR);
            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<Log>(COLLECTION_NAME);
            var enumCollection = collection.AsQueryable().AsEnumerable();
            var results = enumCollection.Select(
                log =>
                    {
                        var resultSpan = enumCollection.Aggregate(
                            0,
                            (span, log1) => (log1.Url == log.Url) && (log1.TimeStamp >= min) && (log1.TimeStamp <= max)
                                                ? span + 1
                                                : span);
                        return new Container { Count = resultSpan, Log = log };
                    }).Distinct(new DistinctContainerUrlComparer()).OrderByDescending(log => log.Log.Url)
                    .ThenByDescending(container => container.Count);
            return results;
        }

        /// <summary>
        /// The map reduce url list sum time.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static IEnumerable<Log> MapReduceUrlsSumTime()
        {
            MongoClient client = new MongoClient(CONNECTION_STR);
            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<Log>(COLLECTION_NAME);
            var enumCollection = collection.AsQueryable().AsEnumerable();
            var results = enumCollection.Select(
                log =>
                    {
                        var resultSpan = enumCollection.Aggregate(
                            TimeSpan.Zero,
                            (span, log1) => log1.Url == log.Url ? span + log.TimeSpent : span);
                        log.TimeSpent = resultSpan;
                        return log;
                    }).Distinct(new DistinctLogUrlComparer()).OrderByDescending(log => log.Url);
            return results;
        }

        /// <summary>
        /// The map reduce url list sum count.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static IEnumerable<Container> MapReduceUrlsSumCount()
        {
            MongoClient client = new MongoClient(CONNECTION_STR);
            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<Log>(COLLECTION_NAME);
            var enumCollection = collection.AsQueryable().AsEnumerable();
            var results = enumCollection.Select(
                log =>
                    {
                        var resultSpan = enumCollection.Aggregate(
                            0,
                            (span, log1) => log1.Url == log.Url ? span + 1 : span);
                        return new Container { Count = resultSpan, Log = log };
                    }).Distinct(new DistinctContainerUrlComparer())
                    .OrderByDescending(log => log.Log.Url);
            return results;
        }

        /// <summary>
        /// The get sorted url list with entrance internet address.
        /// </summary>
        /// <param name="ip">
        /// The internet address.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static IEnumerable<Log> GetSortedUrlsWithEntranceIp(string ip)
        {
            MongoClient client = new MongoClient(CONNECTION_STR);
            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<Log>(COLLECTION_NAME);
            return Queryable.Where(collection.AsQueryable().OrderBy(log => log.Url), log => log.Ip == ip);
        }

        /// <summary>
        /// The get sorted url list with period.
        /// </summary>
        /// <param name="min">
        /// The min.
        /// </param>
        /// <param name="max">
        /// The max.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static IEnumerable<Log> GetSortedUrlsWithPeriod(DateTime min, DateTime max)
        {
            MongoClient client = new MongoClient(CONNECTION_STR);
            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<Log>(COLLECTION_NAME);
            return Queryable.Where(collection.AsQueryable().OrderBy(log => log.Url), log => log.TimeStamp >= min && log.TimeStamp <= max);
        }

        /// <summary>
        /// The get sorted internet address with url list.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static IEnumerable<Log> GetSortedIpWithUrl(string url)
        {
            MongoClient client = new MongoClient(CONNECTION_STR);
            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<Log>(COLLECTION_NAME);
            return Queryable.Where(collection.AsQueryable().OrderBy(log => log.Ip), log => log.Url == url);
        }

        /// <summary>
        /// The get sorted url list.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static IEnumerable<Log> GetSortedUrls()
        {
            MongoClient client = new MongoClient(CONNECTION_STR);
            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<Log>(COLLECTION_NAME);
            return Queryable.OrderBy(collection.AsQueryable(), log => log.Url);
        }

        /// <summary>
        /// The translate from csv.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        public static void TranslateFromCSV(string fileName)
        {
            using (CsvReader csvReader = new CsvReader(File.OpenText(fileName)))
            {
                var collectionLogs = csvReader.GetRecords<Log>().ToList();
                MongoClient client = new MongoClient(CONNECTION_STR);
                var db = client.GetDatabase(DB_NAME);
                var collection = db.GetCollection<Log>(COLLECTION_NAME);
                foreach (var collectionLog in collectionLogs)
                {
                    if (collection.FindSync(log => log.Id == collectionLog.Id).Any())
                    {
                        var filter = Builders<Log>.Filter.Eq(log => log.Id, collectionLog.Id);
                        collection.ReplaceOne(filter, collectionLog);
                    }
                    else
                    {
                        collectionLog.Id = null;
                        collection.InsertOne(collectionLog);
                    }
                }
            }
        }

        /// <summary>
        /// The translate from server to csv.
        /// </summary>
        /// <param name="outFileName">
        /// The out file name.
        /// </param>
        public static void TranslateFromServerToCSV(string outFileName)
        {
            MongoClient client = new MongoClient(CONNECTION_STR);
            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<Log>(COLLECTION_NAME);
            var logs = collection.Find(log => true).ToList();

            using (CsvWriter csvWriter = new CsvWriter(File.CreateText(outFileName)))
            {                            
                csvWriter.WriteRecords(logs);
            }
        }

        /// <summary>
        /// The write to file.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <typeparam name="T">
        /// Collection parameter
        /// </typeparam>
        /// <exception cref="ArgumentException">
        /// Thrown when collection is empty 
        /// </exception>
        public static void WriteToFile<T>(IEnumerable<T> collection, string fileName)
        {
            if (!collection.Any())
            {
                throw new ArgumentException(nameof(collection));
            }

            using (CsvWriter csvWriter = new CsvWriter(File.CreateText(fileName)))
            {
                csvWriter.WriteRecords(collection);
            }
        }

        /// <summary>
        /// The drop logs.
        /// </summary>
        public static void DropLogs()
        {
            MongoClient client = new MongoClient(CONNECTION_STR);
            var db = client.GetDatabase(DB_NAME);
            db.DropCollection(COLLECTION_NAME);
        }

        /// <summary>
        /// The generate data.
        /// </summary>
        /// <param name="count">
        /// The count.
        /// </param>
        public static void GenerateData(int count)
        {
            MongoClient client = new MongoClient(CONNECTION_STR);
            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<Log>(COLLECTION_NAME);
            Generator.Default.Configure(builder => builder.Entity<Log>(
                mappingBuilder =>
                    {
                        mappingBuilder.Property(log => log.Ip).DataSource<IpSource>();
                        mappingBuilder.Property(log => log.TimeStamp).DateTimeSource(
                            new DateTime(2016, 1, 1),
                            DateTime.Now);
                        mappingBuilder.Property(log => log.TimeSpent).DataSource<TimeSpanSource>();
                        mappingBuilder.Property(log => log.Url).DataSource<WebsiteSource>();
                    }));

            var logs = Generator.Default.List<Log>(count);

            collection.InsertMany(logs);
        }        
    }
}

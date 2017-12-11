using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    using System.Collections;
    using System.IO;
    using CsvHelper;
    using DataGenerator;
    using DataGenerator.Sources;

    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    public class Logger
    {
        private string DB_NAME = "Server";

        private string COLLECTION_NAME = "Log";

        private string CONNECTION_STR = "mongodb://Lolik21:123qWe123@servercluster-shard-00-00-ohx52."
                                + "mongodb.net:27017,servercluster-shard-00-01-ohx52.mongodb.net:"
                                + "27017,servercluster-shard-00-02-ohx52.mongodb.net:27017/test?"
                                + "ssl=true&replicaSet=ServerCluster-shard-0&authSource=admin";

        public IEnumerable<IpContainer> MapReduceByIp(DateTime min, DateTime max)
        {
            MongoClient client = new MongoClient(this.CONNECTION_STR);
            var db = client.GetDatabase(this.DB_NAME);
            var collection = db.GetCollection<Log>(this.COLLECTION_NAME);
            var enumCollection = collection.AsQueryable().AsEnumerable();
            var results = enumCollection.Select(
                    log =>
                        {
                            var resultSpan = enumCollection.Aggregate(
                                0,
                                (span, log1) => log1.Ip == log.Ip
                                                    ? span += 1
                                                    : span);

                            var resultSpan2 = enumCollection.Aggregate(
                                TimeSpan.Zero,
                                (span, log1) => log1.Ip == log.Ip
                                                    ? span += log1.TimeSpent
                                                    : span);


                            return new IpContainer { Count = resultSpan, Time = resultSpan2, Ip = log.Ip};
                        }).Distinct(new DistinctIpConteinerUrlComparer())
                        .OrderByDescending(container => container.Ip)
                        .ThenByDescending(container => container.Count)
                        .ThenByDescending(container => container.Time);
            return results;

        }

        public IEnumerable<Container> MapReduceByTime(DateTime min, DateTime max)
        {
            MongoClient client = new MongoClient(this.CONNECTION_STR);
            var db = client.GetDatabase(this.DB_NAME);
            var collection = db.GetCollection<Log>(this.COLLECTION_NAME);
            var enumCollection = collection.AsQueryable().AsEnumerable();
            var results = enumCollection.Select(
                log =>
                    {
                        var resultSpan = enumCollection.Aggregate(
                            0,
                            (span, log1) =>
                                {
                                    return log1.Url == log.Url && log1.TimeStamp < min && log1.TimeStamp >= max
                                               ? span += 1
                                               : span;
                                });
                        return new Container { Count = resultSpan, Log = log };
                    }).Distinct(new DistinctConteinerUrlComparer()).OrderByDescending(log => log.Log.Url)
                    .ThenByDescending(container => container.Count);
            return results;

        }

        public IEnumerable<Log> MapReduceUrlsSumTime()
        {
            MongoClient client = new MongoClient(this.CONNECTION_STR);
            var db = client.GetDatabase(this.DB_NAME);
            var collection = db.GetCollection<Log>(this.COLLECTION_NAME);
            var enumCollection = collection.AsQueryable().AsEnumerable();
            var results = enumCollection.Select(
                log =>
                    {
                        var resultSpan = enumCollection.Aggregate(
                            TimeSpan.Zero,
                            (span, log1) => { return log1.Url == log.Url ? span += log.TimeSpent : span; });
                        return log;
                    }).Distinct(new DistinctLogUrlComparer()).OrderByDescending(log => log.Url);
            return results;
        }

        public IEnumerable<Container> MapReduceUrlsSumCount()
        {
            MongoClient client = new MongoClient(this.CONNECTION_STR);
            var db = client.GetDatabase(this.DB_NAME);
            var collection = db.GetCollection<Log>(this.COLLECTION_NAME);
            var enumCollection = collection.AsQueryable().AsEnumerable();
            var results = enumCollection.Select(
                log =>
                    {
                        var resultSpan = enumCollection.Aggregate(
                            0,
                            (span, log1) => { return log1.Url == log.Url ? span += 1 : span; });
                        return new Container { Count = resultSpan, Log = log };
                    }).Distinct(new DistinctConteinerUrlComparer()).OrderByDescending(log => log.Log.Url);
            return results;
        }

        public IEnumerable<Log> GetSortedUrlsWithEntranceIp(string ip)
        {
            MongoClient client = new MongoClient(this.CONNECTION_STR);
            var db = client.GetDatabase(this.DB_NAME);
            var collection = db.GetCollection<Log>(this.COLLECTION_NAME);
            return Queryable.Where(collection.AsQueryable().OrderBy(log => log.Url), log => log.Ip == ip);
        }

        public IEnumerable<Log> GetSortedUrlsWithPeriod(DateTime min, DateTime max)
        {
            MongoClient client = new MongoClient(this.CONNECTION_STR);
            var db = client.GetDatabase(this.DB_NAME);
            var collection = db.GetCollection<Log>(this.COLLECTION_NAME);
            return Queryable.Where(collection.AsQueryable().OrderBy(log => log.Url), log => log.TimeStamp >= min && log.TimeStamp <= max);
        }

        public IEnumerable<Log> GetSortedIpWithUrl(string url)
        {
            MongoClient client = new MongoClient(this.CONNECTION_STR);
            var db = client.GetDatabase(this.DB_NAME);
            var collection = db.GetCollection<Log>(this.COLLECTION_NAME);
            return Queryable.Where(collection.AsQueryable().OrderBy(log => log.Ip), log => log.Url == url);
        }

        public IEnumerable<Log> GetSortedUrls()
        {
            MongoClient client = new MongoClient(this.CONNECTION_STR);
            var db = client.GetDatabase(this.DB_NAME);
            var collection = db.GetCollection<Log>(this.COLLECTION_NAME);
            return Queryable.OrderBy(collection.AsQueryable(), log => log.Url);
        }

        public void TranslateFromCVS(string fileName)
        {
            using (CsvReader csvReader = new CsvReader(File.OpenText(fileName)))
            {
                var collectionLogs = csvReader.GetRecords<Log>().ToList();
                MongoClient client = new MongoClient(this.CONNECTION_STR);
                var db = client.GetDatabase(this.DB_NAME);
                var collection = db.GetCollection<Log>(this.COLLECTION_NAME);
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

        public void TranslateFromServerToCVS(string outFileName)
        {
            MongoClient client = new MongoClient(this.CONNECTION_STR);
            var db = client.GetDatabase(this.DB_NAME);
            var collection = db.GetCollection<Log>(this.COLLECTION_NAME);
            var logs = collection.Find(log => true).ToList();

            using (CsvWriter csvWriter = new CsvWriter(File.CreateText(outFileName)))
            {                            
                csvWriter.WriteRecords(logs);
            }
        }

        private void WriteToFile(IEnumerable<Log> collection, string fileName)
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

        private void DropLogs()
        {
            MongoClient client = new MongoClient(this.CONNECTION_STR);
            var db = client.GetDatabase(this.DB_NAME);
            db.DropCollection(this.COLLECTION_NAME);
        }

        private void GenerateData(int count)
        {
            MongoClient client = new MongoClient(this.CONNECTION_STR);
            var db = client.GetDatabase(this.DB_NAME);
            var collection = db.GetCollection<Log>(this.COLLECTION_NAME);
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

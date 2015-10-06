﻿using System;
using System.Collections.Generic;
using Amazon;
using Amazon.DynamoDBv2;
using ServiceStack.Aws.DynamoDb;
using ServiceStack.Aws.DynamoDbTests.Shared;
using ServiceStack.Caching;
using ServiceStack.Configuration;

namespace ServiceStack.Aws.DynamoDbTests
{
    public abstract class DynamoTestBase
    {
        //Run ./build/start-local-dynamodb.bat to start local DynamoDB instance on 8000
        public static bool UseLocalDb = true;

        public static IPocoDynamo CreatePocoDynamo()
        {
            var dynamoClient = CreateDynamoDbClient();

            var db = new PocoDynamo(dynamoClient);
            return db;
        }

        public static ICacheClient CreateCacheClient()
        {
            var cache = new DynamoDbCacheClient(CreatePocoDynamo());
            cache.InitSchema();
            return cache;
        }

        public static AmazonDynamoDBClient CreateDynamoDbClient()
        {
            var accessKey = Environment.GetEnvironmentVariable("AWSAccessKey");
            var secretKey = Environment.GetEnvironmentVariable("AWSSecretKey");

            var useLocalDb = UseLocalDb || 
                string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey);

            var dynamoClient = useLocalDb
                ? new AmazonDynamoDBClient("keyId", "key", new AmazonDynamoDBConfig {
                    ServiceURL = ConfigUtils.GetAppSetting("DynamoDbUrl", "http://localhost:8000"),
                })
                : new AmazonDynamoDBClient(accessKey, secretKey, RegionEndpoint.USEast1);
            return dynamoClient;
        }

        public static List<Poco> PutPocoItems(IPocoDynamo db, int count = 10)
        {
            db.RegisterTable<Poco>();
            db.InitSchema();

            var items = count.Times(i => new Poco { Id = i, Title = "Name " + i });

            db.PutItems(items);
            return items;
        }
    }
}
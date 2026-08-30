using System;
using System.IO;
using System.Threading.Tasks;
using Minio;
using Minio.DataModel.Args;

namespace backend.Services
{
    public class StorageService
    {
        private readonly IMinioClient _client;
        private readonly string _bucketName;

        public StorageService()
        {
            var endpoint = Environment.GetEnvironmentVariable("MINIO_ENDPOINT") ?? "minio:9000";
            var accessKey = Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY") ?? "admin";
            var secretKey = Environment.GetEnvironmentVariable("MINIO_SECRET_KEY") ?? "adminpassword";
            _bucketName = Environment.GetEnvironmentVariable("MINIO_BUCKET") ?? "recordings";

            _client = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(false)
                .Build();
        }

        public virtual async Task<string> GeneratePresignedUrlAsync(string objectKey, int expirySeconds = 900)
        {
            var args = new PresignedGetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectKey)
                .WithExpiry(expirySeconds);
            return await _client.PresignedGetObjectAsync(args);
        }

        public async Task UploadFileAsync(Stream data, string objectKey, string contentType, long size)
        {
            var args = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectKey)
                .WithStreamData(data)
                .WithObjectSize(size)
                .WithContentType(contentType);
            await _client.PutObjectAsync(args);
        }

        public async Task DeleteFileAsync(string objectKey)
        {
            var args = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectKey);
            await _client.RemoveObjectAsync(args);
        }
    }
}
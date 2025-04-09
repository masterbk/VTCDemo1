using Demo1.Dto.Options;
using Google;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Demo1.Service
{
    public class GoogleStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly GCPOption _gCPOption;
        private readonly ILogger<GoogleStorageService> _logger;

        public GoogleStorageService(StorageClient storageClient,
            GCPOption gCPOption, ILogger<GoogleStorageService> logger)
        {
            _storageClient = storageClient;
            _gCPOption = gCPOption;
            _logger = logger;
        }

        public async Task<bool> CreateBucketAsync(string bucketName)
        {
            try
            {
                await _storageClient.CreateBucketAsync(_gCPOption.ProjectID, bucketName);
                return true;
            }
            catch (GoogleApiException ex) when (ex.Error.Code == 409)
            {
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{nameof(GoogleStorageService)}.{nameof(CreateBucketAsync)}] => Error creating bucket: {ex.Message}");
                return false;
            }
        }

        public async Task MakeBucketPublicAsync(string bucketName)
        {
            // Lấy policy hiện tại của bucket
            var policy = await _storageClient.GetBucketIamPolicyAsync(bucketName);

            // Thêm quyền roles/storage.objectViewer cho allUsers
            policy.Bindings.Add(new Policy.BindingsData
            {
                Role = "roles/storage.objectViewer",
                Members = new[] { "allUsers" }
            });

            // Cập nhật lại policy
            _storageClient.SetBucketIamPolicy(bucketName, policy);
        }

        public async Task<Google.Apis.Storage.v1.Data.Object?> UploadImageAsync(string bucketUri, string? fileName, 
            Stream stream, string? keyword)
        {
            try
            {
                var strArr = bucketUri.Split("/");
                var bucketName = strArr[0];
                var folderName = strArr.Length > 1 ? $"{strArr[1]}/" : "";

                var obj = await _storageClient.UploadObjectAsync(new Google.Apis.Storage.v1.Data.Object
                {
                    Bucket = bucketName,
                    Name = $"{folderName}{fileName}",
                    Metadata = new Dictionary<string, string?>
                {
                    { "keyword", keyword }
                }
                }, stream);

                return obj;
            }
            catch(Exception ex)
            {
                _logger.LogError($"[{nameof(GoogleStorageService)}.{nameof(UploadImageAsync)}] => Upload error: {ex.Message}");
                return null;
            }
        }
    }
}

using Google.Cloud.Storage.V1;

namespace Demo1.Service
{
    public class GoogleStorageService
    {
        private readonly StorageClient _storageClient;

        public GoogleStorageService(StorageClient storageClient)
        {
            _storageClient = storageClient;
        }

        public async Task<Google.Apis.Storage.v1.Data.Object> UploadImageAsync(string bucketUri, string? fileName, 
            Stream stream, string? keyword)
        {
            var obj = await _storageClient.UploadObjectAsync(new Google.Apis.Storage.v1.Data.Object
            {
                Bucket = bucketUri,
                Name = fileName??"No name",
                Metadata = new Dictionary<string, string>
                {
                    { "keyword", keyword }
                }
            }, stream);

            return obj;
        }
    }
}

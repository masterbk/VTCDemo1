using Demo1.Dto.Options;
using Demo1.Helper;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;

namespace Demo1.Service
{
    public class GoogleStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly GoogleDriveService _googleDriveService;
        public readonly DriveService _driveService;
        private readonly FirestoreDb _firestore;

        private readonly GCPOption _gCPOption;
        public GoogleStorageService(GoogleCredential googleCredential, 
            GCPOption gCPOption,
            GoogleDriveService googleDriveService,
            DriveService driveService,
            FirestoreDb firestoreDb)
        {
            _gCPOption = gCPOption;
            _storageClient = StorageClient.Create(googleCredential);
            _googleDriveService = googleDriveService;
            _driveService = driveService;
            _firestore = firestoreDb;
        }

        public async Task<WriteResult> UploadImageAsync(string bucketUri, string driveUrl, string keyword)
        {
            var driveFileId = driveUrl.ExtractDriveFileId();
            var file = await _driveService.Files.Get(driveFileId).ExecuteAsync();

            using var memStream = new MemoryStream();
            var request = _driveService.Files.Get(driveUrl.ExtractDriveFileId());
            await request.DownloadAsync(memStream);

            var obj = await _storageClient.UploadObjectAsync(new Google.Apis.Storage.v1.Data.Object
            {
                Bucket = bucketUri,
                Name = file.Name,
                Metadata = new Dictionary<string, string>
                {
                    { "keyword", keyword }
                }
            }, memStream);

            var docRef = _firestore.Collection(_gCPOption.FilebaseCollectionName).Document();
            var wrireResult = await docRef.SetAsync(new
            {
                fileName = file.Name,
                url = obj.MediaLink,
                keywords = keyword.Split(",").Select(s=>s?.Trim()?.ToLower()),
                uploadedAt = Timestamp.GetCurrentTimestamp()
            });

            return wrireResult;
        }
    }
}

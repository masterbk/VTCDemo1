using Demo1.Dto.MediaDto.Request;
using Demo1.Dto.MediaDto.Response;
using Demo1.Dto.Options;
using Demo1.Helper;
using Demo1.Service;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace Demo1.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MediaController : Controller
    {
        public readonly GoogleSheetService _googleSheetService;
        public readonly GoogleStorageService _googleStorageService;
        public readonly GoogleDriveService _googleDriveService;
        public readonly FirebaseService _firebaseService;
        public readonly GCPOption _gCPOption;

        public MediaController(GoogleSheetService googleSheetService,
            GoogleStorageService googleStorageService,
            GoogleDriveService googleDriveService,
            FirebaseService firebaseService,
            GCPOption gCPOption)
        {
            _googleSheetService = googleSheetService;
            _googleStorageService = googleStorageService;
            _googleDriveService = googleDriveService;
            _firebaseService = firebaseService;
            _gCPOption = gCPOption;
        }

        /// <summary>
        /// - Đọc thông tin ảnh từ Google Sheet
        /// - Download ảnh từ Google Drive
        /// - Upload ảnh lên Google Storage
        /// - Lưu thông tin meta của ảnh vào Firebase
        /// </summary>
        /// <param name="importMediaRequest"></param>
        /// <returns></returns>
        [HttpPost("ingest")]
        public async Task<ImportMediaResponse> IngestAsync([FromBody] ImportMediaRequest importMediaRequest)
        {
            var numProcessed = 0;

            // Đọc thông tin từ Google Sheet
            var listUrlAndKeyword = await _googleSheetService
                .GetValueByColumnNameAsync(importMediaRequest.SpreadsheetId, importMediaRequest.SheetName, "M", "N");

            var createBucketResult = await _googleStorageService.CreateBucketAsync(importMediaRequest.BucketUri.Split("/")[0]);
            if (createBucketResult)
            {
                //await _googleStorageService.MakeBucketPublicAsync(importMediaRequest.BucketUri.Split("/")[0]);
            }
            else
            {
                throw new Exception("Create bucket failed");
            }

            foreach (var item in listUrlAndKeyword)
            {
                numProcessed++;

                var driveFileId = item["M"]?.ExtractDriveFileId();
                if (string.IsNullOrWhiteSpace(driveFileId))
                {
                    continue;
                }

                var keyword = item["N"];
                var fileInfo = await _googleDriveService.GetFileInfoAsync(driveFileId);
                var fileName = fileInfo.Name;
                var mimeType = fileInfo.MimeType;

                if(!fileInfo.MimeType.IsImage() && !fileInfo.MimeType.IsVideo())
                {
                    continue;
                }

                // Download ảnh từ Google Drive
                var stream = await _googleDriveService.DownloadImageAsync(driveFileId);

                if (stream.Length == 0)
                {
                    continue;
                }

                if (fileInfo.MimeType.IsImage() && !fileInfo.MimeType.IsImageWebp())
                {
                    //convert to webp
                    var streamTmp = await MediaExtensions.ConvertImageToWebpAsync(stream);
                    await stream.DisposeAsync();

                    stream = streamTmp;
                    fileName = $"{fileName.GetFileNameWithoutExtension()}.webp";
                }
                else
                {
                    if (fileInfo.MimeType.IsVideo() && !fileInfo.MimeType.IsVideoMp4())
                    {
                        //convert to mp4
                        var streamTmp = await MediaExtensions.ConvertVideoToMp4Async(stream);
                        await stream.DisposeAsync();
                        fileName = $"{fileName.GetFileNameWithoutExtension()}.mp4";

                        stream = streamTmp;
                    }
                }

                // Upload ảnh lên Google Storage
                var obj = await _googleStorageService.UploadImageAsync(importMediaRequest.BucketUri, fileName,
                    stream, keyword);
                if (obj == null)
                {
                    continue;
                }

                // Lưu thông tin vào Firebase
                var wrireResult = await _firebaseService.CreateAsync<dynamic>(_gCPOption.FilebaseCollectionName, new
                {
                    fileName,
                    url = $"https://storage.googleapis.com/{importMediaRequest.BucketUri}/{fileName}",
                    keywords = keyword?.Split(",").Select(s => s?.Trim()?.ToLower()),
                    uploadedAt = Timestamp.GetCurrentTimestamp()
                });
            }

            return new ImportMediaResponse
            {
                NumProcessed = numProcessed
            };
        }

        /// <summary>
        /// Tìm kiếm ảnh theo từ khóa
        /// </summary>
        /// <param name="searchMediaRequest"></param>
        /// <returns></returns>
        [HttpPost("lookup")]
        public async Task<SearchMediaResponse> LookupAsync([FromBody] SearchMediaRequest searchMediaRequest)
        {
            var result = new SearchMediaResponse();
            if (string.IsNullOrEmpty(searchMediaRequest.Keyword)) 
                return result;

            var searchResult = await _firebaseService.SearchAsync(_gCPOption.FilebaseCollectionName, "keywords", searchMediaRequest.Keyword.Trim().ToLower());
            
            result.Media = searchResult?.GetValue<string>("url");

            return result;
        }
    }
}

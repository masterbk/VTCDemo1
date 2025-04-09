using Demo1.Dto.MediaDto.Request;
using Demo1.Dto.MediaDto.Response;
using Demo1.Dto.Options;
using Demo1.Helper;
using Demo1.Service;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<MediaController> _logger;

        public MediaController(GoogleSheetService googleSheetService,
            GoogleStorageService googleStorageService,
            GoogleDriveService googleDriveService,
            FirebaseService firebaseService,
            GCPOption gCPOption,
            ILogger<MediaController> logger)
        {
            _googleSheetService = googleSheetService;
            _googleStorageService = googleStorageService;
            _googleDriveService = googleDriveService;
            _firebaseService = firebaseService;
            _gCPOption = gCPOption;
            _logger = logger;
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
            var result = new ImportMediaResponse() { NumProcessed = 0 };

            // Đọc thông tin từ Google Sheet
            var listUrlAndKeyword = await _googleSheetService
                .GetValueByColumnNameAsync(importMediaRequest.SpreadsheetId, importMediaRequest.SheetName, "M", "N");
            if (listUrlAndKeyword.Count() == 0)
                return result;

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
                _logger.LogInformation($"[ItemProcess] {new
                {
                    item
                }.ToJson()} ");

                result.NumProcessed++;

                var driveFileId = item["M"]?.ExtractDriveFileId();
                if (string.IsNullOrWhiteSpace(driveFileId))
                {
                    _logger.LogError($"[ItemProcess] {new
                    {
                        message = "DriveId invalid",
                        item
                    }.ToJson()} ");
                    continue;
                }

                var keyword = item["N"];
                var fileInfo = await _googleDriveService.GetFileInfoAsync(driveFileId);
                if(fileInfo == null)
                {
                    _logger.LogError($"[ItemProcess] {new
                    {
                        message = "Get file info error",
                        item
                    }.ToJson()} ");
                    continue;
                }

                var fileName = fileInfo.Name;
                var mimeType = fileInfo.MimeType;

                if(!fileInfo.MimeType.IsImage() && !fileInfo.MimeType.IsVideo())
                {
                    _logger.LogError($"[ItemProcess] {new
                    {
                        message = "Media invalid",
                        item
                    }.ToJson()} ");
                    continue;
                }

                // Download ảnh từ Google Drive
                var stream = await _googleDriveService.DownloadImageAsync(driveFileId);

                if (stream.Length == 0)
                {
                    _logger.LogError($"[ItemProcess] {new
                    {
                        message = "Download media error",
                        item
                    }.ToJson()} ");
                    continue;
                }

                if (fileInfo.MimeType.IsImage() && !fileInfo.MimeType.IsImageWebp())
                {
                    //convert to webp
                    try
                    {
                        var streamTmp = await MediaExtensions.ConvertImageToWebpAsync(stream);
                        await stream.DisposeAsync();
                        stream = streamTmp;
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError($"[ItemProcess] {new
                        {
                            message = $"Convert image error: {ex.Message}",
                            item
                        }.ToJson()} ");

                        continue;
                    }

                    fileName = $"{fileName.GetFileNameWithoutExtension()}.webp";
                }
                else
                {
                    if (fileInfo.MimeType.IsVideo() && !fileInfo.MimeType.IsVideoMp4())
                    {
                        //convert to mp4
                        try
                        {
                            var streamTmp = await MediaExtensions.ConvertVideoToMp4Async(stream);
                            await stream.DisposeAsync();
                            stream = streamTmp;
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError($"[ItemProcess] {new
                            {
                                message = $"Convert video error: {ex.Message}",
                                item
                            }.ToJson()} ");

                            continue;
                        }

                        fileName = $"{fileName.GetFileNameWithoutExtension()}.mp4";
                    }
                }

                // Upload ảnh lên Google Storage
                var obj = await _googleStorageService.UploadImageAsync(importMediaRequest.BucketUri, fileName,
                    stream, keyword);
                if (obj == null)
                {
                    _logger.LogError($"[ItemProcess] {new
                    {
                        message = "Upload media error",
                        item
                    }.ToJson()} ");

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

                if(wrireResult == null)
                {
                    _logger.LogError($"[ItemProcess] {new
                    {
                        message = "Create document error",
                        item
                    }.ToJson()} ");
                }
            }

            return result;
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

using Demo1.Dto.MediaDto.Request;
using Demo1.Dto.MediaDto.Response;
using Demo1.Service;
using Microsoft.AspNetCore.Mvc;

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
        public MediaController(GoogleSheetService googleSheetService,
            GoogleStorageService googleStorageService,
            GoogleDriveService googleDriveService,
            FirebaseService firebaseService)
        {
            _googleSheetService = googleSheetService;
            _googleStorageService = googleStorageService;
            _googleDriveService = googleDriveService;
            _firebaseService = firebaseService;
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
            var listUrlAndKeyword = await _googleSheetService
                .GetValueByColumnNameAsync(importMediaRequest.SpreadsheetId, importMediaRequest.SheetName, "M", "N");

            foreach (var item in listUrlAndKeyword)
            {
                var rsUpload = await _googleStorageService.UploadImageAsync(importMediaRequest.BucketUri, item["M"], item["N"]);
                numProcessed++;
            }

            //var tasks = listUrlAndKeyword.Select(item => _googleStorageService.UploadImageAsync(item["M"], item["N"]));
            //await Task.WhenAll(tasks);

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

            var searchResult = await _firebaseService.SearchAsync(searchMediaRequest.Keyword.Trim().ToLower());
            result.Media = searchResult?.GetValue<string>("url");

            return result;
        }
    }
}

using Demo1.Helper;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo1.Service
{
    public class GoogleDriveService
    {
        private readonly DriveService _driveService;
        private readonly ILogger<GoogleDriveService> _logger;
        public GoogleDriveService(DriveService driveService,
            ILogger<GoogleDriveService> logger)
        {
            _driveService = driveService;
            _logger = logger;
        }

        public async Task<Google.Apis.Drive.v3.Data.File?> GetFileInfoAsync(string driveFileId)
        {
            try
            {
                return await _driveService.Files.Get(driveFileId).ExecuteAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError($"[{nameof(GoogleDriveService)}.{nameof(GetFileInfoAsync)}] => {ex.Message}");
                return null;
            }
        }

        public async Task<MemoryStream> DownloadImageAsync(string driveFileId)
        {
            try
            {
                var memStream = new MemoryStream();
                var request = _driveService.Files.Get(driveFileId);
                await request.DownloadAsync(memStream);

                memStream.Position = 0; // Reset the stream position to the beginning
                return memStream;
            }
            catch(Exception ex)
            {
                _logger.LogError($"[{nameof(GoogleDriveService)}.{nameof(DownloadImageAsync)}] => {ex.Message}");
                return new MemoryStream();
            }
        }
    }
}

using Demo1.Helper;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
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
        public GoogleDriveService(DriveService driveService)
        {
            _driveService = driveService;
        }

        public async Task<string> GetFileNameAsync(string driveUrl)
        {
            var driveFileId = driveUrl.ExtractDriveFileId();
            if (string.IsNullOrEmpty(driveFileId)) return "";

            var file = await _driveService.Files.Get(driveFileId).ExecuteAsync();

            return file != null ? file.Name : "";
        }

        public async Task<Stream> DownloadImageAsync(string driveUrl)
        {
            var driveFileId = driveUrl.ExtractDriveFileId();
            if (string.IsNullOrEmpty(driveFileId)) return null;

            using var memStream = new MemoryStream();
            var request = _driveService.Files.Get(driveFileId);
            await request.DownloadAsync(memStream);

            return memStream;
        }
    }
}

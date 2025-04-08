using Demo1.Helper;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
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

        public async Task<Google.Apis.Drive.v3.Data.File> GetFileInfoAsync(string driveFileId)
        {
            return await _driveService.Files.Get(driveFileId).ExecuteAsync();
        }

        public async Task<MemoryStream> DownloadImageAsync(string driveFileId)
        {
            var memStream = new MemoryStream();
            var request = _driveService.Files.Get(driveFileId);
            await request.DownloadAsync(memStream);

            memStream.Position = 0; // Reset the stream position to the beginning
            return memStream;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo1.Dto.Enums
{
    public static class GoogleScopes
    {
        public const string SheetsReadOnly = "https://www.googleapis.com/auth/spreadsheets.readonly";
        public const string DriveReadOnly = "https://www.googleapis.com/auth/drive.readonly";
        public const string StorageFullControl = "https://www.googleapis.com/auth/devstorage.full_control";
        public const string CloudPlatform = "https://www.googleapis.com/auth/cloud-platform";
    }
}

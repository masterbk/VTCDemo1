using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Demo1.Dto.MediaDto.Request
{
    public class ImportMediaRequest
    {
        [JsonPropertyName("spreadsheet_id")]
        public string SpreadsheetId { get; set; }
        [JsonPropertyName("sheetname")]
        public string SheetName { get; set; }
        [JsonPropertyName("bucket_uri")]
        public string BucketUri { get; set; }
    }
}

using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo1.Service
{
    public class GoogleSheetService
    {
        public readonly SheetsService _sheetsService;
        public GoogleSheetService(SheetsService sheetsService)
        {
            _sheetsService = sheetsService;
        }

        public async Task<List<Dictionary<string, string>>> GetValueByColumnNameAsync(string sheetId, string sheetName, params string[] columnNames)
        {
            var result = new List<Dictionary<string,string>>();

            var ranges = new List<string>();
            foreach(var columnName in columnNames)
            {
                ranges.Add($"{sheetName}!{columnName}3:{columnName}");
            }

            var batchRequest = _sheetsService.Spreadsheets.Values.BatchGet(sheetId);
            batchRequest.Ranges = ranges;

            var batchResponse = await batchRequest.ExecuteAsync();

            var names = batchResponse.ValueRanges[0].Values;
            var imageUrls = batchResponse.ValueRanges[1].Values;

            var minRow = batchResponse.ValueRanges.Max(a=>a.Values.Count());

            for (int i = 0; i < minRow; i++)
            {
                result.Add(new Dictionary<string, string>(columnNames.Select((s, index) => new KeyValuePair<string, string>(s, batchResponse.ValueRanges[index]
                    .Values[i][0].ToString()))));
            }

            return result;
        }
    }
}

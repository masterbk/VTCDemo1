using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Logging;

namespace Demo1.Service
{
    public class GoogleSheetService
    {
        public readonly SheetsService _sheetsService;
        private readonly ILogger<GoogleSheetService> _logger;
        public GoogleSheetService(SheetsService sheetsService,
            ILogger<GoogleSheetService> logger)
        {
            _sheetsService = sheetsService;
            _logger = logger;
        }

        public async Task<List<Dictionary<string, string?>>> GetValueByColumnNameAsync(string sheetId, string sheetName, params string[] columnNames)
        {
            try
            {
                var result = new List<Dictionary<string, string?>>();

                var ranges = new List<string>();
                foreach (var columnName in columnNames)
                {
                    ranges.Add($"{sheetName}!{columnName}3:{columnName}");
                }

                var batchRequest = _sheetsService.Spreadsheets.Values.BatchGet(sheetId);
                batchRequest.Ranges = ranges;

                var batchResponse = await batchRequest.ExecuteAsync();

                var names = batchResponse.ValueRanges[0].Values;
                var imageUrls = batchResponse.ValueRanges[1].Values;

                var minRow = batchResponse.ValueRanges.Max(a => a.Values.Count());

                for (int i = 0; i < minRow; i++)
                {
                    try
                    {
                        result.Add(new Dictionary<string, string?>(columnNames.Select((s, index) => new KeyValuePair<string, string?>(s, batchResponse.ValueRanges[index]
                            .Values[i][0].ToString()))));
                    }
                    catch (Exception ex)
                    {
                        // Handle the exception as needed
                        _logger.LogError($"[{nameof(GoogleSheetService)}.{nameof(GetValueByColumnNameAsync)}] => Error processing row {i}: {ex.Message}");
                    }
                }

                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError($"[{nameof(GoogleSheetService)}.{nameof(GetValueByColumnNameAsync)}] => {ex.Message}");
                return new List<Dictionary<string, string?>>();
            }
        }
    }
}

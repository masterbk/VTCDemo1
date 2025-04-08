using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Demo1.Helper
{
    public static class StringHelper
    {
        public static string ExtractDriveFileId(this string url)
        {
            var match = Regex.Match(url, @"\/d\/([a-zA-Z0-9_-]+)");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}

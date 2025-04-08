using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo1.Dto.Options
{
    public class GCPOption
    {
        public string? ProjectID { get; set; }
        public string? CredentialFile { get; set; }
        public string? FilebaseCollectionName { get; set; }
    }
}

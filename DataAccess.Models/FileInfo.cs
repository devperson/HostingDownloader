using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess.Models
{
    public class FileInfo
    {
        public long Id { get; set; }
        public string FileName { get; set; }
        public int PartSize { get; set; }
        public int LastPartSize { get; set; }
        public int PartsCount { get; set; }
    }
}

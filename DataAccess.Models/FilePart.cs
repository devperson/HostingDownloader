using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess.Models
{
    public class FilePart
    {
        public long Id { get; set; }
        public string FileName { get; set; }
        public byte[] Bytes { get; set; }
        public int Part { get; set; }         
    }
}

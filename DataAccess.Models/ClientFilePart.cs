using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess.Models
{
    public class ClientFilePart
    {
        public long Id { get; set; }
        public string FilePath { get; set; }        
        public int Part { get; set; }         
    }
}

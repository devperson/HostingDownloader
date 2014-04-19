using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

namespace DataAccess.Configurations
{
    public class FilePartModelConfiguration : EntityTypeConfiguration<FilePart>
    {
        public FilePartModelConfiguration()
        {
            HasKey(x => x.Id);
            ToTable("Parts");
        }
    }
}

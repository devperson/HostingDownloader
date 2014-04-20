using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

namespace DataAccess.Configurations
{
    public class FileInfoModelConfiguration : EntityTypeConfiguration<FileInfo>
    {
        public FileInfoModelConfiguration()
        {
            HasKey(x => x.Id);
            ToTable("Files");
        }
    }
}

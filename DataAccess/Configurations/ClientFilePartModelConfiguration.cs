using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

namespace DataAccess.Configurations
{
    public class ClientFilePartModelConfiguration : EntityTypeConfiguration<ClientFilePart>
    {
        public ClientFilePartModelConfiguration()
        {
            HasKey(x => x.Id);
            ToTable("ClientFileParts");
        }
    }
}

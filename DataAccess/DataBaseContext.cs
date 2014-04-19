using DataAccess.Configurations;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace DataAccess
{
    public class DataBaseContext : DbContext
    {
        public DataBaseContext(): base("FilesDataBase")
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.AutoDetectChangesEnabled = true;
        }

        static DataBaseContext()
        {
            Database.SetInitializer<DataBaseContext>(new DataBaseContextInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new FilePartModelConfiguration());            
        }

        public DbSet<FilePart> Parts { get; set; }        
    }
}

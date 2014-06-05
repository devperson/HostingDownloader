using DataAccess.Configurations;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace DataAccess
{
    public class ClientDataBaseContext : DbContext
    {
        public ClientDataBaseContext(): base("FilesDataBase")
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.AutoDetectChangesEnabled = true;
        }

        static ClientDataBaseContext()
        {
            Database.SetInitializer<ClientDataBaseContext>(new ClientDataBaseContextInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new ClientFilePartModelConfiguration());            
        }

        public DbSet<ClientFilePart> Parts { get; set; }        
    }
}

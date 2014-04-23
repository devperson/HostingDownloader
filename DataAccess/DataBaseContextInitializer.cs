using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace DataAccess
{
    public class DataBaseContextInitializer : DropCreateDatabaseIfModelChanges<DataBaseContext>
    {
    }

    public class ClientDataBaseContextInitializer : DropCreateDatabaseIfModelChanges<ClientDataBaseContext>
    {
    }
}

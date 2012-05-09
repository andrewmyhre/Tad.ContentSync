using Orchard.Data.Migration;

namespace Tad.ContentSync
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable("ContentSyncSettings",
                                      table => table
                                                   .Column<int>("Id", column => column.PrimaryKey().Identity())
                                                   .Column<string>("RemoteUrl", c => c.WithLength(2048))
                );

            return 1;
        }
    }
}
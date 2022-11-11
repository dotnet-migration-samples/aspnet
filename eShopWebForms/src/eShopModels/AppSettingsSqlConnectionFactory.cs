using System.Configuration;
using System.Data.SqlClient;

namespace eShopWebForms
{
    public class AppSettingsSqlConnectionFactory : ISqlConnectionFactory
    {
        public SqlConnection CreateConnection()
        {
            return new SqlConnection
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["CatalogDBContext"].ConnectionString
            };
        }
    }
}
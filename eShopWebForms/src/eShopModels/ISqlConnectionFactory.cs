using System.Data.SqlClient;

namespace eShopWebForms
{
    public interface ISqlConnectionFactory
    {
        SqlConnection CreateConnection();
    }
}
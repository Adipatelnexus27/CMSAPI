using Microsoft.Data.SqlClient;

namespace CMS.Infrastructure.Data;

public interface ISqlConnectionFactory
{
    SqlConnection CreateConnection();
}

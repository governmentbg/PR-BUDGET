using Microsoft.Extensions.Configuration;

using System.Data;
using System.Data.SqlClient;

namespace CielaDocs.Shared.DataAccess
{
    public class LogContext
    {
		private readonly IConfiguration _configuration;
		private readonly string _connectionString;

		public LogContext(IConfiguration configuration)
		{
			_configuration = configuration;
			_connectionString = _configuration.GetConnectionString("LogDbConnection");
		}

		public IDbConnection CreateConnection()
			=> new SqlConnection(_connectionString);
	}
}

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WadiAlAin.Application.Common.Interfaces;

namespace WadiAlAin.Infrastructure.Persistence
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost;Database=WadiAlAin_QualityDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
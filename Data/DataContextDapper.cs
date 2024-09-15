using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DotnetAPI.Data;

public class DataContextDapper
{
    private readonly string? _connectionString;

    public DataContextDapper(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public IEnumerable<T> LoadData<T>(string sql, object? parameters = null)
    {
        IDbConnection dbConnection = new SqlConnection(_connectionString);
        return dbConnection.Query<T>(sql, parameters);
    }

    public T LoadDataSingle<T>(string sql, object? parameters = null)
    {
        IDbConnection dbConnection = new SqlConnection(_connectionString);
        return dbConnection.QuerySingle<T>(sql, parameters);
    }

    public bool ExecuteSql(string sql, object? parameters)
    {
        IDbConnection dbConnection = new SqlConnection(_connectionString);
        return dbConnection.Execute(sql, parameters) > 0;
    }

    public int ExecuteSqlWithRowCount(string sql, object? parameters)
    {
        IDbConnection dbConnection = new SqlConnection(_connectionString);
        return dbConnection.Execute(sql, parameters);
    }
}

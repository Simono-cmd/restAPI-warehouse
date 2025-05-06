using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Tutorial9.Models;

namespace Tutorial9.Services;

public class DbService : IDbService
{
    private readonly IConfiguration _configuration;
    public DbService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // await transaction.CommitAsync(); -do commitowania zmian do bazy
    // execute scalar - zwraca liczbę, stałą (object)
    // execute reader - do selecta
    // execute nonquery - do insert update delete
    
    
    public async Task<Product?> GetProductById(int productId)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        await connection.OpenAsync();
        
        DbTransaction transaction = connection.BeginTransaction(); //transakcja zawsze przydatna do modyfikacji 2 tabel na raz
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.CommandText =
                @"
                    SELECT * FROM Product WHERE ProductId = @ProductId
                ";
            command.Parameters.AddWithValue("@ProductId", productId);
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Product
                {
                    IdProduct = reader.GetInt32(reader.GetOrdinal("IdProduct")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                };
            }
        }
        catch (Exception )
        {
            await transaction.RollbackAsync();
            throw;
        }
        return null;
    }

    public async Task<Warehouse?> GetWarehouseById(int warehouseId)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        await connection.OpenAsync();
        
        DbTransaction transaction = connection.BeginTransaction();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.CommandText =
                @"
                    SELECT * FROM Warehouse WHERE WarehouseId = @WarehouseId
                ";
            command.Parameters.AddWithValue("@WarehouseID", warehouseId);
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Warehouse
                {
                   IdWarehouse = reader.GetInt32(reader.GetOrdinal("IdWarehouse")),
                   Name = reader.GetString(reader.GetOrdinal("Name")),
                   Address = reader.GetString(reader.GetOrdinal("Address")),
                };
            }
        }
        catch (Exception )
        {
            await transaction.RollbackAsync();
            throw;
        }
        return null;
    }
    public async Task ProcedureAsync()
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        command.CommandText = "NazwaProcedury";
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@Id", 2);
        
        await command.ExecuteNonQueryAsync();
        
    }
}
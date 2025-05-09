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
        
        DbTransaction transaction = connection.BeginTransaction();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.CommandText =
                @"
                    SELECT * FROM Product WHERE IdProduct = @ProductId
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
                    SELECT * FROM Warehouse WHERE IdWarehouse = @WarehouseId
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
    
    public async Task<int> GetIDOrderByProductId(int idProduct, int requestedAmount, DateTime requestCreatedAt)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();

        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;

        command.CommandText = @"
        SELECT IdOrder, IdProduct, Amount, CreatedAt, FulfilledAt 
        FROM [Order] 
        WHERE IdProduct = @IdProduct";
        command.Parameters.AddWithValue("@IdProduct", idProduct);

        using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            int amountInDb = reader.GetInt32(reader.GetOrdinal("Amount"));
            DateTime createdAtInDb = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));

            if (amountInDb >= requestedAmount && createdAtInDb < requestCreatedAt)
            {
                return reader.GetInt32(reader.GetOrdinal("IdOrder"));
            }
        }

        // Jeśli nie znaleziono żadnego pasującego zamówienia
        throw new Exception("No matching order found with sufficient amount and earlier creation date.");
    }

    public async Task<Boolean> IsOrderFulfilled(int idOrder)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        await connection.OpenAsync();
        
            command.CommandText =
                @"
                    SELECT 1 FROM Product_Warehouse WHERE IdOrder = @idOrder
                ";
            command.Parameters.AddWithValue("@idOrder", idOrder);
            object? result = await command.ExecuteScalarAsync();
            
            return result != null;
    }
    
    public async Task UpdateOrderFulfilled(int idOrder)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();

        await using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            await using SqlCommand command = new SqlCommand();
            command.Connection = connection;
            command.Transaction = transaction;

            command.CommandText = @"
            UPDATE [Order] 
            SET FulfilledAt = @FulfilledAt
            WHERE IdOrder = @IdOrder";
        
            // Parametr aktualnej daty i godziny
            command.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);
            command.Parameters.AddWithValue("@IdOrder", idOrder);

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                throw new Exception($"Order with IdOrder {idOrder} not found.");
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception("An error occurred while updating the order: " + ex.Message, ex);
        }
    }

    public async Task<int> AddProductToWarehouse(Product_Warehouse_POST request, int idOrder, decimal productPrice)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();
        await using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            await using SqlCommand command = new SqlCommand();
            command.Connection = connection;
            command.Transaction = transaction;

            command.CommandText = @"
            INSERT INTO Product_Warehouse 
                (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
            OUTPUT INSERTED.IdProductWarehouse
            VALUES 
                (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);";

            command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@Amount", request.Amount);
            command.Parameters.AddWithValue("@Price", productPrice * request.Amount);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now); // aktualna data i czas

            int insertedId = (int)await command.ExecuteScalarAsync();
            await transaction.CommitAsync();
            return insertedId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception("Failed to insert into Product_Warehouse: " + ex.Message, ex);
        }
    }

    
    
    public async Task<int> ProcedureAsync(int idProduct, int idWarehouse, int amount, DateTime createdAt)
    {
        try
        {
            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            using (var command = new SqlCommand("AddProductToWarehouse", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@IdProduct", idProduct);
                command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@CreatedAt", createdAt);

                var result = await command.ExecuteScalarAsync();

                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    throw new Exception("Error executing stored procedure.");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to execute stored procedure: " + ex.Message, ex);
        }
    }

}
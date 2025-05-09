using Tutorial9.Models;
namespace Tutorial9.Services;

public interface IDbService
{
    Task<Product?> GetProductById(int productId);
    Task<Warehouse?> GetWarehouseById(int warehouseId);
    Task<int> GetIDOrderByProductId(int IdProduct, int requestedAmount, DateTime date);

    Task<Boolean> IsOrderFulfilled(int IdOrder);

    Task UpdateOrderFulfilled(int IdOrder);

    Task<int> AddProductToWarehouse(Product_Warehouse_POST request, int idOrder, decimal productPrice);

    Task<int> ProcedureAsync(int idProduct, int idWarehouse, int amount, DateTime createdAt);
}
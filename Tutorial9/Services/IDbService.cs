using Tutorial9.Models;
namespace Tutorial9.Services;

public interface IDbService
{
   Task<Product?> GetProductById(int productId);
   Task<Warehouse?> GetWarehouseById(int warehouseId);
   
}
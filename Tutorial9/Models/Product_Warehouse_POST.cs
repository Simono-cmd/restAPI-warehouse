using System.ComponentModel.DataAnnotations;

namespace Tutorial9.Models;

public class Product_Warehouse_POST
{
    public int IdProduct { get; set; }
    public int IdWarehouse { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public int Amount { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
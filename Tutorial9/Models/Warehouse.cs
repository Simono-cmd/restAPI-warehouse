﻿using System.ComponentModel.DataAnnotations;

namespace Tutorial9.Models;

public class Warehouse
{
    public int IdWarehouse { get; set; }
    
    [MaxLength(200)]
    public string Name { get; set; }
    
    [MaxLength(200)]
    public string Address { get; set; }
}
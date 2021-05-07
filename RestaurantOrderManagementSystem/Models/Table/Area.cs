using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOrderManagementSystem.Models.Table
{
    public class Area : TableComponent
    {
        [NotMapped] public List<TableComponent> Children { get; set; } = new List<TableComponent>();
        
        public Area() { }
    }
}
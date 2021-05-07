using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOrderManagementSystem.Models.Table
{
    public class Table : TableComponent
    { 
        public int NumberOfSeats { get; set; } = 1;
        
        public Table() {}

    }
}
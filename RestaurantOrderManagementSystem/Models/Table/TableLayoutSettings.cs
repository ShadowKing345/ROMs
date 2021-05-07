using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RestaurantOrderManagementSystem.Models.Table
{
    public class TableLayoutSettings: TableComponent
    {
        public Guid StartingNodeId { get; set; }
        [NotMapped, JsonIgnore] public TableComponent StartingNode { get; set; } 
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RestaurantOrderManagementSystem.Models.RelationshipIntermidiary;

namespace RestaurantOrderManagementSystem.Models.Table
{
    public abstract class TableComponent
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }

        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public string Style { get; set; } = "border: 1px solid red;";

        [NotMapped] public ICollection<TableComponentTableComponent> TableComponentTableComponents1 { get; set; }
        [NotMapped] public ICollection<TableComponentTableComponent> TableComponentTableComponents2 { get; set; }
    }
}
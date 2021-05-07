using System;
using System.ComponentModel.DataAnnotations.Schema;
using RestaurantOrderManagementSystem.Models.Table;

namespace RestaurantOrderManagementSystem.Models.RelationshipIntermidiary
{
    public class TableComponentTableComponent
    {
        public Guid TableComponent1Id { get; set; }
        public Guid TableComponent2Id { get; set; }
        
        [NotMapped] public TableComponent TableComponent1 { get; set; }
        [NotMapped] public TableComponent TableComponent2 { get; set; }
    }
}
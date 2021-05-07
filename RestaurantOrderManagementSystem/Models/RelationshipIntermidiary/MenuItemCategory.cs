using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RestaurantOrderManagementSystem.Models.Menu;

namespace RestaurantOrderManagementSystem.Models.RelationshipIntermidiary
{
    public class MenuItemCategory
    {
        [Key] public Guid MenuItemId { get; set; }
        [Key] public Guid CategoryId { get; set; } // ReSharper disable UnusedAutoPropertyAccessor.Global
        [NotMapped] public virtual MenuItem MenuItem { get; set; }
        [NotMapped] public virtual Category Category { get; set; }
    }
}
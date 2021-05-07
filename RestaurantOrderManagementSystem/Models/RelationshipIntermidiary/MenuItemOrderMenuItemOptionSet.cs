using System;
using System.ComponentModel.DataAnnotations.Schema;
using RestaurantOrderManagementSystem.Models.Order;

namespace RestaurantOrderManagementSystem.Models.RelationshipIntermidiary
{
    public class MenuItemOrderMenuItemOptionSet
    {
        public Guid MenuItemOrderId { get; set; }
        public Guid MenuItemOptionSetId { get; set; } // ReSharper disable UnusedAutoPropertyAccessor.Global
        [NotMapped] public virtual MenuItemOrder MenuItemOrder { get; set; }
        [NotMapped] public virtual MenuItemOptionSet MenuItemOptionSet { get; set; }
    }
}
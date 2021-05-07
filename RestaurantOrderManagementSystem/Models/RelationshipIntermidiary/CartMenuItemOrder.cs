using System;
using System.ComponentModel.DataAnnotations.Schema;
using RestaurantOrderManagementSystem.Models.Order;

namespace RestaurantOrderManagementSystem.Models.RelationshipIntermidiary
{
    public class CartMenuItemOrder
    {
        public Guid CartId { get; set; }
        public Guid MenuItemOrderId { get; set; }
        
        [NotMapped] public Cart Cart { get; set; }
        [NotMapped] public MenuItemOrder MenuItemOrder { get; set; }
    }
}
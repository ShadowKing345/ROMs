using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.VisualBasic;
using RestaurantOrderManagementSystem.Models.Menu;
using RestaurantOrderManagementSystem.Models.RelationshipIntermidiary;

namespace RestaurantOrderManagementSystem.Models.Order
{
    public class MenuItemOrder
    {
        [Key] public Guid Id { get; set; }
        public Guid MenuItemId { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreationTimeStamp { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CompletionTimeStamp { get; set; }
        public bool IsReady { get; set; } = false;

        [NotMapped] public MenuItem MenuItem { get; set; }
        [NotMapped] public List<MenuItemOption> MenuItemOptions { get; set; }
        [NotMapped] public List<MenuItemOptionSet> ValueSets = new List<MenuItemOptionSet>();

        [NotMapped] public virtual ICollection<CartMenuItemOrder> CartMenuItemOrders { get; set; }
        [NotMapped] public virtual ICollection<MenuItemOrderMenuItemOptionSet> MenuItemOrderMenuItemOptionSets { get; set; }
    }
}
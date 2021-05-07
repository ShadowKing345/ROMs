using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using RestaurantOrderManagementSystem.Accounts;
using RestaurantOrderManagementSystem.Models.RelationshipIntermidiary;

namespace RestaurantOrderManagementSystem.Models.Order
{
    public class Cart
    {
        [Key] public Guid Id { get; set; }
        [NotMapped] public readonly List<MenuItemOrder> MenuItems = new List<MenuItemOrder>();
        public Guid TableId { get; set; }
        [NotMapped] public Table.Table Table { get; set; }
        public string WaitstaffId { get; set; }
        [NotMapped] public ApplicationUser Waitstaff { get; set; }
        public string AdditionalDetails { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreationTimeStamp { get; set; } = DateTime.Now;
        [DataType(DataType.DateTime)]
        public DateTime CompletionTimeStamp { get; set; }
        public bool IsComplete { get; set; } = false;
        public bool AreMenuItemsComplete { get; set; } = false;

        [NotMapped] public virtual ICollection<CartMenuItemOrder> CartMenuItemOrders { get; set; }
    }
}
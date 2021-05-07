using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOrderManagementSystem.Models.RelationshipIntermidiary
{
    public class MenuItemOptionSet
    {
        [Key] public Guid Id { get; set; }
        public Guid MenuItemOptionId { get; set; }
        public string Indexes { get; set; }

        [NotMapped] public virtual MenuItemOrderMenuItemOptionSet MenuItemOrderMenuItemOptionSet { get; set; }
    }
}
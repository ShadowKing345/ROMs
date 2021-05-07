using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RestaurantOrderManagementSystem.Models.RelationshipIntermidiary;

namespace RestaurantOrderManagementSystem.Models.Menu
{
    public class Category
    {
        [Key] public Guid Id { get; set; }

        [Required] public string Name { get; set; }

        public string ImagePath { get; set; }

        public virtual ICollection<MenuItemCategory> MenuItemCategories { get; set; }
    }
}
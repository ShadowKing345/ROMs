using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using RestaurantOrderManagementSystem.Models.RelationshipIntermidiary;

namespace RestaurantOrderManagementSystem.Models.Menu
{
    public class MenuItem
    {
        [Key] public Guid Id { get; set; }
        [Required] [Display(Name = "Name:")] public string Name { get; set; }
        [Display(Name = "Short Description:")] public string ShortDescription { get; set; }

        [Required]
        [Display(Name = "Description:")]
        public string LongDescription { get; set; }

        [Display(Name = "Quantity:")] public int? Quantity { get; set; }
        [Display(Name = "Price:")] public float Price { get; set; }
        [Display(Name = "Hidden in Menu?")] public bool IsHidden { get; set; }
        [DataType(DataType.ImageUrl)] public string ImagePath { get; set; }

        [NotMapped] [JsonIgnore] public virtual ICollection<MenuItemCategory> MenuItemCategories { get; set; }
        [NotMapped] [JsonIgnore] public virtual ICollection<MenuItemMenuItemOption> MenuItemOptions { get; set; }
    }
}
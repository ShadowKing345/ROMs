using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using RestaurantOrderManagementSystem.Models.Menu;

namespace RestaurantOrderManagementSystem.Models.RelationshipIntermidiary
{
    public class MenuItemMenuItemOption
    {
        [Key] public Guid MenuItemId { get; set; }
        [Key] public Guid OptionId { get; set; } // ReSharper disable UnusedAutoPropertyAccessor.Global
        [NotMapped] [JsonIgnore] public virtual MenuItem MenuItem { get; set; }
        [NotMapped] [JsonIgnore] public virtual MenuItemOption MenuItemOption { get; set; }
    }
}
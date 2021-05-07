using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RestaurantOrderManagementSystem.Models.RelationshipIntermidiary;

namespace RestaurantOrderManagementSystem.Models.Menu
{
    public class MenuItemOption
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Options { get; set; } = "";
        public string Prices { get; set; } = "";
        public MenuItemOptionType Type { get; set; } = MenuItemOptionType.Checkbox;

        public ICollection<MenuItemMenuItemOption> MenuItemOptions { get; set; }

        public Dictionary<string, string> GetValuesPairs()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string[] options = Options.Split(";");
            string[] prices = Prices.Split(";");
            for (int i = 0;  i < Math.Min(options.Length, prices.Length); i++)
            {
                if (string.IsNullOrEmpty(options[i]) || string.IsNullOrEmpty(prices[i]))
                    continue;
                result.Add(options[i], prices[i]);
            }

            return result;
        }
    }
}
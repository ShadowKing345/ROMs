using System;
using System.Collections.Generic;
using RestaurantOrderManagementSystem.Models.Menu;
using RestaurantOrderManagementSystem.Models.Order;
using RestaurantOrderManagementSystem.Models.ViewModels.Order;

namespace RestaurantOrderManagementSystem.Models.ViewModels.Waitstaff
{
    public class MenuItemOptionViewModel : Order.MenuItemOptionViewModel
    {
        public Guid CartId { get; set; }
    }
    
    public class CartViewModel
    {
        public Table.Table Table { get; set; }
        public Cart Cart { get; set; } = new Cart();
        public List<MenuItemOrderInputForm> MenuItemOrderInputForms { get; set; }
        public List<string> MenuItemOrderInputFormStrings { get; } = new List<string>();
    }
    
    public class MenuItemViewModel
    {
        public List<MenuItem> MenuItems { get; set; }
        public Guid CartId { get; set; }
    }
}
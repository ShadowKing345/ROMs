using System.Collections.Generic;
using RestaurantOrderManagementSystem.Models.Menu;
using RestaurantOrderManagementSystem.Models.Order;
using RestaurantOrderManagementSystem.Models.RelationshipIntermidiary;

namespace RestaurantOrderManagementSystem.Models.ViewModels.Order
{
    public class MenuItemOrderInputForm
    {
        public MenuItem MenuItem { get; set; }
        public Dictionary<string, string[]> Values { get; set; }
    }

    public class CartViewModel
    {
        public Cart Cart { get; set; }
        public List<Table.Table> Tables { get; set; }
        public List<MenuItemOrderInputForm> MenuItemOrderInputForms { get; set; } = new List<MenuItemOrderInputForm>();
        public List<string> MenuItemOrderInputFormStrings { get; } = new List<string>();
    }

    

    public class MenuItemOptionViewModel
    {
        public MenuItem MenuItem { get; set; }
        public List<MenuItemOption> MenuItemOptions { get; set; }
    }

    public class BillViewModel
    {
        public Table.Table Table { get; set; }
        public Cart Cart { get; set; }
        public List<MenuItemBillEntry> Entries { get; set; } = new List<MenuItemBillEntry>();
        public double Total { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
    }

    public class MenuItemBillEntry
    {
        public string Name { get; set; }
        public int Quantity { get; set; } = 1;
        public double Price { get; set; } = 0.0;
        public double Total => Price * Quantity;
    }
}
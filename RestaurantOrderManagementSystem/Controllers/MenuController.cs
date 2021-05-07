using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderManagementSystem.Data;
using RestaurantOrderManagementSystem.Models;
using RestaurantOrderManagementSystem.Models.DB;
using RestaurantOrderManagementSystem.Models.Menu;
using RestaurantOrderManagementSystem.Models.ViewModels.Waitstaff;

namespace RestaurantOrderManagementSystem.Controllers
{
    public class MenuController : Controller
    {
        private readonly MenuItemDbContext _menuItemDbContext;

        public MenuController(MenuItemDbContext menuItemDbContext)
        {
            _menuItemDbContext = menuItemDbContext;
            _menuItemDbContext.Database.EnsureCreated();
        }

        [HttpGet] public IActionResult Index()
        {
            List<MenuItem> menuItems = _menuItemDbContext.MenuItems.Where(x => !x.IsHidden).ToList();
            foreach (var t in menuItems)
            {
                var id = t.Id;
                t.MenuItemCategories = _menuItemDbContext.MenuItemCategories.Include(x => x.Category)
                    .Where(x => x.MenuItemId == id).ToList();
            }
            
            return View(menuItems);
        }

        [HttpGet] public IActionResult MenuItemDetails(Guid guid)
        {
            MenuItem menuItem = _menuItemDbContext.MenuItems.Find(guid);
            menuItem.MenuItemCategories = _menuItemDbContext.MenuItemCategories.Include(mic => mic.Category)
                .Where(mic => mic.MenuItemId == guid).ToList();
            return View("MenuItemDetails", new MenuViewModel
            {
                MenuItem = menuItem,
                Categories = _menuItemDbContext.MenuItemCategories.Where(x => x.MenuItemId == menuItem.Id).Select(x => x.Category).ToList(),
                MenuItemOptions = _menuItemDbContext.MenuItemMenuItemOptions.Where(x => x.MenuItemId == menuItem.Id).Select(x => x.MenuItemOption).ToList()
            });
        }

        [HttpGet] public PartialViewResult GetMenuItems()
        {
            return PartialView("Orders/MenuItemForm", _menuItemDbContext.MenuItems.Where(x => !x.IsHidden).ToList());
        } 
        
        [HttpGet] public PartialViewResult GetMenuItemsFromSidePanel(Guid cartId)
        {
            return PartialView("Orders/Waitstaff/MenuItemForm", new MenuItemViewModel
            {
                MenuItems = _menuItemDbContext.MenuItems.Where(x => !x.IsHidden).ToList(),
                CartId = cartId
            });
        }
    }

    public class MenuItemView
    {
        public MenuItem MenuItem { get; set; }
        public List<Category> Categories { get; set; }
    }
}
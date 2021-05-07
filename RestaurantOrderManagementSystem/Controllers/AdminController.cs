using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderManagementSystem.Data;
using RestaurantOrderManagementSystem.Models;
using RestaurantOrderManagementSystem.Models.DB;
using RestaurantOrderManagementSystem.Models.Menu;

namespace RestaurantOrderManagementSystem.Controllers
{
    [Authorize(Policy = PolicyTypes.Users.Admin)]
    public class AdminController : Controller
    {
        private readonly MenuItemDbContext _menuItemDb;

        public AdminController(MenuItemDbContext menuItemDb)
        {
            _menuItemDb = menuItemDb;
            _menuItemDb.Database.EnsureCreated();
        }

        public IActionResult Index() => View();

        // CRUD Create, Read, Update, Delete

        #region Menu Item

        [HttpGet]
        public IActionResult MenuItemDashBoard() => View("MenuItemDashboard", _menuItemDb.MenuItems.ToList());

        [HttpGet] public ActionResult CreateMenuItem() => View("Menu/Create", new MenuViewModel {Categories = _menuItemDb.Categories.ToList(), AllMenuItemOptions = _menuItemDb.MenuItemOptions.ToList()});

        [HttpGet] public ActionResult MenuItemDetails(Guid id)
        {
            MenuItem menuItem = _menuItemDb.MenuItems.Find(id);

            return View("Menu/Details", new MenuViewModel
            {
                MenuItem = menuItem,
                Categories = _menuItemDb.MenuItemCategories.Where(x => x.MenuItemId == menuItem.Id).Select(x => x.Category).ToList(),
                MenuItemOptions = _menuItemDb.MenuItemMenuItemOptions.Where(x => x.MenuItemId == menuItem.Id).Select(x => x.MenuItemOption).ToList()
            });
        }

        [HttpGet] public IActionResult EditMenuItem(Guid id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            MenuItem menuItem = _menuItemDb.MenuItems.Find(id);
            menuItem.MenuItemCategories = _menuItemDb.MenuItemCategories.Include(x => x.Category)
                .Where(entry => entry.MenuItemId == id).ToList();
            menuItem.MenuItemOptions = _menuItemDb.MenuItemMenuItemOptions.Include(x => x.MenuItemOption)
                .Where(entry => entry.MenuItemId == id).ToList();

            return View("Menu/Edit", new MenuViewModel
            {
                MenuItem = menuItem,
                Categories = _menuItemDb.Categories.ToList(),
                MenuItemOptions = _menuItemDb.MenuItemMenuItemOptions.Where(x => x.MenuItemId == menuItem.Id).Select(x => x.MenuItemOption).ToList(),
                AllMenuItemOptions = _menuItemDb.MenuItemOptions.ToList()
            });
        }

        #endregion

        #region Categories

        [HttpGet]
        public IActionResult CategoryDashBoard() => View("CategoryDashboard", _menuItemDb.Categories.ToList());

        [HttpGet]
        public IActionResult CreateCategory() => View("Category/Create");

        [HttpGet]
        public IActionResult EditCategory(Guid? id)
        {
            if (id == null)
                return BadRequest();

            return View("Category/Edit", new CategoryViewModel {Category = _menuItemDb.Categories.Find(id)});
        }

        #endregion

        #region Menu Item Options

        [HttpGet] public IActionResult OptionsDashboard() => View("MenuItemOptionDashboard", _menuItemDb.MenuItemOptions.ToList());
        

        [HttpGet] public IActionResult MenuItemOptionDetails(Guid? id)
        {
            if (id == null) return BadRequest();
            return View("Options/MenuItemOptionDetails", _menuItemDb.MenuItemOptions.Find(id));
        }

        [HttpGet]
        public IActionResult CreateMenuItemOption() => View("Options/Create", new MenuItemOption());

        [HttpGet]
        public IActionResult EditMenuItemOption(Guid? id, string returnUrl = null)
        {
            ViewData["returnUrl"] = returnUrl;
            if (id == null) return BadRequest();
            return View("Options/Edit", _menuItemDb.MenuItemOptions.Find(id));
        }

        #endregion

    }

    public class MenuViewModel
    {
        public MenuItem MenuItem { get; set; }
        public List<Category> Categories { get; set; }
        public List<MenuItemOption> MenuItemOptions { get; set; } = new List<MenuItemOption>();
        public List<MenuItemOption> AllMenuItemOptions { get; set; }
        [BindProperty]
        public IFormFile Image { get; set; }
    }

    public class CategoryViewModel
    {
        public Category Category { get; set; }
        [BindProperty]
        public IFormFile Image { get; set; }
    }
}
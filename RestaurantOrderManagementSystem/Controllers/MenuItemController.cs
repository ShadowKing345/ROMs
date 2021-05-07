using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantOrderManagementSystem.Data;
using RestaurantOrderManagementSystem.Models;
using RestaurantOrderManagementSystem.Models.DB;
using RestaurantOrderManagementSystem.Models.Menu;
using RestaurantOrderManagementSystem.Models.RelationshipIntermidiary;

namespace RestaurantOrderManagementSystem.Controllers
{
    [Authorize(Policy = PolicyTypes.Users.Admin)]
    public class MenuItemController : Controller
    {
        private readonly MenuItemDbContext _menuItemDb;

        public MenuItemController(MenuItemDbContext menuItemDb)
        {
            _menuItemDb = menuItemDb;
            _menuItemDb.Database.EnsureCreated();
        }

        #region MenuItem
        
        [HttpPost] [ValidateAntiForgeryToken] public ActionResult CreateMenuItem(MenuViewModel menuViewModel, Guid[] categoryIds, HashSet<Guid> options)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    MenuItem menuItem = menuViewModel.MenuItem;
                    _menuItemDb.MenuItems.Add(menuItem);

                    if (menuViewModel.Image != null)
                    {
                        menuItem.ImagePath = Path.Combine(_imageAssetRoot, $"{menuItem.Id}{Path.GetExtension(menuViewModel.Image.FileName)}");
                        CreateImage(menuViewModel.Image, menuItem.ImagePath);
                    }

                    CreateMenuItemCategories(categoryIds, menuItem.Id);
                    CreateMenuItemMenuItemOptions(menuItem.Id, options);
                    
                    _menuItemDb.SaveChanges();
                    return RedirectToAction("MenuItemDashBoard", "Admin");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("","Unable to save Changes. Try again, and if the problem persists see your system administrator.");
            }
            return RedirectToAction("EditMenuItem", "Admin", new { menuViewModel.MenuItem.Id });
        }

        [HttpPost] [ValidateAntiForgeryToken] public ActionResult UpdateMenuItem(Guid? id, MenuViewModel menuViewModel, Guid[] categoryIds, HashSet<Guid> options, string returnUrl = null)
        {
            if (id == null)
                return BadRequest();
            
            try
            {
                if (ModelState.IsValid)
                {
                    MenuItem menuItem = menuViewModel.MenuItem;
                    menuItem.Id = (Guid) id;
                    _menuItemDb.MenuItems.Update(menuItem);

                    if (menuViewModel.Image != null)
                    {
                        menuItem.ImagePath ??= Path.Combine(_imageAssetRoot,
                            $"{menuItem.Id}{Path.GetExtension(menuViewModel.Image.FileName)}");

                        CreateImage(menuViewModel.Image, menuItem.ImagePath);
                    }

                    CreateMenuItemCategories(categoryIds, (Guid) id, true);
                    CreateMenuItemMenuItemOptions(menuItem.Id, options, true);
                    
                    _menuItemDb.SaveChanges();
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("MenuItemDashBoard", "Admin");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("","Unable to save Changes. Try again, and if the problem persists see your system administrator.");
            }
            return RedirectToAction("EditMenuItem", "Admin", new { id });
        }

        [HttpPost] [ValidateAntiForgeryToken] public ActionResult DeleteMenuItem(Guid? id)
        {
            if (id == null)
                return new BadRequestResult();

            MenuItem menuItem = _menuItemDb.MenuItems.Find(id);
            RemoveImage(menuItem.ImagePath);
            
            _menuItemDb.MenuItems.Remove(menuItem);
            _menuItemDb.SaveChanges();

            return RedirectToAction("MenuItemDashBoard", "Admin");
        }

        #endregion

        #region Categories
        
        [HttpPost] [ValidateAntiForgeryToken] public ActionResult CreateMenuItemCategory(CategoryViewModel categoryViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    categoryViewModel.Category.Id = Guid.NewGuid();
                    _menuItemDb.Categories.Add(categoryViewModel.Category);

                    if (categoryViewModel.Image != null)
                    {
                        categoryViewModel.Category.ImagePath = Path.Combine(_imageAssetRoot,
                            $"{categoryViewModel.Category.Id}{Path.GetExtension(categoryViewModel.Image.FileName)}");
                        CreateImage(categoryViewModel.Image, categoryViewModel.Category.ImagePath);
                    }

                    _menuItemDb.SaveChanges();
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("",
                    "Unable to save Changes. Try again, and if the problem persists see your system administrator.");
            }

            return RedirectToAction("CategoryDashBoard", "Admin");
        }

        [HttpPost] [ValidateAntiForgeryToken] public ActionResult UpdateMenuItemCategory(Guid? id, CategoryViewModel categoryViewModel)
        {
            if (id == null)
                return BadRequest();

            try
            {
                if (ModelState.IsValid)
                {
                    categoryViewModel.Category.Id = (Guid) id;
                    _menuItemDb.Categories.Update(categoryViewModel.Category);
                    if (categoryViewModel.Image != null)
                    {
                        categoryViewModel.Category.ImagePath ??= Path.Combine(_imageAssetRoot,
                            $"{categoryViewModel.Category.Id}{Path.GetExtension(categoryViewModel.Image.FileName)}");

                        CreateImage(categoryViewModel.Image, categoryViewModel.Category.ImagePath);
                    }

                    _menuItemDb.SaveChanges();
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("",
                    "Unable to save Changes. Try again, and if the problem persists see your system administrator.");
            }

            return RedirectToAction("CategoryDashBoard", "Admin");
        }

        [HttpPost] [ValidateAntiForgeryToken] public ActionResult DeleteMenuItemCategory(Guid? id)
        {
            if (id == null)
                return BadRequest();

            Category category = _menuItemDb.Categories.Find(id);
            RemoveImage(category.ImagePath);
            _menuItemDb.Categories.Remove(category);
            _menuItemDb.SaveChanges();
            
            return RedirectToAction("CategoryDashBoard", "Admin");
        }

        #endregion

        #region MenuItemOption
        
        [HttpPost] [ValidateAntiForgeryToken] public ActionResult CreateMenuItemOption(MenuItemOption menuItemOption, Dictionary<string, string> options )
        {
            try
            {
                if(ModelState.IsValid) 
                {
                    menuItemOption.Options = string.Join(";", options.Keys.Select(x => x?.Replace(";", "")));
                    menuItemOption.Prices = string.Join(";", options.Values.Select(x => x?.Replace(";", "")));
                
                    _menuItemDb.MenuItemOptions.Add(menuItemOption);
                    _menuItemDb.SaveChanges();
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("","Unable to save Changes. Try again, and if the problem persists see your system administrator.");
            }
            
            return RedirectToAction("OptionsDashboard", "Admin");
        }

        [HttpPost] [ValidateAntiForgeryToken] public ActionResult UpdateMenuItemOption(Guid? id, MenuItemOption menuItemOption, Dictionary<string, string> options)
        {
            if (id == null)
                return BadRequest();
            
            try
            {
                if(ModelState.IsValid)
                {
                    menuItemOption.Id = (Guid) id;
                    menuItemOption.Options = string.Join(";", options.Keys.Select(x => x?.Replace(";", "")));
                    menuItemOption.Prices = string.Join(";", options.Values.Select(x => x?.Replace(";", "")));
                
                    _menuItemDb.MenuItemOptions.Update(menuItemOption);
                    _menuItemDb.SaveChanges();
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("","Unable to save Changes. Try again, and if the problem persists see your system administrator.");
            }
            
            return RedirectToAction("OptionsDashboard", "Admin");

        }

        [HttpPost] [ValidateAntiForgeryToken] public ActionResult DeleteMenuItemOption(Guid? id)
        {
            if (id == null) return BadRequest();

            _menuItemDb.MenuItemOptions.Remove(_menuItemDb.MenuItemOptions.Find(id));
            _menuItemDb.SaveChanges();
            
            return RedirectToAction("OptionsDashboard", "Admin");
        }

        #endregion

        #region Utils

        private void CreateMenuItemCategories(Guid[] categoryIds, Guid menuItemId, bool clearExisting = false)
        {
            if (clearExisting)
                _menuItemDb.MenuItemCategories.RemoveRange(
                    _menuItemDb.MenuItemCategories.Where(m => m.MenuItemId == menuItemId));

            foreach (Guid categoryId in categoryIds)
            {
                MenuItemCategory menuItemCategory = new MenuItemCategory
                {
                    CategoryId = categoryId,
                    MenuItemId = menuItemId
                };

                _menuItemDb.MenuItemCategories.Add(menuItemCategory);
            }
        }

        private void CreateMenuItemMenuItemOptions(Guid menuItemId, HashSet<Guid> options, bool clearExisting = false)
        {
            if (clearExisting)
                _menuItemDb.MenuItemMenuItemOptions.RemoveRange(
                    _menuItemDb.MenuItemMenuItemOptions.Where(m => m.MenuItemId == menuItemId));

            foreach (Guid option in options)
            {
                MenuItemMenuItemOption menuItemOption = new MenuItemMenuItemOption
                {
                    MenuItemId = menuItemId,
                    OptionId = option
                };

                _menuItemDb.MenuItemMenuItemOptions.Add(menuItemOption);
            }
        }

        private readonly string _websiteContentRoot = "wwwroot";
        private readonly string _imageAssetRoot = "Assets/Images/";
        private void CreateImage(IFormFile image, string imagePath)
        {
            if (image == null || string.IsNullOrEmpty(imagePath) || string.IsNullOrWhiteSpace(imagePath))
                return;
            
            if (!image.ContentType.StartsWith("image"))
                return;
            
            if (!Directory.Exists(Path.Combine(_websiteContentRoot, _imageAssetRoot)))
                Directory.CreateDirectory(Path.Combine(_websiteContentRoot, _imageAssetRoot));

            using Stream fileStream = new FileStream(Path.Combine(_websiteContentRoot, imagePath), FileMode.Create);
            image.CopyTo(fileStream);
        }
        
        private void RemoveImage(string imagePath)
        {
            if(imagePath == null) return;

            if (System.IO.File.Exists(Path.Combine(_websiteContentRoot, imagePath)))
            {
                try
                {
                    System.IO.File.Delete(Path.Combine(_websiteContentRoot, imagePath));
                } catch (FileNotFoundException) {}
                catch (Exception e) { Console.WriteLine(e); }
            }
        }
        
        #endregion
    }
}
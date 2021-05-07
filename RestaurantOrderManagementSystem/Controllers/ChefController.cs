using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderManagementSystem.Data;
using RestaurantOrderManagementSystem.Models.DB;
using RestaurantOrderManagementSystem.Models.Order;
using RestaurantOrderManagementSystem.Models.RelationshipIntermidiary;
using RestaurantOrderManagementSystem.Models.Table;

namespace RestaurantOrderManagementSystem.Controllers
{
    [Authorize(Policy = PolicyTypes.Users.Chef)]

    public class ChefController : Controller
    {
        private readonly OrderDbContext _orderDb;
        private readonly TableDbContext _tableDb;
        private readonly MenuItemDbContext _menuItemDb;
        
        public ChefController(OrderDbContext orderDb, TableDbContext tableDb, MenuItemDbContext menuItemDb)
        {
            _orderDb = orderDb;
            _orderDb.Database.EnsureCreated();
            _tableDb = tableDb;
            _tableDb.Database.EnsureCreated();
            _menuItemDb = menuItemDb;
            _menuItemDb.Database.EnsureCreated();
        }

        public IActionResult Index()
        {
            List<ChefOrderViewModel> model = new List<ChefOrderViewModel>();
            List<Cart> carts = _orderDb.Carts.Where(x => x.CompletionTimeStamp < x.CreationTimeStamp).ToList();

            foreach (Cart cart in carts)
            {
                if (cart.IsComplete) continue;
                List<MenuItemOrder> menuItemOrders = _orderDb.CartMenuItemOrders
                    .Include(x => x.MenuItemOrder)
                    .Where(x => x.CartId == cart.Id)
                    .Select(x => x.MenuItemOrder).ToList();
                
                bool completedMenuItemsFlag = true;
                
                menuItemOrders.ForEach(item =>
                {
                    if (!item.IsReady) completedMenuItemsFlag = false;
                    item.MenuItem = _menuItemDb.MenuItems.Find(item.MenuItemId);
                    item.ValueSets = _orderDb.MenuItemOrderMenuItemOptionSets.Where(x => x.MenuItemOrderId == item.Id).Select(x => x.MenuItemOptionSet).ToList();
                    item.MenuItemOptions = _menuItemDb.MenuItemMenuItemOptions.Where(x => x.MenuItemId == item.MenuItemId).Select(x => x.MenuItemOption).ToList();
                });
                
                if (completedMenuItemsFlag) continue;
                
                model.Add(new ChefOrderViewModel
                {
                    Cart = cart,
                    Table = _tableDb.Tables.Find(cart.TableId),
                    MenuItemOrders = menuItemOrders
                });
            }
            
            return View("Index", model);
        }

        [HttpPost] public IActionResult MarkMenuItemReadyToCollect(Guid? id)
        {
            if (id == null) return BadRequest();

            MenuItemOrder order = _orderDb.CartMenuItemOrders.Include(x => x.MenuItemOrder)
                .First(x => x.MenuItemOrderId == id).MenuItemOrder;
            order.IsReady = true;

            _orderDb.MenuItemOrders.Update(order);
            _orderDb.SaveChanges();
            
            CartMenuItemOrder cartMenuItemOrder = _orderDb.CartMenuItemOrders.First(x => x.MenuItemOrderId == id);
            {
                Cart cart = _orderDb.Carts.Find(cartMenuItemOrder.CartId);
                if (_orderDb.CartMenuItemOrders.FirstOrDefault(x => x.CartId == cart.Id && !x.MenuItemOrder.IsReady) == null)
                {
                    cart.AreMenuItemsComplete = true;
                    _orderDb.Carts.Update(cart);
                    _orderDb.SaveChanges();
                }
            }

            return Ok();
        }

        [HttpPost] public IActionResult MarkOrderAsReady(Guid? id)
        {
            if (id == null) return BadRequest();

            Cart cart = _orderDb.Carts.Find(id);
            List<MenuItemOrder> orders = _orderDb.CartMenuItemOrders.Include(x => x.MenuItemOrder).Where(x => x.CartId == id).Select(x => x.MenuItemOrder).ToList();
            foreach (MenuItemOrder mi in orders)
            {
                mi.IsReady = true;
                _orderDb.MenuItemOrders.Update(mi);
            }
            cart.AreMenuItemsComplete = true;
            _orderDb.Carts.Update(cart);
            
            _orderDb.SaveChanges();
            
            return Ok();
        }
    }

    public class ChefOrderViewModel
    {
        public Cart Cart { get; set; }
        public Table Table { get; set; }
        public List<MenuItemOrder> MenuItemOrders { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RestaurantOrderManagementSystem.Models.Menu;
using RestaurantOrderManagementSystem.Models.Order;
using RestaurantOrderManagementSystem.Models.Table;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using RestaurantOrderManagementSystem.Accounts;
using RestaurantOrderManagementSystem.Models.DB;
using RestaurantOrderManagementSystem.Models.RelationshipIntermidiary;
using RestaurantOrderManagementSystem.Models.ViewModels.Order;
using CartViewModel = RestaurantOrderManagementSystem.Models.ViewModels.Order.CartViewModel;
using MenuItemOptionViewModel = RestaurantOrderManagementSystem.Models.ViewModels.Waitstaff.MenuItemOptionViewModel;

namespace RestaurantOrderManagementSystem.Controllers
{
    [Authorize(Policy = PolicyTypes.Users.Waitstaff)]
    public class OrdersController : Controller
    {
        private readonly OrderDbContext _orderDb;
        private readonly TableDbContext _tableDb;
        private readonly MenuItemDbContext _menuItemDb;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(OrderDbContext orderDb, TableDbContext tableDbContext, MenuItemDbContext menuItemDb, UserManager<ApplicationUser> userManager)
        {
            _menuItemDb = menuItemDb;
            _menuItemDb.Database.EnsureCreated();
            _tableDb = tableDbContext;
            _tableDb.Database.EnsureCreated();
            _orderDb = orderDb;
            _orderDb.Database.EnsureCreated();
            _userManager = userManager;
        }

        [HttpGet] public IActionResult OrderDashboard()
        {
            List<Cart> carts = _orderDb.Carts.ToList();
            
            carts.ForEach(c =>
            {
                var result = _userManager.FindByIdAsync(c.WaitstaffId);
                if (result.IsCompletedSuccessfully)
                {
                    c.Waitstaff = result.Result;
                }
            });
            
            return View("OrderDashboard", _orderDb.Carts.ToList());
        }

        #region Cart

        [HttpGet] public IActionResult CreateCart() => View("Create", new CartViewModel {Tables = _tableDb.Tables.ToList()});

        [HttpGet] public IActionResult CreateCartPartial(Guid? id) => id == null ? (IActionResult) BadRequest() : PartialView("Orders/CartForm", new Models.ViewModels.Waitstaff.CartViewModel
        {
            Table = _tableDb.Tables.Find(id)
        });

        [HttpGet] public IActionResult EditCart(Guid? id)
        {
            if (id == null) return BadRequest();

            Cart cart = _orderDb.Carts.Find(id);
            
            return View("Edit", new CartViewModel
            {
                Cart = cart,
                Tables = _tableDb.Tables.ToList(),
                MenuItemOrderInputForms = GetMenuItemOrderInputForms(cart.Id)
            });
        }

        [HttpPost] [ValidateAntiForgeryToken] public IActionResult CreateCart(CartViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    viewModel.Cart.CreationTimeStamp = DateTime.Now;
                    viewModel.Cart.WaitstaffId = _userManager.GetUserId(HttpContext.User);
                    _orderDb.Carts.Add(viewModel.Cart);
                    
                    List<MenuItemOrder> menuItemOrders = CreateMenuItemOrders(viewModel.MenuItemOrderInputFormStrings);
                    CreateCartMenuItemLinks(menuItemOrders, viewModel.Cart.Id);

                    _orderDb.SaveChanges();
                    return RedirectToAction("OrderDashboard", "Orders");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("","Unable to save Changes. Try again, and if the problem persists see your system administrator.");
            }

            return BadRequest();
        }
        
        [HttpPost] [ValidateAntiForgeryToken] public IActionResult UpdateCart(Guid? id, CartViewModel viewModel)
        {
            if (id == null) return BadRequest();
            
            try
            {
                if (ModelState.IsValid)
                {
                    viewModel.Cart.Id = (Guid) id;
                    _orderDb.Carts.Update(viewModel.Cart);

                    List<MenuItemOrder> menuItemOrders = CreateMenuItemOrders(viewModel.MenuItemOrderInputFormStrings);
                    CreateCartMenuItemLinks(menuItemOrders, (Guid) id, true);
                    
                    _orderDb.SaveChanges();
                    return RedirectToAction("OrderDashboard", "Orders");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("","Unable to save Changes. Try again, and if the problem persists see your system administrator.");
            }

            return BadRequest();
        }
        
        [HttpPost] [ValidateAntiForgeryToken] public IActionResult DeleteCart(Guid? id)
        {
            if (id == null) return BadRequest();

            try
            {
                if (ModelState.IsValid)
                {
                    _orderDb.Carts.Remove(_orderDb.Carts.Find(id));
                    foreach (CartMenuItemOrder cartMenuItemOrder in _orderDb.CartMenuItemOrders.Where(x =>
                        x.CartId == (Guid) id))
                    {
                        _orderDb.CartMenuItemOrders.Remove(cartMenuItemOrder);

                        foreach (MenuItemOrderMenuItemOptionSet set in _orderDb.MenuItemOrderMenuItemOptionSets.Where(
                            x =>
                                x.MenuItemOrderId == _orderDb.MenuItemOrders
                                    .Remove(_orderDb.MenuItemOrders.Find(cartMenuItemOrder.MenuItemOrderId)).Entity.Id))
                        {
                            _orderDb.MenuItemOrderMenuItemOptionSets.Remove(set);
                            _orderDb.MenuItemOptionSets.Remove(
                                _orderDb.MenuItemOptionSets.Find(set.MenuItemOptionSetId));
                        }
                    }

                    _orderDb.SaveChanges();
                    return RedirectToAction("OrderDashboard");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("","Unable to save Changes. Try again, and if the problem persists see your system administrator.");
            }

            return BadRequest();
        }

        [HttpPost] [ValidateAntiForgeryToken] public IActionResult CompleteCart(Guid? id)
        {
            if (id == null) return BadRequest();

            try
            {
                if(ModelState.IsValid){
                    Cart cart = _orderDb.Carts.Find(id);
                    cart.CompletionTimeStamp = DateTime.Now;
                    cart.IsComplete = true;
                    _orderDb.Carts.Update(cart);
                    _orderDb.SaveChanges();
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("","Unable to save Changes. Try again, and if the problem persists see your system administrator.");
            }

            return RedirectToAction("OrderDashboard", "Orders");
        }

        #endregion

        #region WaitStaff
        
        [HttpGet] public IActionResult GetMenuItemOptions(Guid? id) =>
            id == null ? (IActionResult) BadRequest() :
                PartialView("Orders/MenuItemOptionForm", new Models.ViewModels.Order.MenuItemOptionViewModel
                {
                    MenuItem = _menuItemDb.MenuItems.Find(id), MenuItemOptions = _menuItemDb.MenuItemMenuItemOptions.Where(x => x.MenuItemId == (Guid) id).Select(x => x.MenuItemOption).ToList()
                });

        [HttpGet] public IActionResult GetMenuItemOptionsFromSidePanel(Guid? id, Guid cartId) =>
            id == null ? (IActionResult) BadRequest() :
                PartialView("Orders/Waitstaff/MenuItemOptionForm", new MenuItemOptionViewModel
                {
                    MenuItem = _menuItemDb.MenuItems.Find(id),
                    MenuItemOptions = _menuItemDb.MenuItemMenuItemOptions.Where(x => x.MenuItemId == (Guid) id).Select(x => x.MenuItemOption).ToList(),
                    CartId = cartId
                });

        [HttpGet] public IActionResult GetTableCart(Guid? id)
        {
            if (id == null) return BadRequest();

            Table table = _tableDb.Tables.Find(id);
            Cart cart = _orderDb.Carts.Where(x => !x.IsComplete).FirstOrDefault(x => x.TableId == table.Id);
            
            return PartialView("Orders/TablePanelView", new Models.ViewModels.Waitstaff.CartViewModel
            {
                Table = table,
                Cart = cart,
                MenuItemOrderInputForms = cart != null ? GetMenuItemOrderInputForms(cart.Id) : null
            });
        }
        
        [HttpPost] [ValidateAntiForgeryToken] public IActionResult WaitstaffCreateCart(Models.ViewModels.Waitstaff.CartViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.Cart.WaitstaffId = _userManager.GetUserId(HttpContext.User);
                    _orderDb.Carts.Add(model.Cart);
                    
                    List<MenuItemOrder> menuItemOrders = CreateMenuItemOrders(model.MenuItemOrderInputFormStrings);
                    CreateCartMenuItemLinks(menuItemOrders, model.Cart.Id);

                    _orderDb.SaveChanges();
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("","Unable to save Changes. Try again, and if the problem persists see your system administrator.");
            }
            return RedirectToAction("WaitstaffTableView", "Table");
        }

        [HttpPost] [ValidateAntiForgeryToken] public IActionResult WaitstaffCompleteCart(Guid? id)
        {
            if (id == null) return BadRequest();
            CompleteCart(id);
            return RedirectToAction("WaitstaffTableView", "Table");
        }

        [HttpPost] public IActionResult WaitstaffAddMenuItemToOrder(string obj, Guid cartId)
        {
            if (cartId == Guid.Empty) return BadRequest();

            Cart cart = _orderDb.Carts.Find(cartId);

            List<MenuItemOrder> menuItemOrders = CreateMenuItemOrders(new List<string>(new []{obj}));
            CreateCartMenuItemLinks(menuItemOrders, cart.Id);

            _orderDb.SaveChanges();
            
            return Ok();
        }
        
        [HttpGet] public IActionResult GenerateBill(Guid? id)
        {
            if (id == null) return BadRequest();

            Cart cart = _orderDb.Carts.Find(id);
            Table table = _tableDb.Tables.Find(cart.TableId);

            double total = 0.0;
            List<MenuItemBillEntry> entries = new List<MenuItemBillEntry>();

            foreach (MenuItemOrder order in _orderDb.CartMenuItemOrders.Where(x => x.CartId == cart.Id).Select(x => x.MenuItemOrder))
            {
                MenuItem item = _menuItemDb.MenuItems.Find(order.MenuItemId);
                MenuItemBillEntry entry = entries.Find(x => x.Name == item.Name);
                if (entry != null)
                {
                    entry.Quantity++;
                }
                else
                {
                    entries.Add(new MenuItemBillEntry
                    {
                        Name = item.Name,
                        Price = item.Price
                    });
                }

                foreach (MenuItemOptionSet optionSet in _orderDb.MenuItemOrderMenuItemOptionSets.Where(x => x.MenuItemOrderId == order.Id).Select(x => x.MenuItemOptionSet))
                {
                    MenuItemOption option = _menuItemDb.MenuItemOptions.Find(optionSet.MenuItemOptionId);
                    foreach (KeyValuePair<string, string> pair in option.GetValuesPairs())
                    {
                        MenuItemBillEntry optionEntry = entries.Find(x => x.Name == pair.Key);
                        if (optionEntry != null)
                        {
                            optionEntry.Quantity++;
                        }
                        else
                        {
                            if (!double.TryParse(pair.Value, out var price))
                                price = 0.0;
                            
                            entries.Add(new MenuItemBillEntry
                            {
                                Name = option.Name + " : " + pair.Key,
                                Price = price
                            });
                        }
                    }
                }
            }

            entries.ForEach(x => total += x.Price * x.Quantity);
            
            return PartialView("Orders/BillPartial", new BillViewModel
            {
                Table = table,
                Cart = cart,
                Entries = entries,
                Footer = "Buy Goods: 123456\nThank You for Coming. Do come again!".Replace("\n", "<br/>"),
                Header = $"Cart ID: {cart.Id}\nTable Name: {table.Name}".Replace("\n", "<br/>"),
                Total = total
            });
        }

        [HttpGet] public IActionResult IsOrderReady(Guid? id)
        {
            if (id == null) return BadRequest();

            Cart cart = _orderDb.Carts.Where(x => x.TableId == (Guid) id).ToList().LastOrDefault(); 
            if (cart != null && cart.AreMenuItemsComplete && !cart.IsComplete)
                return Content("<i class='bi bi-bell'></i>");
            return Content("");
        }
        
        #endregion

        #region Utils

        public List<MenuItemOrder> CreateMenuItemOrders(List<string> menuItemOrderInputFormsStrings)
        {
            List<MenuItemOrderInputForm> menuItemOrderInputForms = menuItemOrderInputFormsStrings.Select(s => JsonSerializer.Deserialize<MenuItemOrderInputForm>(s)).ToList();

            List<MenuItemOrder> menuItemOrders = new List<MenuItemOrder>();
            foreach (MenuItemOrderInputForm menuItemOrderInputForm in menuItemOrderInputForms)
            {
                MenuItemOrder menuItemOrder = new MenuItemOrder
                {
                    Id = Guid.NewGuid(),
                    MenuItemId = menuItemOrderInputForm.MenuItem.Id,
                    CreationTimeStamp = DateTime.Now
                };

                List<MenuItemOptionSet> menuItemOptionSets = new List<MenuItemOptionSet>();
                foreach (KeyValuePair<string, string[]> value in menuItemOrderInputForm.Values)
                {
                    MenuItemOptionSet set = new MenuItemOptionSet
                    {
                        MenuItemOptionId = Guid.Parse(value.Key),
                        Indexes = string.Join(";", value.Value)
                    };
                    menuItemOptionSets.Add(set);
                }

                menuItemOrder.ValueSets = menuItemOptionSets;
                menuItemOrders.Add(menuItemOrder);
            }

            return menuItemOrders;
        }

        public void CreateCartMenuItemLinks(List<MenuItemOrder> menuItemOrders, Guid cartId, bool clearExisting = false)
        {
            List<CartMenuItemOrder> cartMenuItemOrders = new List<CartMenuItemOrder>();
            List<MenuItemOrderMenuItemOptionSet> menuItemOrderMenuItemOptionSets = new List<MenuItemOrderMenuItemOptionSet>();
            
            if (clearExisting)
            {
                foreach (MenuItemOrder order in _orderDb.CartMenuItemOrders
                    .Where(x => x.CartId == cartId)
                    .Select(x => x.MenuItemOrder)
                    .ToList())
                {
                    _orderDb.MenuItemOptionSets.RemoveRange(order.ValueSets);
                    _orderDb.MenuItemOrders.Remove(order);
                }
            }
            _orderDb.MenuItemOrders.AddRange(menuItemOrders);
            foreach (MenuItemOrder menuItemOrder in menuItemOrders)
            {
                cartMenuItemOrders.Add(new CartMenuItemOrder
                {
                    CartId = cartId,
                    MenuItemOrderId = menuItemOrder.Id
                });

                _orderDb.MenuItemOptionSets.AddRange(menuItemOrder.ValueSets);
                foreach (MenuItemOptionSet set in menuItemOrder.ValueSets)
                    menuItemOrderMenuItemOptionSets.Add(new MenuItemOrderMenuItemOptionSet
                    {
                        MenuItemOrderId = menuItemOrder.Id,
                        MenuItemOptionSetId = set.Id
                    });
            }
            
            _orderDb.CartMenuItemOrders.AddRange(cartMenuItemOrders);
            _orderDb.MenuItemOrderMenuItemOptionSets.AddRange(menuItemOrderMenuItemOptionSets);
        }

        public List<MenuItemOrderInputForm> GetMenuItemOrderInputForms(Guid cartId)
        {
            List<MenuItemOrderInputForm> menuItemOrderInputForms = new List<MenuItemOrderInputForm>();
            foreach (MenuItemOrder menuItemOrder in _orderDb.CartMenuItemOrders
                .Where(x => x.CartId == cartId)
                .Select(x => x.MenuItemOrder).ToList())
            {
                Dictionary<string, string[]> values = new Dictionary<string, string[]>();
                foreach (MenuItemOptionSet set in _orderDb.MenuItemOrderMenuItemOptionSets
                    .Where(x => x.MenuItemOrderId == menuItemOrder.Id)
                    .Select(x => x.MenuItemOptionSet).ToList())
                    values.Add(set.Id.ToString(), set.Indexes.Split(';'));
                
                MenuItem menuItem = _menuItemDb.MenuItems.Find(menuItemOrder.MenuItemId);
                
                MenuItemOrderInputForm menuItemOrderInputForm = new MenuItemOrderInputForm
                {
                    MenuItem = menuItem,
                    Values = values
                };
                menuItemOrderInputForms.Add(menuItemOrderInputForm);
            }

            return menuItemOrderInputForms;
        }

        #endregion
    }
}
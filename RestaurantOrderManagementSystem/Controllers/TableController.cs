using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestaurantOrderManagementSystem.Data;
using RestaurantOrderManagementSystem.Models.DB;
using RestaurantOrderManagementSystem.Models.RelationshipIntermidiary;
using RestaurantOrderManagementSystem.Models.Table;


namespace RestaurantOrderManagementSystem.Controllers
{
    [Authorize(Policy = PolicyTypes.Users.Admin)]
    public class TableController : Controller
    {
        private readonly TableDbContext _tableDb;
        private readonly TableLayoutSettings _settings;
        
        public TableController(TableDbContext tableDb)
        {
            _tableDb = tableDb;
            _tableDb.Database.EnsureCreated();
            
           if (!System.IO.File.Exists("TableSettings.json"))
            {
                var streamWriter = System.IO.File.CreateText("TableSettings.json");
                _settings ??= new TableLayoutSettings();
                streamWriter.Write(JsonSerializer.Serialize(_settings));
                streamWriter.Close();
            }

            try
            {
                _settings = JsonSerializer.Deserialize<TableLayoutSettings>(
                    System.IO.File.ReadAllText("TableSettings.json"));
            }
            catch (JsonException)
            {
                _settings ??= new TableLayoutSettings();
                System.IO.File.WriteAllText("TableSettings.json",JsonSerializer.Serialize(_settings));
            }

            if (_settings.StartingNodeId == Guid.Empty || _tableDb.TableComponent.Find(_settings.StartingNodeId) == null)
            {
                Area area = new Area
                {
                    Name = "Restaurant"
                };
                _settings.StartingNodeId = area.Id;
                _tableDb.Areas.Add(area);
                _tableDb.SaveChanges();
                System.IO.File.WriteAllText("TableSettings.json", JsonSerializer.Serialize(_settings));
            }
        }

        [AllowAnonymous] [Authorize(Policy = PolicyTypes.Users.Waitstaff)] [HttpGet] public IActionResult WaitstaffTableView()
        {
            Area startingNode = (Area) SetChildrenComponents(_tableDb.Areas.Find(_settings.StartingNodeId), _tableDb.TableComponentTableComponent.ToList(),
                _tableDb.TableComponent.ToList());
            
            return View("WaitstaffTableView", startingNode);
        }

        #region Table

        [HttpGet] public IActionResult TableDashboard()
        {
            Area startingNode = _tableDb.Areas.Find(_settings.StartingNodeId);
            
            // Send the ones which no parent node.
            return View("TableDashboard", new TableViewModel
            {
                TableLayoutSettings = _settings,
                StartingNode = startingNode != null ? (Area) SetChildrenComponents(startingNode,
                        _tableDb.TableComponentTableComponent.ToList(),
                        _tableDb.TableComponent.ToList())
                    : null,
                UnusedComponents = _tableDb.TableComponent.Where(t => _tableDb.TableComponentTableComponent.First(c => c.TableComponent2Id == t.Id) == null && t.Id != _settings.StartingNodeId).ToList()
            });
        }
        
        [HttpGet] public IActionResult CreateTable() => PartialView("Admin/_TableForm", new Table());
        [HttpGet] public IActionResult UpdateTable(Guid? guid) => guid == null ? (IActionResult) BadRequest() : PartialView("Admin/_TableForm", _tableDb.Tables.Find(guid));

        [HttpPost] [ValidateAntiForgeryToken] public IActionResult CreateTable(Guid guid, Table table)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _tableDb.Tables.Add(table);
                    CreateTableComponent(_tableDb.TableComponent.Find(guid), table);
                    _tableDb.SaveChanges();

                    return RedirectToAction("TableDashboard", "Table");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("","Unable to save Changes. Try again, and if the problem persists see your system administrator.");
            }

            return BadRequest();
        }
        [HttpPost] [ValidateAntiForgeryToken] public IActionResult UpdateTable(Guid? guid, Table table)
        {
            if (guid == null)
                return BadRequest();
            
            try
            {
                if (ModelState.IsValid)
                {
                    table.Id = (Guid) guid;
                    _tableDb.Tables.Update(table);
                    _tableDb.SaveChanges();
                    return RedirectToAction("TableDashboard", "Table");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("","Unable to save Changes. Try again, and if the problem persists see your system administrator.");
            }
            
            return BadRequest();
        }
        [HttpPost] [ValidateAntiForgeryToken] public IActionResult DeleteTable(Guid? guid)
        {
            if (guid == null)
                return BadRequest();

            try
            {
                if (ModelState.IsValid)
                {
                    Table table = _tableDb.Tables.Find(guid);
                    _tableDb.Tables.Remove(table);
                    _tableDb.SaveChanges();
                    return RedirectToAction("TableDashboard", "Table");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("","Unable to save Changes. Try again, and if the problem persists see your system administrator.");
            }
            
            return BadRequest();
        }

        #endregion

        #region Area

        [HttpGet] public IActionResult CreateArea() => PartialView("Admin/_AreaForm", new Area());
        [HttpGet] public IActionResult UpdateArea(Guid? guid) => guid == null ? (IActionResult) BadRequest() : PartialView("Admin/_AreaForm", _tableDb.Areas.Find(guid));

        [HttpPost] [ValidateAntiForgeryToken] public IActionResult CreateArea(Guid guid, Area area)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    _tableDb.Areas.Add(area);
                    CreateTableComponent(_tableDb.TableComponent.Find(guid), area);
                    _tableDb.SaveChanges();
                    return RedirectToAction("TableDashboard", "Table");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("","Unable to save Changes. Try again, and if the problem persists see your system administrator.");
            }

            return RedirectToAction("TableDashboard", "Table");
        }

        [HttpPost] [ValidateAntiForgeryToken] public IActionResult UpdateArea(Guid? guid, Area table)
        {
            try
            {
                if(guid != null && ModelState.IsValid)
                {
                    table.Id = (Guid) guid;
                    _tableDb.Entry(_tableDb.Areas.Find(guid)).CurrentValues.SetValues(table);
                    _tableDb.SaveChanges();
                    return RedirectToAction("TableDashboard", "Table");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("","Unable to save Changes. Try again, and if the problem persists see your system administrator.");
            }

            return RedirectToAction("TableDashboard", "Table");
        }

        [HttpPost] [ValidateAntiForgeryToken] public IActionResult DeleteArea(Guid? guid)
        {
            try
            {
                if(guid != null && ModelState.IsValid)
                {
                    _tableDb.Areas.Remove(_tableDb.Areas.Find(guid));
                    _tableDb.SaveChanges();
                    return RedirectToAction("TableDashboard", "Table");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("","Unable to save Changes. Try again, and if the problem persists see your system administrator.");
            }

            return RedirectToAction("TableDashboard", "Table");
        }

        #endregion

        #region Utils

        public void CreateTableComponent(TableComponent component1, TableComponent component2)
        {
            _tableDb.TableComponentTableComponent.Add(
                new TableComponentTableComponent
                {
                    TableComponent1Id = component1.Id,
                    TableComponent2Id = component2.Id
                }
            );
        }
        
        public string LoadAsString(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                return System.IO.File.ReadAllText(filePath);
            }

            return "";
        }
        
        private TableComponent SetChildrenComponents(TableComponent component, List<TableComponentTableComponent> tableComponentTableComponents, List<TableComponent> tableComponents)
        {
            if (component is Area area)
            {
                foreach (Guid tableComponentId in tableComponentTableComponents
                    .Where(c => c.TableComponent1Id == component.Id).Select(c => c.TableComponent2Id))
                {
                    TableComponent tableComponent = tableComponents.Find(t => t.Id == tableComponentId);
                    if (tableComponent == null) continue;
                    
                    area.Children.Add(tableComponent);
                    if (tableComponent is Area)
                        SetChildrenComponents(tableComponent, tableComponentTableComponents, tableComponents);
                }
                
            }
            
            return component;
        }

        #endregion
    }
    
    public class TableViewModel
    {
        public TableLayoutSettings TableLayoutSettings { get; set; }
        public Area StartingNode { get; set; }
        public List<TableComponent> UnusedComponents { get; set; }
    }
}
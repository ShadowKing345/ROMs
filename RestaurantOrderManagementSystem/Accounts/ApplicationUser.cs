using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace RestaurantOrderManagementSystem.Accounts
{
    public class ApplicationUser: IdentityUser
    {
        [PersonalData] public string FirstName { get; set; }
        [PersonalData] public string MiddleNames { get; set; }
        [PersonalData] public string LastName { get; set; }
        [PersonalData, DataType(DataType.Date)] public DateTime DOB { get; set; }
        [PersonalData] public string IDNumber { get; set; }
        [PersonalData, DataType(DataType.PhoneNumber)] public override string PhoneNumber { get; set; }
        
        public bool IsAdmin { get; set; }
    }
}
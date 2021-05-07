using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderManagementSystem.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestaurantOrderManagementSystem.Accounts;
using RestaurantOrderManagementSystem.Controllers;
using RestaurantOrderManagementSystem.Models.DB;

namespace RestaurantOrderManagementSystem
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AccountDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("Authentication")));
            services.AddDbContext<MenuItemDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("MenuConnection")));
            services.AddDbContext<OrderDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("OrderConnection")));
            services.AddDbContext<TableDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("TableConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AccountDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyTypes.Users.Admin, policy => { policy.RequireClaim(CustomClaimTypes.Permission.Admin, true.ToString());});
                options.AddPolicy(PolicyTypes.Users.Waitstaff,
                    policy => { policy.RequireClaim(CustomClaimTypes.Permission.WaitStaff, true.ToString()); });
                options.AddPolicy(PolicyTypes.Users.Chef,
                    policy => { policy.RequireClaim(CustomClaimTypes.Permission.Chef, true.ToString()); });
                options.AddPolicy(PolicyTypes.Users.Manager,
                    policy => { policy.RequireClaim(CustomClaimTypes.Permission.Manager, true.ToString()); });
            });
            
            services.AddDirectoryBrowser();
            services.AddControllersWithViews().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null).AddRazorRuntimeCompilation();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
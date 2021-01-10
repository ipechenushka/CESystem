using System.Globalization;
using System.Threading.Tasks;
using CESystem.AdminPart;
using CESystem.ClientPart;
using CESystem.DB;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CESystem
{
    public class Startup
    {

        public Startup(IConfiguration config)
        {
            Configuration = config;
        }
        
        public IConfiguration Configuration { set; get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddControllers();
            services.AddLogging();
            services.AddAuthentication();
            services.AddDbContext<LocalDbContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("test"));
            });
    
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAdminService, AdminService>();

            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => 
                {
                    options.LoginPath = new PathString("/login");
                    options.AccessDeniedPath = new PathString("/login");
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            
            app.UseDeveloperExceptionPage();
            app.UseStatusCodePages();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
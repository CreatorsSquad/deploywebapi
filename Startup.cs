using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CreatorsSquad.Helpers;
using CreatorsSquad.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;

namespace CreatorsSquad
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
            var emailconfig = Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
            services.AddSingleton(emailconfig);

            string connString = string.Empty;
            var server = Configuration["MYSQL_SERVICE_HOST"] ?? "localhost";
            var port = Configuration["MYSQL_SERVICE_PORT"] ?? "3306";
            var user = Configuration["MYSQL_USER"] ?? "root";
            var password = Configuration["MYSQL_PASSWORD"] ?? "wWbFync5fEazeqRg";
            var database = Configuration["MYSQL_DATABASE"] ?? "contentsqd_db";

            connString = $"Server={server};Port={port};Database={database};User ID={user};Password={password}";

            services.AddDbContextPool<NGCoreJWT_DbContext>(options => options.UseMySql(connString, ServerVersion.AutoDetect(connString)));

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            // database connection
            //services.AddDbContext<NGCoreJWT_DbContext>(option =>
            //option.UseMySQL(Configuration.GetConnectionString("angularcoreaws")));
            // option.UseSqlServer(Configuration.GetConnectionString("angularcoreaws")));

            // enable cors - cross origin resource sharing or cross origin request
            services.AddCors();

            //services.AddCors(options =>
            //{
            //    options.AddPolicy("CorsPolicy",
            //    builder => builder.WithOrigins("http://angular8deploy-cs.s3-website.ap-south-1.amazonaws.com/", "http://localhost:4200")
            //    .AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(isOriginAllowed: _ => true));
            //});

            services.AddMvc(opt => opt.EnableEndpointRouting = false).AddControllersAsServices();
            services.AddScoped(p => new NGCoreJWT_DbContext(p.GetService<DbContextOptions<NGCoreJWT_DbContext>>()));
            services.Configure<AppSettings>(Configuration.GetSection("StoreinAWS_S3"));


            const int maxRequestLimit = 1073741824;
            // If using IIS
            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = maxRequestLimit;
            });
            // If using Kestrel
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = maxRequestLimit;
            });
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = maxRequestLimit;
                x.MultipartBodyLengthLimit = maxRequestLimit;
                x.MultipartHeadersLengthLimit = maxRequestLimit;
            });

            // Specifiying we are going to use Identity Framework
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

            }).AddEntityFrameworkStores<NGCoreJWT_DbContext>().AddDefaultTokenProviders();


            // Configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            // Authentication Middleware
            services.AddAuthentication(o =>
            {
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = appSettings.Site,
                    ValidAudience = appSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)

                };
            });

            /*
          Requirement: 
          User should be Authenticated
          User Must be Authorized.

          In Order to post video,audio and document and unlock the respective ones.
          */

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireLoggedIn", policy => policy.RequireRole("Admin", "Celebrity", "Follower").RequireAuthenticatedUser());

                options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Admin").RequireAuthenticatedUser());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            //  app.UseAuthorization();

            //  app.UseHttpsRedirection();
            app.UseCors(x => x
               .AllowAnyMethod()
               .AllowAnyHeader()
               .SetIsOriginAllowed(origin => true) // allow any origin
               .AllowCredentials()); // allow credentials
                                     // app.UseHttpsRedirection();

            //app.UseHttpsRedirection();
            app.UseAuthentication();
            // this is the request execution pipeline for client side
            app.UseMvc();

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

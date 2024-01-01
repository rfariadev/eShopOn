using System.Text;
using Identity.API.Data;
using Identity.API.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Identity.API;

public class Startup {
  private readonly IConfiguration _config;
  public Startup(IConfiguration config) {
    _config = config;
  }

  public void ConfigureServices(IServiceCollection services) {
    services.AddSwaggerGen(c => {
      c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "Enterprise Application",
        Description = "API Enterprise Application",
        Contact = new OpenApiContact() { Name = "Renato Faria", Email = "email@email.com" },
        License = new OpenApiLicense() { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
      });
    });

    var connectionString = _config.GetConnectionString("DefaultConnection");

    services.AddDbContext<AppDbContext>(o =>
      o.UseSqlServer(connectionString));

    services.AddDefaultIdentity<IdentityUser>()
      .AddRoles<IdentityRole>()
      .AddEntityFrameworkStores<AppDbContext>()
      .AddDefaultTokenProviders();

    var appSettingsSection = _config.GetSection("AppSettings");
    services.Configure<AppSettings>(appSettingsSection);

    var appSettings = appSettingsSection.Get<AppSettings>();
    var key = Encoding.ASCII.GetBytes(appSettings.Jti);

    services.AddAuthentication(o => {
      o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(jwto => {
      jwto.RequireHttpsMetadata = true;
      jwto.SaveToken = true;
      jwto.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = appSettings.Iss,
        ValidAudience = appSettings.Aud
      };
    });

    services.AddAuthorization();

    services.AddControllers();
  }

  public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
    if (env.IsDevelopment()) {
      app.UseSwagger();
      app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
      });

    }

    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseEndpoints(endpoints => {
      endpoints.MapControllers();
    });
  }
}
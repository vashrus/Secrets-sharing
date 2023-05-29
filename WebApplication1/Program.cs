using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using WebApplication1.Model;
using WebApplication1.Model;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DatabaseModel>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var appSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);
var appSettings = appSettingsSection.Get<AppSettings>();
var key = Encoding.ASCII.GetBytes(appSettings.Secret);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddControllers().AddJsonOptions(x =>
{
    x.JsonSerializerOptions.IgnoreNullValues = true;
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = ".NET Core 5 ", Version = "v1", Description = "This test description" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
                     {
                         new OpenApiSecurityScheme{
                             Reference=new OpenApiReference  {
                                    Type=ReferenceType.SecurityScheme,
                                    Id="Bearer"
                             }
                         },
                         new string[]{}

                     }

              });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "dotnetClaimAuthorization v1"));

}

app.UseSwagger(options =>
{
    options.SerializeAsV2 = true;

});

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "myApi");
    options.RoutePrefix = string.Empty;
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

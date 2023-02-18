using CancerPredictionApi.Data;
using CancerPredictionApi.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using Swashbuckle.AspNetCore.Filters;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddScoped<IAuthRepository, AuthRepository>();
    // Add services to the container.
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Description = "Standart Authorization header using the bearer Scheme",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });
        c.OperationFilter<SecurityRequirementsOperationFilter>();

    });

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                                      .AddJwtBearer(options =>
                                      {
                                          options.TokenValidationParameters = new TokenValidationParameters
                                          {
                                              ValidateIssuerSigningKey = true,
                                              IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.
                                              GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
                                              ValidateIssuer = false,
                                              ValidateAudience = false
                                          };
                                      });

    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")
            ));

    builder.Services.AddCors(options => options.AddPolicy(name: "FinalAsses",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }));

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();


    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors("FinalAsses");

    app.UseAuthentication();
    app.UseRouting();
    app.UseAuthorization();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
    app.MapControllers();

    app.Run();


}
catch(Exception ex)
{
    logger.Error(ex, "stopped because of exception");
}
finally
{
    NLog.LogManager.Shutdown();
}

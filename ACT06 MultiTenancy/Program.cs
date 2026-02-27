
using ACT06_MultiTenancy.Application.Interfaces;
using ACT06_MultiTenancy.Infrastructure.Data;
using ACT06_MultiTenancy.Infrastructure.Repositories;
using ACT06_MultiTenancy.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Serilog;
using System.Text;

namespace ACT06_MultiTenancy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((ctx, lc) =>
                lc.ReadFrom.Configuration(ctx.Configuration));

            builder.Services.AddControllers();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<ITenantProvider, JwtTenantProvider>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITokenGenerator, JwtTokenGenerator>();

            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            string? connStr;

            if (!string.IsNullOrWhiteSpace(databaseUrl))
            {
                var uri = new Uri(databaseUrl);

                var userInfo = uri.UserInfo.Split(':', 2);
                var username = userInfo[0];
                var password = userInfo.Length > 1 ? userInfo[1] : "";

                var npgsql = new NpgsqlConnectionStringBuilder
                {
                    Host = uri.Host,
                    Port = uri.Port,
                    Username = username,
                    Password = password,
                    Database = uri.AbsolutePath.TrimStart('/'),
                    SslMode = SslMode.Require,
                    Pooling = true
                };

                connStr = npgsql.ToString();
            }
            else
            {
                connStr = builder.Configuration.GetConnectionString("DefaultConnection");
            }

            builder.Services.AddDbContext<AppDbContext>(opt =>
                opt.UseNpgsql(connStr));

            //puerto para el deploy
            var port = Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrWhiteSpace(port))
            {
                builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
            }

            // JWT
            var jwtKey = builder.Configuration["JWT_Key"] ?? throw new Exception("JWT_Key no configurada");
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "Api",
                        ValidateAudience = true,
                        ValidAudience = "Api",
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ValidateLifetime = true
                    };
                });

            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseSerilogRequestLogging();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

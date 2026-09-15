using Gax.Identity;
using Gax.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Context 1 — custom RBAC (roles/rights/resources)
builder.Services.AddDbContext<GaxDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("GaxDb"),
        sql => sql.MigrationsAssembly(typeof(GaxDbContext).Assembly.FullName)));

// Context 2 — standard ASP.NET Core Identity
builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("IdentityDb"),
        sql => sql.MigrationsAssembly(typeof(AppIdentityDbContext).Assembly.FullName)));

builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<AppIdentityDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(o => o.LoginPath = "/login");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

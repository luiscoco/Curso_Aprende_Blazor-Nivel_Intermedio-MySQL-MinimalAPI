using BlazorMySQLMinimalAPI.Components;
using BlazorMySQLMinimalAPI.Data;
using BlazorMySQLMinimalAPI.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Enable detailed error reporting in Blazor circuits
builder.Services.Configure<CircuitOptions>(options =>
{
    options.DetailedErrors = true;
});

// Register HttpClient with BaseAddress using NavigationManager
builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 38))));

var app = builder.Build();

// Ensure the database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

// Define Minimal API Endpoints
app.MapGet("/api/employees", async (ApplicationDbContext db) =>
{
    return await db.Employees.ToListAsync();
});

app.MapGet("/api/employees/{id}", async (int id, ApplicationDbContext db) =>
{
    var employee = await db.Employees.FindAsync(id);
    if (employee == null) return Results.NotFound();
    return Results.Ok(employee);
});

app.MapPost("/api/employees", async (Employee employee, ApplicationDbContext db) =>
{
    db.Employees.Add(employee);
    await db.SaveChangesAsync();
    return Results.Created($"/api/employees/{employee.Id}", employee);
});

app.MapPut("/api/employees/{id}", async (int id, Employee updatedEmployee, ApplicationDbContext db) =>
{
    var employee = await db.Employees.FindAsync(id);
    if (employee == null) return Results.NotFound();

    employee.Name = updatedEmployee.Name;
    employee.Position = updatedEmployee.Position;
    employee.Salary = updatedEmployee.Salary;

    await db.SaveChangesAsync();
    return Results.Ok(employee);
});

app.MapDelete("/api/employees/{id}", async (int id, ApplicationDbContext db) =>
{
    var employee = await db.Employees.FindAsync(id);
    if (employee == null) return Results.NotFound();

    db.Employees.Remove(employee);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

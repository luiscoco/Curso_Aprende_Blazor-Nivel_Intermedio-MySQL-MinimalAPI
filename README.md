# How to create a Blazor Web Application with MinimalAPIs and connected to MySQL(Oracle database)

## 1. Download and Install MySQL (Oracle database)

For downloading MySQL navigate to this URL: 

https://dev.mysql.com/downloads/installer/

![image](https://github.com/user-attachments/assets/168e9ed5-3f84-43fd-b891-6332b53f1c8e)

Click on the Donwload button and double click on the donwloaded file

![image](https://github.com/user-attachments/assets/6814496d-b48a-4615-aba1-d01152170ad4)

Install MySQL and set the root user password

First we **create a new database** 

```
CREATE DATABASE MinimalApiDb;

USE MinimalApiDb;
```

Then we **create a new table**

```
CREATE TABLE Employees (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Position VARCHAR(100) NOT NULL,
    Salary DECIMAL(10, 2) NOT NULL
);
```

and **populate with data**

```
USE MinimalApiDb;

-- Insert Employee 1
INSERT INTO Employees (Name, Position, Salary)
VALUES ('John Doe', 'Software Engineer', 75000.00);

-- Insert Employee 2
INSERT INTO Employees (Name, Position, Salary)
VALUES ('Jane Smith', 'Project Manager', 85000.00);

-- Insert Employee 3
INSERT INTO Employees (Name, Position, Salary)
VALUES ('Alice Johnson', 'QA Engineer', 65000.00);

-- Insert Employee 4
INSERT INTO Employees (Name, Position, Salary)
VALUES ('Robert Brown', 'DevOps Engineer', 78000.00);

-- Insert Employee 5
INSERT INTO Employees (Name, Position, Salary)
VALUES ('Emily Davis', 'HR Manager', 70000.00);
```

We verify in MySQL the created database

![image](https://github.com/user-attachments/assets/6154b360-7a79-4776-83c0-4399a7dec9dd)

## 2. Create Blazor Web Application with Visual Studio 2022 Community Edition

Run Visual Studio 2022 Community Edition and create a new project

![image](https://github.com/user-attachments/assets/5bbf5e8d-f56c-441a-8333-321b95a994c6)

Select the Blazor Web App project template

![image](https://github.com/user-attachments/assets/5ce159a1-84ef-4674-9e9e-b3c34f3f729a)

Input the project name and set the project location in the hard disk

![image](https://github.com/user-attachments/assets/69f7bcb9-f58f-4745-bd5d-9d3d003d7675)

Leave the default values and be sure to select the .NET 9 framework and press the create button

![image](https://github.com/user-attachments/assets/9cb8b435-2fe3-4be6-9d48-9f1c57560c46)

## 3. Add Nuget packages

Microsoft.EntityFrameworkCore.Design

Microsoft.EntityFrameworkCore.Tools

Pomelo.EntityFrameworkCore.MySql

![image](https://github.com/user-attachments/assets/7262d050-4d3e-4e85-9d55-74bd37885786)

## 4. Add bootstrap 5 in App.razor

Add these two lines for adding bootstrap 5:

```
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/css/bootstrap.min.css" rel="stylesheet" />
<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons/font/bootstrap-icons.css" rel="stylesheet" />
```

This is the modified App.razor file:

**App.razor**

```razor
<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons/font/bootstrap-icons.css" rel="stylesheet" />
    <link rel="stylesheet" href="@Assets["app.css"]" />
    <link rel="stylesheet" href="@Assets["BlazorMySQLMinimalAPI.styles.css"]" />
    <ImportMap />
    <link rel="icon" type="image/png" href="favicon.png" />
    <HeadOutlet @rendermode="InteractiveServer" />
</head>

<body>
    <Routes @rendermode="InteractiveServer" />
    <script src="_framework/blazor.web.js"></script>
</body>

</html>
```

## 5. Create Data and Models folders

![image](https://github.com/user-attachments/assets/e52950f3-062a-4d8f-a358-1d2c3cd33bb4)

## 6. Create Employee data model

**Employee.cs**

```csharp
namespace BlazorMySQLMinimalAPI.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public decimal Salary { get; set; }
    }
}
```

## 7. Create ApplicationDbContext

This code defines the database context for Entity Framework Core, mapping the Employee entity to the Employees table in the MySQL database

The ApplicationDbContext class is used to interact with the database, perform CRUD operations, and manage database configurations

**ApplicationDbContext.cs**

```csharp
using BlazorMySQLMinimalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorMySQLMinimalAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
    }
}
```

## 8. Modify middleware(Program.cs)

We include these two functionalities:

The **HttpClient** is registered so that it can be injected into Blazor components to make HTTP requests

By setting the BaseAddress, you ensure that all outgoing requests automatically target the correct API or server location

```csharp
builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});
```

And also this setup **configures the Entity Framework Core** to work with a **MySQL database**, allowing the application to interact with the database (for example, through CRUD operations) using the ApplicationDbContext

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 38))));
```

We ensure the **database is created**:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}
```

We also define the **Minimal APIs endpoints** with this code:

```csharp
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
```

See the whole code:

**Program.cs**

```csharp
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
```

## 9. Add Employees.razor component

**Employees.razor**

```razor
@page "/employees"
@using BlazorMySQLMinimalAPI.Models
@inject HttpClient Http

<div class="container mt-4">
    <h3 class="text-center text-primary">Employee Management</h3>

    <!-- Button to Refresh Employee List -->
    <div class="d-flex justify-content-end mb-3">
        <button class="btn btn-primary" @onclick="LoadEmployees">
            <i class="bi bi-arrow-clockwise"></i> Refresh
        </button>
    </div>

    <!-- Employee List -->
    <ul class="list-group mb-4">
        @foreach (var employee in employees)
        {
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <div>
                    <strong class="text-success">@employee.Name</strong>
                    <span class="text-muted">- @employee.Position</span>
                    <span class="badge bg-info text-dark">Salary: $@employee.Salary</span>
                </div>
                <div>
                    <button class="btn btn-sm btn-warning me-2" @onclick="() => EditEmployee(employee.Id)">
                        <i class="bi bi-pencil"></i> Edit
                    </button>
                    <button class="btn btn-sm btn-danger" @onclick="() => DeleteEmployee(employee.Id)">
                        <i class="bi bi-trash"></i> Delete
                    </button>
                </div>
            </li>
        }
    </ul>

    <!-- Form to Add or Edit Employee -->
    <div class="card">
        <div class="card-header bg-primary text-white">
            <h4 class="mb-0">@(isEditing ? "Edit Employee" : "Add Employee")</h4>
        </div>
        <div class="card-body">
            <EditForm Model="currentEmployee" OnValidSubmit="SaveEmployee" class="row g-3">
                <div class="col-md-4">
                    <label for="name" class="form-label">Name</label>
                    <InputText id="name" class="form-control" @bind-Value="currentEmployee.Name" />
                </div>
                <div class="col-md-4">
                    <label for="position" class="form-label">Position</label>
                    <InputText id="position" class="form-control" @bind-Value="currentEmployee.Position" />
                </div>
                <div class="col-md-4">
                    <label for="salary" class="form-label">Salary</label>
                    <InputNumber id="salary" class="form-control" @bind-Value="currentEmployee.Salary" />
                </div>
                <div class="col-12 d-flex justify-content-end">
                    <button type="submit" class="btn btn-success">
                        <i class="bi bi-save"></i> Save
                    </button>
                </div>
            </EditForm>
        </div>
    </div>
</div>

@code {
    private List<Employee> employees = new();
    private Employee currentEmployee = new();
    private bool isEditing = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadEmployees();
    }

    private async Task LoadEmployees()
    {
        employees = await Http.GetFromJsonAsync<List<Employee>>("api/employees");
    }

    private async Task SaveEmployee()
    {
        if (isEditing)
        {
            await Http.PutAsJsonAsync($"api/employees/{currentEmployee.Id}", currentEmployee);
        }
        else
        {
            await Http.PostAsJsonAsync("api/employees", currentEmployee);
        }

        await LoadEmployees();
        currentEmployee = new Employee();
        isEditing = false;
    }

    private async Task EditEmployee(int id)
    {
        currentEmployee = await Http.GetFromJsonAsync<Employee>($"api/employees/{id}");
        isEditing = true;
    }

    private async Task DeleteEmployee(int id)
    {
        await Http.DeleteAsync($"api/employees/{id}");
        await LoadEmployees();
    }
}
```

## 10. Modify appsettings.json to include the database string connection

In the appsettings.json file we include the connection string for MySQL database:

**appsettings.json**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MinimalApiDb;User=root;Password=4PAB75Lbc15@;"
  },
  "AllowedHosts": "*"
}
```

## 11. Run the application and view the results

![image](https://github.com/user-attachments/assets/c5ac97b1-46ab-4481-9682-c0b7dcd8c129)








Mini tutorial pra criar um mvc simples;

## Primeitamente, vc vai digitar no cmd
```
dotnet new mvc -n NOMEDOPROJETO
```

## Segundo, vc vai adicionar os frameworks necessários, e o que vc vai usar para o banco de dados
```
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package NpgSql.EntityFrameworkCore.PostgreSQL
```

## Terceiro, vc vai criar o seu arquivo Models, onde vai declarar quais irão ser a colunas do banco, juntamente com o nome da tabela,
Logo,  `Models\Cars.cs`, nome da tabela vai ser Cars.
```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace crud_dotnet.Models
{
    public class Car
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string email {get; set; }
    }
}
```

## Em quarto lugar, vamos criar o contexto de conexão para o banco;
`Models\ApplicationDbContext.cs`
```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace crud_dotnet.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)   
            : base(options) {}

        public DbSet<Car> Cars { get; set; }
    }
}
```

## Logo após, vamos configurar o `appsettings.json`
```
"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=crud;Username=postgres;Password=root"
  },
```

## Agora, vamos linkar o banco de dados no `Program.cs`
```
using crud_dotnet.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// conexao com o banco de dados 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cars}/{action=Index}/{id?}");

app.Run();
```

## Vamos rodar o comando no cmd para gerar o banco de dados e atualizar:
```
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Agora, criaremos o controlador `Controllers\CarsController.cs`
```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using crud_dotnet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace crud_dotnet.Controllers
{
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.Cars.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Car car)
        {
            if (ModelState.IsValid)
            {
                _context.Add(car);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(car);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            return View(car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,name,email")] Car car)
        {
            if (id != car.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(car);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(car);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.Id == id);
        }
    }
}
```

## Agora, vamos criar o frontend. E pra isso, vamos precisar criar a pasta "Cars" na Views.
`Views\Cars` e dentro da pasta Cars, teremos:
`Cars\Index.cshtml`
`Cars\Create.cshtml`
`Cars\Edit.cshtml`
`Cars\Delete.cshtml`

## Index.cshtml
```
@model IEnumerable<crud_dotnet.Models.Car>

@{
ViewData["Title"] = "Car List";
}
<style>
  table{
    text-align: center;
  }
</style>

<div class="container mt-4">
    <h2>Car List</h2>

    <p>
        <a class="btn btn-primary" href="@Url.Action("Create")">Criar novo</a>
    </p>

    <table class="table table-striped">
        <thead class="thead-dark">
        <tr>
            <th>ID</th>
            <th>Name</th>
            <th>Email</th>
            <th>Ações</th>
        </tr>
        </thead>
        <tbody>
        @foreach (var car in Model)
        {
        <tr>
            <td>@car.Id</td>
            <td>@car.Name</td>
            <td>@car.email</td>
            <td>
                <a class="btn btn-warning btn-sm" href="@Url.Action("Edit", new { id = car.Id })">Editar</a>
                <a class="btn btn-danger btn-sm" href="@Url.Action("Delete", new { id = car.Id })" onclick="return confirm('Tem certeza que deseja excluir?');">Excluir</a>
            </td>
        </tr>
        }
        </tbody>
    </table>
</div>
```

## Edit.cshtml
```
@model crud_dotnet.Models.Car

@{
    ViewData["Title"] = "Edit";
}

<h2>Edit</h2>

<form asp-action="Edit">
    <div class="form-group">
        <label asp-for="Name" class="control-label"></label>
        <input asp-for="Name" class="form-control" />
        <span asp-validation-for="Name" class="text-danger"></span>
    </div>
    <div class="form-group">
        <label asp-for="email" class="control-label"></label>
        <input asp-for="email" class="form-control" />
        <span asp-validation-for="email" class="text-danger"></span>
    </div>
    <div class="form-group" style="margin-top: 1rem;>
        <input type="submit" value="Save" class="btn btn-primary" />
        <a asp-action="Index" class="btn btn-secondary">Back to List</a>
    </div>
</form>
```

## Delete.cshtml
```
@model crud_dotnet.Models.Car

@{
    ViewData["Title"] = "Delete";
}

<h2>Delete</h2>

<h3>Are you sure you want to delete this?</h3>

<div>
    <h4>Car</h4>
    <div>
        <dl class="row">
            <dt class="col-sm-2">
                Name
            </dt>
            <dd class="col-sm-10">
                @Model.Name
            </dd>
            <dt class="col-sm-2">
                Email
            </dt>
            <dd class="col-sm-10">
                @Model.email
            </dd>
        </dl>
    </div>
    <form asp-action="Delete" method="post">
        <input type="hidden" asp-for="Id" />
        <div class="form-group">
            <input type="submit" value="Delete" class="btn btn-danger" />
            <a asp-action="Index" class="btn btn-secondary">Back to List</a>
        </div>
    </form>
</div>
```

## Create.cshtml
```
@model crud_dotnet.Models.Car

@{
ViewData["Title"] = "Create/Edit";
}

<h2>@ViewData["Title"]</h2>

<form asp-action="Create" method="post">
    <div class="form-group">
        <label asp-for="Name" class="control-label"></label>
        <input asp-for="Name" class="form-control" />
        <span asp-validation-for="Name" class="text-danger"></span>
    </div>
    <div class="form-group">
        <label asp-for="email" class="control-label"></label>
        <input asp-for="email" class="form-control" />
        <span asp-validation-for="email" class="text-danger"></span>
    </div>
    <div class="form-group" style="margin-top: 1rem;">
        <input type="submit" value="Save" class="btn btn-primary" />
        <a asp-action="Index" class="btn btn-secondary">Back to List</a>
    </div>
</form>
```

## Após seguir todos os passos corretamente, é só digitar no cmd, e verificar se a aplicação está funcionando
```
dotnet run
```



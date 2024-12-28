using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

var builder = WebApplication.CreateBuilder(args);

// Définition de la chaîne de connexion MySQL
builder.Services.AddDbContext<ResultContext>(opt => 
    opt.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), 
    new MySqlServerVersion(new Version(8, 0, 30))));
builder.Services.AddControllers();

var app = builder.Build();

// Vérifie si la base de données existe, sinon elle est créée
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ResultContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Définition de la table "resultats"
[Table("processeddata")]
public class Result
{
    public int id { get; set; }
    public int nombre { get; set; }
    public bool pair { get; set; }
    public bool premier { get; set; }
    public bool parfait { get; set; }
}

// Contexte de la base de données
public class ResultContext : DbContext
{
    public ResultContext(DbContextOptions<ResultContext> options) : base(options) { }
    public DbSet<Result> Results { get; set; }
}

// Contrôleur pour les résultats (API)
[ApiController]
[Route("api")]
public class ResultsController : ControllerBase
{
    private readonly ResultContext _context;

    public ResultsController(ResultContext context)
    {
        _context = context;
    }

    // Vérifier si un nombre existe dans la base de données
    [HttpGet("check")]
    public IActionResult CheckResult(int nombre)
    {
        var exists = _context.Results.Any(r => r.nombre == nombre);
        return Ok(new { exists });
    }

    // Enregistrer un résultat dans la base de données
    [HttpPost("ajout")]
    public IActionResult StoreResult([FromBody] Result donnees)
    {
        _context.Results.Add(donnees);
        _context.SaveChanges();
        return Ok(donnees);
    }

    // Récupérer un résultat de la base de données
    [HttpGet("recuperer")]
    public IActionResult GetResult(int nombre)
    {
        var donnees = _context.Results.FirstOrDefault(r => r.nombre == nombre);
        return Ok(donnees);
    }

    // Supprimer un résultat de la base de données
    [HttpDelete("supprimer")]
    public IActionResult DeleteResult(int nombre)
    {
        var donnees = _context.Results.FirstOrDefault(r => r.nombre == nombre);
        _context.Results.Remove(donnees);
        _context.SaveChanges();
        return Ok(donnees);
    }

    // Récupérer tous les résultats de la base de données
    [HttpGet("all")]
    public IActionResult GetAllResults()
    {
        var results = _context.Results.ToList();
        return Ok(results);
    }
}

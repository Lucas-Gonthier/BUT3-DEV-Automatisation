using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

var builder = WebApplication.CreateBuilder(args);

var dbConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

// Définition de la chaîne de connexion MySQL
builder.Services.AddDbContext<ResultContext>(opt => 
    opt.UseMySql(dbConnectionString, // Utiliser dbConnectionString ici
    new MySqlServerVersion(new Version(8, 0, 21)),
    mySqlOptions => mySqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(10),
        errorNumbersToAdd: null)));

// Configuration CORS : Permettre toutes les origines, méthodes et en-têtes
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // Accepter toutes les origines
              .AllowAnyMethod()   // Accepter toutes les méthodes HTTP
              .AllowAnyHeader();  // Accepter tous les en-têtes
    });
});

builder.Services.AddControllers();

// Définition du service Minio
builder.Services.AddSingleton<MinioService>();

var app = builder.Build();
app.UseCors("AllowAll");

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

// Service Minio pour la gestion des fichiers
public class SyracuseData
{
    public int Nombre { get; set; }
    public List<int> Suite { get; set; } = new List<int>();
}

// Contrôleur pour les résultats (API)
[ApiController]
[Route("api")]
public class ResultsController : ControllerBase
{
    private readonly ResultContext _context;
    private readonly MinioService _minioService;

    public ResultsController(ResultContext context, MinioService minioService)
    {
        _context = context;
        _minioService = minioService;
    }

    // Vérifier si un nombre existe dans la base de données
    [HttpGet("check")]
    public IActionResult CheckResult(int nombre)
    {
        try
        {
            var exists = _context.Results.Any(r => r.nombre == nombre);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
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

    // Enregistrer la suite de Syracuse dans MinIO
    [HttpPost("envoyer-syracuse")]
    public async Task<IActionResult> StoreSyracuse([FromBody] SyracuseData syracuseData)
    {
        var syracuseJson = System.Text.Json.JsonSerializer.Serialize(syracuseData.Suite);
        await _minioService.UploadFileAsync($"syracuse_{syracuseData.Nombre}.json", syracuseJson);

        return Ok();
    }

    // Télécharger la suite de Syracuse depuis MinIO
    [HttpGet("download-syracuse")]
    public async Task<IActionResult> DownloadSyracuse(int nombre)
    {
        var objectName = $"syracuse_{nombre}.json";
        var stream = await _minioService.GetFileAsync(objectName);

        if (stream == null)
        {
            return NotFound();
        }

        return File(stream, "application/json", objectName);
    }
}

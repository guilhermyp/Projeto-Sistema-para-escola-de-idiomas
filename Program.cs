using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEscolaDeIdiomas.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>();
var app = builder.Build();

app.MapGet("/materias", async ([FromServices] AppDbContext db) =>
{
    return await db.Materias.ToListAsync();
});

app.MapPost("/alunos", async ([FromServices] AppDbContext db, [FromBody] Aluno novoAluno) =>
{
    
    var alunoExistente = await db.Alunos.FindAsync(novoAluno.Matricula);
    if (alunoExistente != null)
    {
        return Results.BadRequest("Aluno já existe.");
    }

   
    db.Alunos.Add(novoAluno);

   
    await db.SaveChangesAsync();

    
    return Results.Created($"/alunos/{novoAluno.Matricula}", novoAluno);
});

app.MapDelete("/alunos/{matricula:int}", async ([FromServices] AppDbContext db, int matricula) =>
{
    var aluno = await db.Alunos.FindAsync(matricula);
    if (aluno is null)
    {
        return Results.NotFound();
    }

    db.Alunos.Remove(aluno); 
    await db.SaveChangesAsync(); 

    return Results.NoContent(); 
});

app.MapGet("/alunos", async ([FromServices] AppDbContext db) =>
{
    return await db.Alunos.ToListAsync(); // Retorna a lista de alunos
});


app.Run();

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEscolaDeIdiomas.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
});
var app = builder.Build();

//materia
app.MapGet("/materias/listar", async ([FromServices] AppDbContext db) => await db.Materias.ToListAsync());

app.MapPost("/materias/adicionar-materia", async ([FromServices] AppDbContext db, [FromBody] Materia novaMateria) =>
{
    if (string.IsNullOrWhiteSpace(novaMateria.Nome))
    {
        return Results.BadRequest("O nome da matéria é obrigatório;");
    }

    db.Materias.Add(novaMateria);
    await db.SaveChangesAsync();

    return Results.Created($"/materias/{novaMateria.Id}", novaMateria);
});

//alunos
app.MapGet("/alunos/listar", async ([FromServices] AppDbContext db) => await db.Alunos.ToListAsync());

app.MapGet("/alunos/{matricula:int}", async ([FromServices] AppDbContext db, int matricula) =>
{
    var aluno = await db.Alunos
        .Include(a => a.AlunoMaterias)
        .ThenInclude(am => am.Materia)
        .FirstOrDefaultAsync(a => a.Matricula == matricula);

    if (aluno == null)
    {
        return Results.NotFound();
    }

    var alunoDto = new AlunoDto
    {
        Matricula = aluno.Matricula,
        Nome = aluno.Nome,
        Materias = aluno.AlunoMaterias
            .Select(am => new MateriaDto
            {
                Id = am.MateriaId,
                Nome = am.Materia.Nome
            })
            .ToList()
    };

    return Results.Ok(alunoDto);
});

app.MapPost("/alunos/adicionar-aluno", async ([FromServices] AppDbContext db, [FromBody] Aluno novoAluno) =>
{
    if (string.IsNullOrWhiteSpace(novoAluno.Nome))
    {
        return Results.BadRequest("O nome do aluno é obrigatório.");
    }
    
    db.Alunos.Add(novoAluno);
    await db.SaveChangesAsync();

    return Results.Created($"/alunos/{novoAluno.Matricula}", novoAluno);
});

app.MapDelete("/alunos/excluir-aluno/{matricula:int}", async ([FromServices] AppDbContext db, int matricula) =>
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

app.MapPost("/alunos/{matricula:int}/inscrever/{materiaId:int}", async ([FromServices] AppDbContext db, int matricula, int materiaId) =>
{
    try
    {
        var aluno = await db.Alunos.FindAsync(matricula);
        var materia = await db.Materias.FindAsync(materiaId);

        if (aluno is null || materia is null)
        {
            return Results.NotFound("Aluno ou matéria não encontrada.");
        }

        var alunoMateria = new AlunoMateria { AlunoId = matricula, MateriaId = materiaId };

        var jaInscrito = await db.AlunoMaterias
            .AnyAsync(am => am.AlunoId == matricula && am.MateriaId == materiaId);

        if (jaInscrito)
        {
            return Results.BadRequest("O aluno já está inscrito nessa matéria.");
        }

        db.AlunoMaterias.Add(alunoMateria);
        await db.SaveChangesAsync();

        return Results.Created($"/alunos/{matricula}/materias/{materiaId}", alunoMateria);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro: {ex.Message}");
        return Results.Problem("Ocorreu um erro ao processar a solicitação.");
    }
});

app.MapPost("/alunos/{matricula:int}/atribuir-nota/{materiaId:int}", async ([FromServices] AppDbContext db, int matricula, int materiaId, [FromBody] AtribuirNotaRequest request) =>
{
    if (request.Nota is < 0 or > 10)
    {
        return Results.BadRequest("A nota deve estar entre 0 e 10.");
    }

    var alunoMateria = await db.AlunoMaterias
        .FirstOrDefaultAsync(am => am.AlunoId == matricula && am.MateriaId == materiaId);

    if (alunoMateria is null)
    {
        return Results.NotFound("Aluno ou matéria não encontrados, ou aluno não inscrito.");
    }

    alunoMateria.Nota = request.Nota;
    await db.SaveChangesAsync();

    return Results.Ok(alunoMateria);
});


app.MapGet("/alunos/{matricula:int}/boletim", async ([FromServices] AppDbContext db, int matricula) =>
{
    var aluno = await db.Alunos
        .Include(a => a.AlunoMaterias)
        .ThenInclude(am => am.Materia)
        .FirstOrDefaultAsync(a => a.Matricula == matricula);

    if (aluno == null)
    {
        return Results.NotFound("Aluno não encontrado.");
    }

    var boletim = new
    {
        NomeAluno = aluno.Nome,
        Materias = aluno.AlunoMaterias
            .Select(am => new
            {
                Materia = am.Materia.Nome, am.Nota
            })
            .ToList()
    };

    return Results.Ok(boletim);
});


app.Run();
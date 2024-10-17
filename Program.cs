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

// Professores adicionar
app.MapPost("/professores/adicionar", async ([FromServices] AppDbContext db, [FromBody] ProfessorDto professorDto) =>
{
    if (string.IsNullOrWhiteSpace(professorDto.Nome))
    {
        return Results.BadRequest("O nome do professor é obrigatório.");
    }

    var professor = new Professor { Nome = professorDto.Nome };
    db.Professores.Add(professor);
    await db.SaveChangesAsync();

    return Results.Created($"/professores/{professor.Id}", professor);
});

app.MapGet("/professores/listar", async ([FromServices] AppDbContext db) =>
    await db.Professores.ToListAsync()
);

// Vincular professor a matéria
app.MapPut("/materias/{materiaId:int}/vincular-professor/{professorId:int}", async ([FromServices] AppDbContext db, int materiaId, int professorId) =>
{
    var materia = await db.Materias.FindAsync(materiaId);
    if (materia is null) return Results.NotFound("Matéria não encontrada.");

    var professor = await db.Professores.FindAsync(professorId);
    if (professor is null) return Results.NotFound("Professor não encontrado.");

    materia.ProfessorId = professorId;
    await db.SaveChangesAsync();

    return Results.Ok(materia);
});

// Materias listar
app.MapGet("/materias/listar", async ([FromServices] AppDbContext db) =>
    await db.Materias
        .Include(m => m.Professor)
        .Select(m => new
        {
            m.Id,
            m.Nome,
            Professor = m.Professor != null ? m.Professor.Nome : "Sem professor"
        })
        .ToListAsync()
);

//adicionar materia
app.MapPost("/materias/adicionar-materia", async ([FromServices] AppDbContext db, [FromBody] Materia novaMateria) =>
{
    if (string.IsNullOrWhiteSpace(novaMateria.Nome))
    {
        return Results.BadRequest("O nome da matéria é obrigatório.");
    }

    db.Materias.Add(novaMateria);
    await db.SaveChangesAsync();

    return Results.Created($"/materias/{novaMateria.Id}", novaMateria);
});

// Alunos listar
app.MapGet("/alunos/listar", async ([FromServices] AppDbContext db) => await db.Alunos.ToListAsync());

app.MapGet("/alunos/{matricula:int}", async ([FromServices] AppDbContext db, int matricula) =>
{
    var aluno = await db.Alunos
        .Include(a => a.AlunoMaterias)
        .ThenInclude(am => am.Materia)
        .FirstOrDefaultAsync(a => a.Matricula == matricula);

    if (aluno == null) return Results.NotFound();

    var alunoDto = new AlunoDto
    {
        Matricula = aluno.Matricula,
        Nome = aluno.Nome,
        Materias = aluno.AlunoMaterias.Select(am => new MateriaDto
        {
            Id = am.MateriaId,
            Nome = am.Materia.Nome
        }).ToList()
    };

    return Results.Ok(alunoDto);
});

//adicionar aluno
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

//excluir aluno
app.MapDelete("/alunos/excluir-aluno/{matricula:int}", async ([FromServices] AppDbContext db, int matricula) =>
{
    var aluno = await db.Alunos.FindAsync(matricula);
    if (aluno is null) return Results.NotFound();

    db.Alunos.Remove(aluno);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

//inscrição
app.MapPost("/alunos/{matricula:int}/inscrever/{materiaId:int}", async ([FromServices] AppDbContext db, int matricula, int materiaId) =>
{
    var aluno = await db.Alunos.FindAsync(matricula);
    var materia = await db.Materias.FindAsync(materiaId);

    if (aluno is null || materia is null) return Results.NotFound("Aluno ou matéria não encontrada.");

    if (await db.AlunoMaterias.AnyAsync(am => am.AlunoId == matricula && am.MateriaId == materiaId))
    {
        return Results.BadRequest("O aluno já está inscrito nessa matéria.");
    }

    var alunoMateria = new AlunoMateria { AlunoId = matricula, MateriaId = materiaId };
    db.AlunoMaterias.Add(alunoMateria);
    await db.SaveChangesAsync();

    return Results.Created($"/alunos/{matricula}/materias/{materiaId}", alunoMateria);
});

//atribuição de nota
app.MapPost("/alunos/{matricula:int}/atribuir-nota/{materiaId:int}", async ([FromServices] AppDbContext db, int matricula, int materiaId, [FromBody] AtribuirNotaRequest request) =>
{
    if (request.Nota is < 0 or > 10) return Results.BadRequest("A nota deve estar entre 0 e 10.");

    var alunoMateria = await db.AlunoMaterias.FirstOrDefaultAsync(am => am.AlunoId == matricula && am.MateriaId == materiaId);
    if (alunoMateria is null) return Results.NotFound("Aluno ou matéria não encontrados, ou aluno não inscrito.");

    alunoMateria.Nota = request.Nota;
    await db.SaveChangesAsync();

    return Results.Ok(alunoMateria);
});

//boletim
app.MapGet("/alunos/{matricula:int}/boletim", async ([FromServices] AppDbContext db, int matricula) =>
{
    var aluno = await db.Alunos
        .Include(a => a.AlunoMaterias)
        .ThenInclude(am => am.Materia)
        .FirstOrDefaultAsync(a => a.Matricula == matricula);

    if (aluno == null) return Results.NotFound("Aluno não encontrado.");

    var boletim = new
    {
        NomeAluno = aluno.Nome,
        Materias = aluno.AlunoMaterias.Select(am => new { am.Materia.Nome, am.Nota }).ToList()
    };

    return Results.Ok(boletim);
});

app.Run();

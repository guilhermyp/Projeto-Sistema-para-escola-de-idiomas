using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEscolaDeIdiomas.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// autenticação com JWT
var key = "chave-32-caracteres-seguranca12345"; //Chave de 32 caracteres

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "seu_sistema",
            ValidAudience = "seu_sistema",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

// Configuração de autorização com políticas de "Permissão"
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireClaim("Permissao", "Admin"));
    options.AddPolicy("Visualizar", policy => policy.RequireClaim("Permissao", "Professor", "Aluno"));
    options.AddPolicy("AdminOrVisualizar", policy =>
        policy.RequireClaim("Permissao", "Admin", "Professor", "Aluno"));
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
});

var app = builder.Build();

// Habilita autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/auth/login", async ([FromBody] LoginRequest request, [FromServices] AppDbContext db) =>
{
    // Login Admin em formato estatico
    if (request.Username == "admin" && request.Password == "admin")
    {
        var token = GenerateJwtToken(new User
        {
            Username = "admin",
            Password = "admin",
            Permissao = "Admin"
        });
        return Results.Ok(new { Token = token });
    }

    // Login para Professor dinamico
    var professor = await db.Professores
        .FirstOrDefaultAsync(p => p.Username == request.Username && p.Password == request.Password);

    if (professor != null)
    {
        var token = GenerateJwtToken(new User
        {
            Username = professor.Username,
            Password = professor.Password,
            Permissao = "Professor",
            ProfessorId = professor.Id
        });

        return Results.Ok(new { Token = token });
    }

    return Results.Unauthorized();
});

string GenerateJwtToken(User user)
{
    var claims = new List<System.Security.Claims.Claim>
    {
        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Username),
        new System.Security.Claims.Claim("Permissao", user.Permissao)
    };

    if (user.ProfessorId.HasValue)
    {
        claims.Add(new System.Security.Claims.Claim("ProfessorId", user.ProfessorId.Value.ToString()));
    }

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("chave-32-caracteres-seguranca12345"));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: "seu_sistema",
        audience: "seu_sistema",
        claims: claims,
        expires: DateTime.Now.AddHours(1),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

//Adicionar professor (Admin)
app.MapPost("/professores/adicionar", [Authorize(Policy = "Admin")] async ([FromServices] AppDbContext db, [FromBody] ProfessorDto professorDto) =>
{
    if (string.IsNullOrWhiteSpace(professorDto.Nome) || string.IsNullOrWhiteSpace(professorDto.Username) || string.IsNullOrWhiteSpace(professorDto.Password))
    {
        return Results.BadRequest("Nome, username e password são obrigatórios.");
    }

    var professor = new Professor
    {
        Nome = professorDto.Nome,
        Username = professorDto.Username,
        Password = professorDto.Password
    };

    db.Professores.Add(professor);
    await db.SaveChangesAsync();

    return Results.Created($"/professores/{professor.Id}", professor);
});

// Alterar professor
app.MapPut("/professores/alterar/{id:int}", [Authorize(Policy = "Admin")] 
    async (AppDbContext db, int id, ProfessorDto professorDto) =>
{
    var professor = await db.Professores.FindAsync(id);
    if (professor is null)
    {
        return Results.NotFound("Professor não encontrado.");
    }

    professor.Nome = professorDto.Nome;
    await db.SaveChangesAsync();

    return Results.Ok(professor);
});

// Excluir professor (apenas Admin)
app.MapDelete("/professores/excluir/{id:int}", [Authorize(Policy = "Admin")] async ([FromServices] AppDbContext db, int id) =>
{
    var professor = await db.Professores.FindAsync(id);
    if (professor is null)
    {
        return Results.NotFound($"Professor com ID {id} não encontrado.");
    }

    db.Professores.Remove(professor);
    await db.SaveChangesAsync();

    return Results.Ok($"Professor com ID {id} foi excluído.");
});

// Adicionar matéria (Admin)
app.MapPost("/materias/adicionar-materia", [Authorize(Policy = "Admin")] async ([FromServices] AppDbContext db, [FromBody] Materia novaMateria) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(novaMateria.Nome))
        {
            return Results.BadRequest("O nome da matéria é obrigatório.");
        }

        db.Materias.Add(novaMateria);
        await db.SaveChangesAsync();

        return Results.Ok($"Matéria com ID {novaMateria.Id} foi adicionada.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro: {ex.Message}");
        return Results.Problem("Erro interno ao adicionar matéria.");
    }
});

// Listar Professores (Admin, Professor e Aluno)
app.MapGet("/professores/listar", [Authorize(Policy = "AdminOrVisualizar")] async ([FromServices] AppDbContext db) =>
    await db.Professores.ToListAsync()
);

// Listar Matérias - (Admin, Professor e Aluno)
app.MapGet("/materias/listar", [Authorize(Policy = "AdminOrVisualizar")] async ([FromServices] AppDbContext db) =>
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


app.MapGet("/alunos/listar", [Authorize(Policy = "AdminOrVisualizar")] 
    async ([FromServices] AppDbContext db) =>
{
    return await db.Alunos.ToListAsync();
});

// Adicionar aluno (Admin)
app.MapPost("/alunos/adicionar", [Authorize(Policy = "Admin")] async ([FromServices] AppDbContext db, [FromBody] Aluno novoAluno) =>
{
    if (string.IsNullOrWhiteSpace(novoAluno.Nome))
    {
        return Results.BadRequest("O nome do aluno é obrigatório.");
    }

    db.Alunos.Add(novoAluno);
    await db.SaveChangesAsync();

    return Results.Created($"/alunos/{novoAluno.Matricula}", novoAluno);
});

app.MapDelete("/alunos/excluir-aluno/{matricula:int}", [Authorize(Policy = "Admin")] 
    async ([FromServices] AppDbContext db, int matricula) =>
{
    var aluno = await db.Alunos.FindAsync(matricula);
    if (aluno is null)
    {
        return Results.NotFound($"Aluno com matrícula {matricula} não encontrado.");
    }

    db.Alunos.Remove(aluno);
    await db.SaveChangesAsync();

    return Results.Ok($"Aluno com matrícula {matricula} foi excluído com sucesso.");
});


// Vincular professor a matéria (Admin)
app.MapPut("/materias/{materiaId:int}/vincular-professor/{professorId:int}", 
    [Authorize(Policy = "Admin")] async ([FromServices] AppDbContext db, int materiaId, int professorId) =>
    {
        var materia = await db.Materias.FindAsync(materiaId);
        if (materia is null) return Results.NotFound($"Matéria com ID {materiaId} não encontrada.");

        var professor = await db.Professores.FindAsync(professorId);
        if (professor is null) return Results.NotFound($"Professor com ID {professorId} não encontrado.");

        materia.ProfessorId = professorId;
        await db.SaveChangesAsync();

        return Results.Ok($"Professor com ID {professorId} foi vinculado à matéria com ID {materiaId}.");
    });


// Inscrever aluno na matéria (Admin)
app.MapPost("/alunos/{matricula:int}/inscrever/{materiaId:int}", [Authorize(Policy = "Admin")] 
    async ([FromServices] AppDbContext db, int matricula, int materiaId) =>
{
  
    var aluno = await db.Alunos.FindAsync(matricula);
    if (aluno is null) 
        return Results.NotFound($"Aluno com matrícula {matricula} não encontrado.");

    
    var materia = await db.Materias.FindAsync(materiaId);
    if (materia is null) 
        return Results.NotFound($"Matéria com ID {materiaId} não encontrada.");

    //Para verificar se o aluno já está inscrito na matéria
    var jaInscrito = await db.AlunoMaterias
        .AnyAsync(am => am.AlunoId == matricula && am.MateriaId == materiaId);

    if (jaInscrito)
    {
        return Results.BadRequest($"O aluno com matrícula {matricula} já está inscrito na matéria {materiaId}.");
    }

    // Criar o vínculo entre o aluno e a matéria
    var alunoMateria = new AlunoMateria { AlunoId = matricula, MateriaId = materiaId };
    db.AlunoMaterias.Add(alunoMateria);
    await db.SaveChangesAsync();

    return Results.Created($"/alunos/{matricula}/materias/{materiaId}", 
        $"Aluno {matricula} inscrito na matéria {materiaId} com sucesso.");
});

// Atribuir nota para o aluno em uma matéria (Admin)
app.MapPost("/alunos/{matricula:int}/atribuir-nota/{materiaId:int}", 
    [Authorize(Policy = "AdminOrVisualizar")] async ([FromServices] AppDbContext db, int matricula, int materiaId, [FromBody] AtribuirNotaRequest request) =>
{
    if (request.Nota < 0 || request.Nota > 10)
    {
        return Results.BadRequest("A nota deve estar entre 0 e 10.");
    }

    var alunoMateria = await db.AlunoMaterias
        .FirstOrDefaultAsync(am => am.AlunoId == matricula && am.MateriaId == materiaId);

    if (alunoMateria is null)
    {
        return Results.NotFound($"Aluno {matricula} ou matéria {materiaId} não encontrados, ou aluno não está inscrito.");
    }

    alunoMateria.Nota = request.Nota;
    await db.SaveChangesAsync();

    return Results.Ok($"Nota {request.Nota} atribuída ao aluno {matricula} na matéria {materiaId}.");
});


// Gerar boletim do aluno (Admin, Professor, ou Aluno)
app.MapGet("/alunos/{matricula:int}/boletim", [Authorize(Policy = "AdminOrVisualizar")]
    async ([FromServices] AppDbContext db, int matricula) =>
{
    var aluno = await db.Alunos
        .Include(a => a.AlunoMaterias)
        .ThenInclude(am => am.Materia)
        .FirstOrDefaultAsync(a => a.Matricula == matricula);

    if (aluno == null)
    {
        return Results.NotFound($"Aluno com matrícula {matricula} não encontrado.");
    }

    //Boletim com nome do aluno, matérias e notas
    var boletim = new
    {
        NomeAluno = aluno.Nome,
        Materias = aluno.AlunoMaterias.Select(am => new
        {
            Materia = am.Materia.Nome,
            Nota = am.Nota.HasValue ? am.Nota.Value.ToString("0.0") : "Sem nota"
        }).ToList()
    };

    return Results.Ok(boletim);
});


app.Run();

// ClASSES
public class User
{
    public string Username { get; set; }  = string.Empty;
    public string Password { get; set; }  = string.Empty;
    public string Permissao { get; set; }  = string.Empty;
    public int? ProfessorId { get; set; } 
}

public class LoginRequest
{
    public string Username { get; set; }  = string.Empty;
    public string Password { get; set; }  = string.Empty;
}




/* 
instalado o pacote JWT

dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.IdentityModel.Tokens
dotnet add package System.IdentityModel.Tokens.Jwt

*/
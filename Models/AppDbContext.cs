using Microsoft.EntityFrameworkCore;

namespace ProjetoEscolaDeIdiomas.Models;

public class AppDbContext : DbContext
{   
    public DbSet<Aluno> Alunos { get; set; }
    public DbSet<Materia> Materias { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Notas.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aluno>()
            .HasKey(a => a.Matricula);
        
        modelBuilder.Entity<Materia>()
            .HasData(
                new Materia { Id = 1, Nome = "Portugues", Nota = 0.0f, Inscrito = false },
                new Materia { Id = 2, Nome = "Ingles",    Nota = 0.0f, Inscrito = false },
                new Materia { Id = 3, Nome = "Alemao",    Nota = 0.0f, Inscrito = false },
                new Materia { Id = 4, Nome = "Frances",   Nota = 0.0f, Inscrito = false },
                new Materia { Id = 5, Nome = "Japones",   Nota = 0.0f, Inscrito = false }
                );
    }
}
using Microsoft.EntityFrameworkCore;

namespace ProjetoEscolaDeIdiomas.Models;

public class AppDbContext : DbContext
{
    public DbSet<Aluno> Alunos { get; set; }
    public DbSet<Materia> Materias { get; set; }
    public DbSet<AlunoMateria> AlunoMaterias { get; set; }
    public DbSet<Professor> Professores { get; set; }

    // Config. do BD 
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=Notas.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<Aluno>()
            .HasKey(a => a.Matricula);

       
        modelBuilder.Entity<Materia>()
            .HasKey(m => m.Id);

        modelBuilder.Entity<Materia>()
            .HasOne(m => m.Professor)
            .WithMany(p => p.Materias)
            .HasForeignKey(m => m.ProfessorId)
            .OnDelete(DeleteBehavior.SetNull);  // manter a matéria se o prof for exlcuido 

     
        modelBuilder.Entity<AlunoMateria>()
            .HasKey(am => new { am.AlunoId, am.MateriaId });

        modelBuilder.Entity<AlunoMateria>()
            .HasOne(am => am.Aluno)
            .WithMany(a => a.AlunoMaterias)
            .HasForeignKey(am => am.AlunoId);

        modelBuilder.Entity<AlunoMateria>()
            .HasOne(am => am.Materia)
            .WithMany(m => m.AlunoMaterias)
            .HasForeignKey(am => am.MateriaId);
    }
}

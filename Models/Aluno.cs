using System.ComponentModel.DataAnnotations;

namespace ProjetoEscolaDeIdiomas.Models;

public class Aluno
{
    [Key]
    public int Matricula { get; set; }
    public string Nome { get; set; }
    public Materia[] Materias { get; set; }
}
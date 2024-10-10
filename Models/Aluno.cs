using System.ComponentModel.DataAnnotations;

namespace ProjetoEscolaDeIdiomas.Models;

public class Aluno
{
    [Key]
    public int Matricula { get; set; }

    [Required(ErrorMessage = "O nome do aluno é obrigatório.")]
    public string Nome { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A matrícula deve ser um número positivo.")]
    public Materia[] Materias { get; set; }
}
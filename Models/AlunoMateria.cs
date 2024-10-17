using System.ComponentModel.DataAnnotations;

namespace ProjetoEscolaDeIdiomas.Models;

public class AlunoMateria
{
    public int AlunoId { get; set; }
    public Aluno Aluno { get; set; }
    
    public int MateriaId { get; set; }
    public Materia Materia { get; set; }
    
    [Range(0, 10, ErrorMessage = "A nota deve estar entre 0 e 10.")]
    public decimal Nota { get; set; }
}
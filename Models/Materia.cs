using System.ComponentModel.DataAnnotations; //biblioteca com atributos de validação

namespace ProjetoEscolaDeIdiomas.Models;

public class Materia
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome da matéria é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O nome da matéria pode ter no máximo 100 caracteres.")]
    public string Nome { get; set; }

    [Range(0, 10, ErrorMessage = "A nota deve estar entre 0 e 10.")]
    public float Nota { get; set; }

    public bool Inscrito { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProjetoEscolaDeIdiomas.Models;

public class Materia
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "O nome da matéria é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O nome pode ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;
    
   [JsonIgnore]
    public ICollection<AlunoMateria> AlunoMaterias { get; set; } = new List<AlunoMateria>();

    public int? ProfessorId { get; set; }  
    [JsonIgnore]
    public Professor? Professor { get; set; } 
    
}
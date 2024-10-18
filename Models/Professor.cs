using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ProjetoEscolaDeIdiomas.Models
{
    public class Professor
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;
           public string Username { get; set; } = string.Empty;  
           public string Password { get; set; } = string.Empty;  

        public ICollection<Materia> Materias { get; set; } = new List<Materia>();
    }
}

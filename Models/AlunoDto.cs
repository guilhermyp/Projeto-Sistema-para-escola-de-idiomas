namespace ProjetoEscolaDeIdiomas.Models;

public class AlunoDto
{
    public int Matricula { get; set; }
    public string Nome { get; set; } = string.Empty;
    public List<MateriaDto> Materias { get; set; } = new List<MateriaDto>();
}
namespace ProjetoEscolaDeIdiomas.Models;

public class AlunoDto
{
    public int Matricula { get; set; }
    public string Nome { get; set; }
    public List<MateriaDto> Materias { get; set; }
}
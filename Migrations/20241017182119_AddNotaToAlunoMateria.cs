using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEscolaDeIdiomas.Migrations
{
    /// <inheritdoc />
    public partial class AddNotaToAlunoMateria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Nota",
                table: "AlunoMaterias",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nota",
                table: "AlunoMaterias");
        }
    }
}

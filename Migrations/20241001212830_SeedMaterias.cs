using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProjetoEscolaDeIdiomas.Migrations
{
    /// <inheritdoc />
    public partial class SeedMaterias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Inscrito",
                table: "Materias",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Materias",
                columns: new[] { "Id", "AlunoMatricula", "Inscrito", "Nome", "Nota" },
                values: new object[,]
                {
                    { 1, null, false, "Portugues", 0f },
                    { 2, null, false, "Ingles", 0f },
                    { 3, null, false, "Alemao", 0f },
                    { 4, null, false, "Frances", 0f },
                    { 5, null, false, "Japones", 0f }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "Inscrito",
                table: "Materias");
        }
    }
}

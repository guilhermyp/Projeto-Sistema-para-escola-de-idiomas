using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEscolaDeIdiomas.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessorCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Professores",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Professores",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "Nota",
                table: "AlunoMaterias",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Professores");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Professores");

            migrationBuilder.AlterColumn<decimal>(
                name: "Nota",
                table: "AlunoMaterias",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}

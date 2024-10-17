using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEscolaDeIdiomas.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarProfessorEVinculo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfessorId",
                table: "Materias",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Professores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Professores", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Materias_ProfessorId",
                table: "Materias",
                column: "ProfessorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Materias_Professores_ProfessorId",
                table: "Materias",
                column: "ProfessorId",
                principalTable: "Professores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materias_Professores_ProfessorId",
                table: "Materias");

            migrationBuilder.DropTable(
                name: "Professores");

            migrationBuilder.DropIndex(
                name: "IX_Materias_ProfessorId",
                table: "Materias");

            migrationBuilder.DropColumn(
                name: "ProfessorId",
                table: "Materias");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KisiRehberiApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOccupationsAndFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OccupationId",
                table: "Contacts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Occupations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Occupations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_OccupationId",
                table: "Contacts",
                column: "OccupationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Occupations_OccupationId",
                table: "Contacts",
                column: "OccupationId",
                principalTable: "Occupations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Occupations_OccupationId",
                table: "Contacts");

            migrationBuilder.DropTable(
                name: "Occupations");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_OccupationId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "OccupationId",
                table: "Contacts");
        }
    }
}

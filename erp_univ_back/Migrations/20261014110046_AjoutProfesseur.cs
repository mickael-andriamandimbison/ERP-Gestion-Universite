using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_univ_back.Migrations
{
    /// <inheritdoc />
    public partial class AjoutProfesseur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "professeurs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Matricule = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NumeroTelephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    NumeroCin = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    DateNaissance = table.Column<DateOnly>(type: "date", nullable: false),
                    LieuNaissance = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EtablissementId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professeurs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_professeurs_etablissements_EtablissementId",
                        column: x => x.EtablissementId,
                        principalTable: "etablissements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_professeurs_EtablissementId",
                table: "professeurs",
                column: "EtablissementId");

            migrationBuilder.CreateIndex(
                name: "IX_professeurs_Matricule",
                table: "professeurs",
                column: "Matricule",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professeurs_NumeroCin",
                table: "professeurs",
                column: "NumeroCin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professeurs_NumeroTelephone",
                table: "professeurs",
                column: "NumeroTelephone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "professeurs");
        }
    }
}

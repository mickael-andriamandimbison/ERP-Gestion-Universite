using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_univ_back.Migrations
{
    /// <inheritdoc />
    public partial class Inscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EtudiantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FiliereNiveauId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnneeAcademiqueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EtablissementId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inscriptions_AnneeAcademiques_AnneeAcademiqueId",
                        column: x => x.AnneeAcademiqueId,
                        principalTable: "AnneeAcademiques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inscriptions_etablissements_EtablissementId",
                        column: x => x.EtablissementId,
                        principalTable: "etablissements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inscriptions_etudiants_EtudiantId",
                        column: x => x.EtudiantId,
                        principalTable: "etudiants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inscriptions_filiere_niveaux_FiliereNiveauId",
                        column: x => x.FiliereNiveauId,
                        principalTable: "filiere_niveaux",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inscriptions_AnneeAcademiqueId",
                table: "inscriptions",
                column: "AnneeAcademiqueId");

            migrationBuilder.CreateIndex(
                name: "IX_inscriptions_EtablissementId_EtudiantId_FiliereNiveauId_Ann~",
                table: "inscriptions",
                columns: new[] { "EtablissementId", "EtudiantId", "FiliereNiveauId", "AnneeAcademiqueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inscriptions_EtudiantId",
                table: "inscriptions",
                column: "EtudiantId");

            migrationBuilder.CreateIndex(
                name: "IX_inscriptions_FiliereNiveauId",
                table: "inscriptions",
                column: "FiliereNiveauId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inscriptions");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_univ_back.Migrations
{
    /// <inheritdoc />
    public partial class AjoutUniteEnseignement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "unites_enseignement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nom = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Credit = table.Column<int>(type: "integer", nullable: false),
                    Semestre = table.Column<int>(type: "integer", nullable: false),
                    FiliereNiveauId = table.Column<Guid>(type: "uuid", nullable: false),
                    EtablissementId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unites_enseignement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_unites_enseignement_etablissements_EtablissementId",
                        column: x => x.EtablissementId,
                        principalTable: "etablissements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_unites_enseignement_filiere_niveaux_FiliereNiveauId",
                        column: x => x.FiliereNiveauId,
                        principalTable: "filiere_niveaux",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_unites_enseignement_EtablissementId_Code",
                table: "unites_enseignement",
                columns: new[] { "EtablissementId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_unites_enseignement_FiliereNiveauId",
                table: "unites_enseignement",
                column: "FiliereNiveauId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "unites_enseignement");
        }
    }
}

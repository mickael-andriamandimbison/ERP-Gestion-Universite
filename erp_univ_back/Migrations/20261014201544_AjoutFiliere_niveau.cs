using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_univ_back.Migrations
{
    /// <inheritdoc />
    public partial class AjoutFiliere_niveau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "filieres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nom = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    EtablissementId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_filieres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_filieres_etablissements_EtablissementId",
                        column: x => x.EtablissementId,
                        principalTable: "etablissements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "niveaux",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ordre = table.Column<int>(type: "integer", nullable: false),
                    EtablissementId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_niveaux", x => x.Id);
                    table.ForeignKey(
                        name: "FK_niveaux_etablissements_EtablissementId",
                        column: x => x.EtablissementId,
                        principalTable: "etablissements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filiere_niveaux",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FiliereId = table.Column<Guid>(type: "uuid", nullable: false),
                    NiveauId = table.Column<Guid>(type: "uuid", nullable: false),
                    LibelleAffiche = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    EtablissementId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_filiere_niveaux", x => x.Id);
                    table.ForeignKey(
                        name: "FK_filiere_niveaux_etablissements_EtablissementId",
                        column: x => x.EtablissementId,
                        principalTable: "etablissements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_filiere_niveaux_filieres_FiliereId",
                        column: x => x.FiliereId,
                        principalTable: "filieres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_filiere_niveaux_niveaux_NiveauId",
                        column: x => x.NiveauId,
                        principalTable: "niveaux",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_filiere_niveaux_EtablissementId_FiliereId_NiveauId",
                table: "filiere_niveaux",
                columns: new[] { "EtablissementId", "FiliereId", "NiveauId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_filiere_niveaux_FiliereId",
                table: "filiere_niveaux",
                column: "FiliereId");

            migrationBuilder.CreateIndex(
                name: "IX_filiere_niveaux_NiveauId",
                table: "filiere_niveaux",
                column: "NiveauId");

            migrationBuilder.CreateIndex(
                name: "IX_filieres_EtablissementId_Code",
                table: "filieres",
                columns: new[] { "EtablissementId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_niveaux_EtablissementId_Code",
                table: "niveaux",
                columns: new[] { "EtablissementId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "filiere_niveaux");

            migrationBuilder.DropTable(
                name: "filieres");

            migrationBuilder.DropTable(
                name: "niveaux");
        }
    }
}

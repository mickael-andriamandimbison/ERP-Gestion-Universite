using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_univ_back.Migrations
{
    /// <inheritdoc />
    public partial class AjoutEtudiantParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "etudiants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Matricule = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DateNaissance = table.Column<DateOnly>(type: "date", nullable: false),
                    LieuNaissance = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NumeroCin = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: true),
                    NumeroTelphone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_etudiants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "parents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NumeroTelphone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Profession = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "etudiant_parent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EtudiantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_etudiant_parent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_etudiant_parent_etudiants_EtudiantId",
                        column: x => x.EtudiantId,
                        principalTable: "etudiants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_etudiant_parent_parents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_etudiant_parent_EtudiantId",
                table: "etudiant_parent",
                column: "EtudiantId");

            migrationBuilder.CreateIndex(
                name: "IX_etudiant_parent_ParentId",
                table: "etudiant_parent",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_etudiants_Matricule",
                table: "etudiants",
                column: "Matricule",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_etudiants_NumeroCin",
                table: "etudiants",
                column: "NumeroCin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_etudiants_NumeroTelphone",
                table: "etudiants",
                column: "NumeroTelphone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parents_NumeroTelphone",
                table: "parents",
                column: "NumeroTelphone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "etudiant_parent");

            migrationBuilder.DropTable(
                name: "etudiants");

            migrationBuilder.DropTable(
                name: "parents");
        }
    }
}

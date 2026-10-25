using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_univ_back.Migrations
{
    /// <inheritdoc />
    public partial class AjoutAnneeAcademique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnneeAcademiques",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Libelle = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    DateDebut = table.Column<DateOnly>(type: "date", nullable: false),
                    DateFin = table.Column<DateOnly>(type: "date", nullable: false),
                    EstActive = table.Column<bool>(type: "boolean", nullable: false),
                    EtablissementId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnneeAcademiques", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnneeAcademiques_etablissements_EtablissementId",
                        column: x => x.EtablissementId,
                        principalTable: "etablissements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnneeAcademiques_EtablissementId_Libelle",
                table: "AnneeAcademiques",
                columns: new[] { "EtablissementId", "Libelle" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnneeAcademiques");
        }
    }
}

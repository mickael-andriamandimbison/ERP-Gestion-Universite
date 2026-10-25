using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_univ_back.Migrations
{
    /// <inheritdoc />
    public partial class CorrectionOrthographeTelephone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NumeroTelphone",
                table: "etudiants",
                newName: "NumeroTelephone");

            migrationBuilder.RenameIndex(
                name: "IX_etudiants_EtablissementId_NumeroTelphone",
                table: "etudiants",
                newName: "IX_etudiants_EtablissementId_NumeroTelephone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NumeroTelephone",
                table: "etudiants",
                newName: "NumeroTelphone");

            migrationBuilder.RenameIndex(
                name: "IX_etudiants_EtablissementId_NumeroTelephone",
                table: "etudiants",
                newName: "IX_etudiants_EtablissementId_NumeroTelphone");
        }
    }
}

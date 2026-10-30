using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_univ_back.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMultiTenantIndexesAndParents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_professeurs_EtablissementId",
                table: "professeurs");

            migrationBuilder.DropIndex(
                name: "IX_professeurs_Matricule",
                table: "professeurs");

            migrationBuilder.DropIndex(
                name: "IX_professeurs_NumeroCin",
                table: "professeurs");

            migrationBuilder.DropIndex(
                name: "IX_professeurs_NumeroTelephone",
                table: "professeurs");

            migrationBuilder.DropIndex(
                name: "IX_parents_NumeroTelphone",
                table: "parents");

            migrationBuilder.DropIndex(
                name: "IX_etudiants_EtablissementId",
                table: "etudiants");

            migrationBuilder.DropIndex(
                name: "IX_etudiants_Matricule",
                table: "etudiants");

            migrationBuilder.DropIndex(
                name: "IX_etudiants_NumeroCin",
                table: "etudiants");

            migrationBuilder.DropIndex(
                name: "IX_etudiants_NumeroTelphone",
                table: "etudiants");

            migrationBuilder.DropIndex(
                name: "IX_etudiant_parent_EtudiantId",
                table: "etudiant_parent");

            migrationBuilder.AddColumn<Guid>(
                name: "EtablissementId",
                table: "parents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("564ef3da-994f-4566-88d8-f61e8a7e569d"));

            migrationBuilder.CreateIndex(
                name: "IX_professeurs_EtablissementId_Matricule",
                table: "professeurs",
                columns: new[] { "EtablissementId", "Matricule" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professeurs_EtablissementId_NumeroCin",
                table: "professeurs",
                columns: new[] { "EtablissementId", "NumeroCin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professeurs_EtablissementId_NumeroTelephone",
                table: "professeurs",
                columns: new[] { "EtablissementId", "NumeroTelephone" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parents_EtablissementId_NumeroTelphone",
                table: "parents",
                columns: new[] { "EtablissementId", "NumeroTelphone" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_etudiants_EtablissementId_Matricule",
                table: "etudiants",
                columns: new[] { "EtablissementId", "Matricule" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_etudiants_EtablissementId_NumeroCin",
                table: "etudiants",
                columns: new[] { "EtablissementId", "NumeroCin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_etudiants_EtablissementId_NumeroTelphone",
                table: "etudiants",
                columns: new[] { "EtablissementId", "NumeroTelphone" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_etudiant_parent_EtudiantId_ParentId",
                table: "etudiant_parent",
                columns: new[] { "EtudiantId", "ParentId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_parents_etablissements_EtablissementId",
                table: "parents",
                column: "EtablissementId",
                principalTable: "etablissements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_parents_etablissements_EtablissementId",
                table: "parents");

            migrationBuilder.DropIndex(
                name: "IX_professeurs_EtablissementId_Matricule",
                table: "professeurs");

            migrationBuilder.DropIndex(
                name: "IX_professeurs_EtablissementId_NumeroCin",
                table: "professeurs");

            migrationBuilder.DropIndex(
                name: "IX_professeurs_EtablissementId_NumeroTelephone",
                table: "professeurs");

            migrationBuilder.DropIndex(
                name: "IX_parents_EtablissementId_NumeroTelphone",
                table: "parents");

            migrationBuilder.DropIndex(
                name: "IX_etudiants_EtablissementId_Matricule",
                table: "etudiants");

            migrationBuilder.DropIndex(
                name: "IX_etudiants_EtablissementId_NumeroCin",
                table: "etudiants");

            migrationBuilder.DropIndex(
                name: "IX_etudiants_EtablissementId_NumeroTelphone",
                table: "etudiants");

            migrationBuilder.DropIndex(
                name: "IX_etudiant_parent_EtudiantId_ParentId",
                table: "etudiant_parent");

            migrationBuilder.DropColumn(
                name: "EtablissementId",
                table: "parents");

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

            migrationBuilder.CreateIndex(
                name: "IX_parents_NumeroTelphone",
                table: "parents",
                column: "NumeroTelphone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_etudiants_EtablissementId",
                table: "etudiants",
                column: "EtablissementId");

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
                name: "IX_etudiant_parent_EtudiantId",
                table: "etudiant_parent",
                column: "EtudiantId");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_univ_back.Migrations
{
    /// <inheritdoc />
    public partial class EtablissementEtudiantLiaison : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_etudiant_parent_etudiants_EtudiantId",
                table: "etudiant_parent");

            migrationBuilder.DropForeignKey(
                name: "FK_etudiant_parent_parents_ParentId",
                table: "etudiant_parent");

            migrationBuilder.AddColumn<Guid>(
                name: "EtablissementId",
                table: "etudiants",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_etudiants_EtablissementId",
                table: "etudiants",
                column: "EtablissementId");

            migrationBuilder.AddForeignKey(
                name: "FK_etudiant_parent_etudiants_EtudiantId",
                table: "etudiant_parent",
                column: "EtudiantId",
                principalTable: "etudiants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_etudiant_parent_parents_ParentId",
                table: "etudiant_parent",
                column: "ParentId",
                principalTable: "parents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_etudiants_etablissements_EtablissementId",
                table: "etudiants",
                column: "EtablissementId",
                principalTable: "etablissements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_etudiant_parent_etudiants_EtudiantId",
                table: "etudiant_parent");

            migrationBuilder.DropForeignKey(
                name: "FK_etudiant_parent_parents_ParentId",
                table: "etudiant_parent");

            migrationBuilder.DropForeignKey(
                name: "FK_etudiants_etablissements_EtablissementId",
                table: "etudiants");

            migrationBuilder.DropIndex(
                name: "IX_etudiants_EtablissementId",
                table: "etudiants");

            migrationBuilder.DropColumn(
                name: "EtablissementId",
                table: "etudiants");

            migrationBuilder.AddForeignKey(
                name: "FK_etudiant_parent_etudiants_EtudiantId",
                table: "etudiant_parent",
                column: "EtudiantId",
                principalTable: "etudiants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_etudiant_parent_parents_ParentId",
                table: "etudiant_parent",
                column: "ParentId",
                principalTable: "parents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Pedidos360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarOrdenesServicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TiposServicio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposServicio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesServicio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    TipoServicioId = table.Column<int>(type: "int", nullable: false),
                    Equipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DescripcionProblema = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CostoEstimado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesServicio", x => x.Id);
                    table.CheckConstraint("CK_OrdenesServicio_CostoEstimado", "[CostoEstimado] > 0");
                    table.CheckConstraint("CK_OrdenesServicio_Estado", "[Estado] IN (1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_OrdenesServicio_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesServicio_TiposServicio_TipoServicioId",
                        column: x => x.TipoServicioId,
                        principalTable: "TiposServicio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TiposServicio",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Diagnóstico" },
                    { 2, "Reparación de hardware" },
                    { 3, "Mantenimiento preventivo" },
                    { 4, "Instalación de software" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesServicio_ClienteId",
                table: "OrdenesServicio",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesServicio_Estado",
                table: "OrdenesServicio",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesServicio_TipoServicioId",
                table: "OrdenesServicio",
                column: "TipoServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposServicio_Nombre",
                table: "TiposServicio",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrdenesServicio");

            migrationBuilder.DropTable(
                name: "TiposServicio");
        }
    }
}

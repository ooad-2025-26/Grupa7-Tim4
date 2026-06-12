using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZamETF.Migrations
{
    /// <inheritdoc />
    public partial class DodajMaxBodoviNaZadacu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxBodovi",
                table: "Zadace",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxBodovi",
                table: "Zadace");
        }
    }
}

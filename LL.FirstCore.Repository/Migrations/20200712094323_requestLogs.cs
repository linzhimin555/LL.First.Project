using Microsoft.EntityFrameworkCore.Migrations;

namespace LL.FirstCore.Repository.Migrations
{
    public partial class requestLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ElaspedTime",
                table: "RequestLog",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElaspedTime",
                table: "RequestLog");
        }
    }
}

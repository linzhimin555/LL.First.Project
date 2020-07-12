using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LL.FirstCore.Repository.Migrations
{
    public partial class initlogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TranceId = table.Column<string>(nullable: true),
                    ClientIp = table.Column<string>(nullable: true),
                    RequestMethod = table.Column<string>(nullable: true),
                    RequestHeaders = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    ExecutedTime = table.Column<DateTime>(nullable: true),
                    RequestParamters = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestLog", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestLog");
        }
    }
}

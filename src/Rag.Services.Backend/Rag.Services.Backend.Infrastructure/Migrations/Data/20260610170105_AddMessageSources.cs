using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rag.Services.Backend.Infrastructure.Migrations.Data
{
    /// <inheritdoc />
    public partial class AddMessageSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Excerpt = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Score = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageSources_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageSources_MessageId",
                table: "MessageSources",
                column: "MessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageSources");
        }
    }
}

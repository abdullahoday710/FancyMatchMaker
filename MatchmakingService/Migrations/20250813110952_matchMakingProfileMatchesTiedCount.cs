using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchmakingService.Migrations
{
    /// <inheritdoc />
    public partial class matchMakingProfileMatchesTiedCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MatchesTied",
                table: "MatchMakingProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchesTied",
                table: "MatchMakingProfiles");
        }
    }
}

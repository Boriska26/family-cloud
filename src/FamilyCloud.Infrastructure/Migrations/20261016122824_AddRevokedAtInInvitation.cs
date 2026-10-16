using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyCloud.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRevokedAtInInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RevokedAt",
                table: "Invitations",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "Invitations");
        }
    }
}

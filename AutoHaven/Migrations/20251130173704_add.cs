using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoHaven.Migrations
{
    /// <inheritdoc />
    public partial class add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "IsApproved", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "11744226-3b86-4196-aea6-da9ca34abf44", new DateTime(2025, 11, 30, 19, 37, 2, 713, DateTimeKind.Local).AddTicks(2557), true, "AQAAAAIAAYagAAAAEDKeiyM8FIgviAxXfaBMpLcRIxB+zh9G5huFRD9sAf0ir1ICUuTGENoNmeTT1rF3tA==", "3a6a3c5b-2613-496b-b7db-c0ce524ccda5", new DateTime(2025, 11, 30, 19, 37, 2, 713, DateTimeKind.Local).AddTicks(2562) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "IsApproved", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "78eb066d-ef45-46ff-b8bd-487334cdc087", new DateTime(2025, 11, 30, 19, 37, 2, 812, DateTimeKind.Local).AddTicks(8812), true, "AQAAAAIAAYagAAAAEF7xv8YoP0ccbiv27gPUWU6JQspqAvZSI5StjoybFjcEa1w7nE9fk9tvcYdrO4OxUw==", "449e99fe-a06e-4fa2-823a-dea2ee73f89d", new DateTime(2025, 11, 30, 19, 37, 2, 812, DateTimeKind.Local).AddTicks(8899) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "IsApproved", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "9f1c0904-7f0a-41f6-be05-0eb1b52ec696", new DateTime(2025, 11, 30, 19, 37, 2, 913, DateTimeKind.Local).AddTicks(4891), true, "AQAAAAIAAYagAAAAEOD/FTqHOuo0cOFm6DYKfEtR3kdn3Nv0+9weP+xC6l2T+A+Fj9zwL0ftv2AMNzfWJA==", "aae6e6c9-c4cd-493a-b576-394476fd3cc4", new DateTime(2025, 11, 30, 19, 37, 2, 913, DateTimeKind.Local).AddTicks(4896) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "IsApproved", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "a0b255c0-d260-433a-b6ae-8945101373f8", new DateTime(2025, 11, 30, 19, 37, 3, 13, DateTimeKind.Local).AddTicks(5167), true, "AQAAAAIAAYagAAAAEHATsoOD96dM//XrOTZR+jPUhZcQmyQqKh1AweKr7jdNK2GMabcCjDudj8CXHQA+Ew==", "59f436ab-e6af-4056-ab36-b9bdbf3e5296", new DateTime(2025, 11, 30, 19, 37, 3, 13, DateTimeKind.Local).AddTicks(5172) });

            migrationBuilder.UpdateData(
                table: "UserSubscriptions",
                keyColumn: "UserSubscriptionId",
                keyValue: 1,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 11, 30, 19, 37, 3, 113, DateTimeKind.Local).AddTicks(4700), new DateTime(2025, 11, 30, 19, 37, 3, 113, DateTimeKind.Local).AddTicks(4634) });

            migrationBuilder.UpdateData(
                table: "UserSubscriptions",
                keyColumn: "UserSubscriptionId",
                keyValue: 2,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 11, 30, 19, 37, 3, 113, DateTimeKind.Local).AddTicks(4728), new DateTime(2025, 11, 30, 19, 37, 3, 113, DateTimeKind.Local).AddTicks(4725) });

            migrationBuilder.UpdateData(
                table: "UserSubscriptions",
                keyColumn: "UserSubscriptionId",
                keyValue: 3,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 11, 30, 19, 37, 3, 113, DateTimeKind.Local).AddTicks(4745), new DateTime(2025, 11, 30, 19, 37, 3, 113, DateTimeKind.Local).AddTicks(4733) });

            migrationBuilder.UpdateData(
                table: "UserSubscriptions",
                keyColumn: "UserSubscriptionId",
                keyValue: 4,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 11, 30, 19, 37, 3, 113, DateTimeKind.Local).AddTicks(4803), new DateTime(2025, 11, 30, 19, 37, 3, 113, DateTimeKind.Local).AddTicks(4799) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "IsApproved", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "f6c46e1f-9400-47be-898b-d0f2046419f6", new DateTime(2025, 11, 30, 17, 54, 14, 49, DateTimeKind.Local).AddTicks(6323), false, "AQAAAAIAAYagAAAAEN4U/dujMLRzhWSvF5OJeERwVz32lfiBbLCwBixzqIkicUQtRzgEwu8kCgjjb5yskw==", "09f9fc49-dc93-419f-9b5b-1b509a811cd9", new DateTime(2025, 11, 30, 17, 54, 14, 49, DateTimeKind.Local).AddTicks(6351) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "IsApproved", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "823a1988-639c-43b9-bb81-c6a06f3cd10b", new DateTime(2025, 11, 30, 17, 54, 14, 141, DateTimeKind.Local).AddTicks(6680), false, "AQAAAAIAAYagAAAAEIt2weFIe2pzdU3vJWuo51YwQhJb5ETVa8CKMG+YhCfZHKHcdBJ5lepSbeU1/m7U4g==", "201580a4-b807-4384-9d77-7aa397b6a118", new DateTime(2025, 11, 30, 17, 54, 14, 141, DateTimeKind.Local).AddTicks(6684) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "IsApproved", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "af0dc4ba-4529-469d-964f-9589d9e2e16f", new DateTime(2025, 11, 30, 17, 54, 14, 251, DateTimeKind.Local).AddTicks(7636), false, "AQAAAAIAAYagAAAAEKwe3YtSSCrs+GYc9YNlU0mJSWyxW6IfClA5dn4fOnDzK2BXB3XOBV4EfATaDS6gBQ==", "eef6be3b-0e69-47b4-b398-ab57a179b1dd", new DateTime(2025, 11, 30, 17, 54, 14, 251, DateTimeKind.Local).AddTicks(7642) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "IsApproved", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "bf1f34db-b52a-4b73-b628-015ebfff6dc3", new DateTime(2025, 11, 30, 17, 54, 14, 369, DateTimeKind.Local).AddTicks(2789), false, "AQAAAAIAAYagAAAAECt1OwkX3rY33mrHkSvQKSJknxQ7EM5jndRoa6458RFtrLSn/eimViFZjlha5bFmsw==", "b493f5ff-f502-4b30-9306-851569e525eb", new DateTime(2025, 11, 30, 17, 54, 14, 369, DateTimeKind.Local).AddTicks(2794) });

            migrationBuilder.UpdateData(
                table: "UserSubscriptions",
                keyColumn: "UserSubscriptionId",
                keyValue: 1,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 11, 30, 17, 54, 14, 453, DateTimeKind.Local).AddTicks(6665), new DateTime(2025, 11, 30, 17, 54, 14, 453, DateTimeKind.Local).AddTicks(6567) });

            migrationBuilder.UpdateData(
                table: "UserSubscriptions",
                keyColumn: "UserSubscriptionId",
                keyValue: 2,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 11, 30, 17, 54, 14, 453, DateTimeKind.Local).AddTicks(6736), new DateTime(2025, 11, 30, 17, 54, 14, 453, DateTimeKind.Local).AddTicks(6731) });

            migrationBuilder.UpdateData(
                table: "UserSubscriptions",
                keyColumn: "UserSubscriptionId",
                keyValue: 3,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 11, 30, 17, 54, 14, 453, DateTimeKind.Local).AddTicks(6744), new DateTime(2025, 11, 30, 17, 54, 14, 453, DateTimeKind.Local).AddTicks(6740) });

            migrationBuilder.UpdateData(
                table: "UserSubscriptions",
                keyColumn: "UserSubscriptionId",
                keyValue: 4,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 11, 30, 17, 54, 14, 453, DateTimeKind.Local).AddTicks(6751), new DateTime(2025, 11, 30, 17, 54, 14, 453, DateTimeKind.Local).AddTicks(6748) });
        }
    }
}

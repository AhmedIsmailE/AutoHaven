using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoHaven.Migrations
{
    /// <inheritdoc />
    public partial class testPhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "3c832667-fdd7-4f11-bbaa-ebbd0c43bf09", new DateTime(2025, 11, 29, 23, 46, 46, 460, DateTimeKind.Local).AddTicks(3599), "AQAAAAIAAYagAAAAELpRzVkRrR5c0twDH7Z69OrJT917W2Kcnk12Z1lhKiJSv7gvTwgaGDEG73GqdgUSjQ==", "d7eb2556-175a-446a-8b0d-bbd393ff532c", new DateTime(2025, 11, 29, 23, 46, 46, 460, DateTimeKind.Local).AddTicks(3634) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "0f73a6fa-0e28-4aff-bcd2-d89455b16c94", new DateTime(2025, 11, 29, 23, 46, 46, 561, DateTimeKind.Local).AddTicks(9426), "AQAAAAIAAYagAAAAENRPSQ2IuqSHWWbpMS2QUpafNh1L+TVSG6I95dxvt2bR8XEj4oxpl0iMKqaLBtutmw==", "52c1d2f5-5625-4e8b-82c5-23b44309153c", new DateTime(2025, 11, 29, 23, 46, 46, 561, DateTimeKind.Local).AddTicks(9433) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "a07c4f55-243f-4ea6-ba77-f8f701420cdc", new DateTime(2025, 11, 29, 23, 46, 46, 662, DateTimeKind.Local).AddTicks(6568), "AQAAAAIAAYagAAAAEDXox/4+2pXYeVb8q1IvPLsrvr98zIUxtmp1THyyBUsaYFUAuDypZ+FBJUixmiKW+Q==", "040bf9c6-747e-47e8-9722-438af12e5221", new DateTime(2025, 11, 29, 23, 46, 46, 662, DateTimeKind.Local).AddTicks(6574) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "e8f05ace-4cae-43bc-b425-2a3f24a4a259", new DateTime(2025, 11, 29, 23, 46, 46, 763, DateTimeKind.Local).AddTicks(26), "AQAAAAIAAYagAAAAEH6rHuMS+l5gjC23+trEfzBBAtNB1No2TfNaJShyMvkmljrVDz2/TzT55L1HFLTtpw==", "7e9cf261-15bc-4cfc-b3b2-08d1d65d2c5c", new DateTime(2025, 11, 29, 23, 46, 46, 763, DateTimeKind.Local).AddTicks(31) });

            migrationBuilder.UpdateData(
                table: "UserSubscriptions",
                keyColumn: "UserSubscriptionId",
                keyValue: 1,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 11, 29, 23, 46, 46, 866, DateTimeKind.Local).AddTicks(524), new DateTime(2025, 11, 29, 23, 46, 46, 866, DateTimeKind.Local).AddTicks(441) });

            migrationBuilder.UpdateData(
                table: "UserSubscriptions",
                keyColumn: "UserSubscriptionId",
                keyValue: 2,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 11, 29, 23, 46, 46, 866, DateTimeKind.Local).AddTicks(573), new DateTime(2025, 11, 29, 23, 46, 46, 866, DateTimeKind.Local).AddTicks(569) });

            migrationBuilder.UpdateData(
                table: "UserSubscriptions",
                keyColumn: "UserSubscriptionId",
                keyValue: 3,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 11, 29, 23, 46, 46, 866, DateTimeKind.Local).AddTicks(581), new DateTime(2025, 11, 29, 23, 46, 46, 866, DateTimeKind.Local).AddTicks(578) });

            migrationBuilder.UpdateData(
                table: "UserSubscriptions",
                keyColumn: "UserSubscriptionId",
                keyValue: 4,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 11, 29, 23, 46, 46, 866, DateTimeKind.Local).AddTicks(589), new DateTime(2025, 11, 29, 23, 46, 46, 866, DateTimeKind.Local).AddTicks(586) });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PhoneNumber",
                table: "AspNetUsers",
                column: "PhoneNumber",
                unique: true,
                filter: "[PhoneNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PhoneNumber",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "23322f47-553e-47f8-9b8c-99af700e4208", new DateTime(2025, 11, 29, 22, 49, 25, 192, DateTimeKind.Local).AddTicks(4422), "AQAAAAIAAYagAAAAEFfvOSgekG+TrqaY1iBLyA2lFPYSM5c/1ZVwTWWdFGyRpYoXPZJZqANn0yf/bb0brw==", "f4100d11-aafd-48ec-b673-6e00051150f8", new DateTime(2025, 11, 29, 22, 49, 25, 192, DateTimeKind.Local).AddTicks(4459) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "42c39b07-c2a2-44d2-b27d-eda104a9467d", new DateTime(2025, 11, 29, 22, 49, 25, 263, DateTimeKind.Local).AddTicks(7107), "AQAAAAIAAYagAAAAEMLlVDZsymXHA3Q/FDNijZcPEm/zrBii/ztila7E+pJWHWbPbbD9raFm4L1u4/+rtA==", "90d616f7-013b-4f76-94fd-8b0fe9c8550d", new DateTime(2025, 11, 29, 22, 49, 25, 263, DateTimeKind.Local).AddTicks(7110) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "cd7778ed-c809-4b8f-9cae-aff5c594bfbb", new DateTime(2025, 11, 29, 22, 49, 25, 342, DateTimeKind.Local).AddTicks(4948), "AQAAAAIAAYagAAAAEMZRqJ43Gtgu2IWz2p1Jp2OyL8Xadg8rxz6MoPnR4137fYN3Z9Sl8Xc07UZI5dKjKA==", "3e496a25-b10f-473a-a831-e732f8b38333", new DateTime(2025, 11, 29, 22, 49, 25, 342, DateTimeKind.Local).AddTicks(4951) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "2e50d3d6-d455-4626-815b-6cd8f049dad4", new DateTime(2025, 11, 29, 22, 49, 25, 430, DateTimeKind.Local).AddTicks(8933), "AQAAAAIAAYagAAAAEFFppWYNgAgUK+DMXipateqk46Zb4lDyaWkZAnRtl0/VYuAj0RrL+86JqgBgcdwM+Q==", "da8ff12e-5d5f-45b2-ba3d-6565d755d74c", new DateTime(2025, 11, 29, 22, 49, 25, 430, DateTimeKind.Local).AddTicks(8950) });

            migrationBuilder.UpdateData(
                table: "UserSubscriptions",
                keyColumn: "UserSubscriptionId",
                keyValue: 1,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 11, 29, 22, 49, 25, 541, DateTimeKind.Local).AddTicks(82), new DateTime(2025, 11, 29, 22, 49, 25, 540, DateTimeKind.Local).AddTicks(9944) });

            migrationBuilder.UpdateData(
                table: "UserSubscriptions",
                keyColumn: "UserSubscriptionId",
                keyValue: 2,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 11, 29, 22, 49, 25, 541, DateTimeKind.Local).AddTicks(268), new DateTime(2025, 11, 29, 22, 49, 25, 541, DateTimeKind.Local).AddTicks(251) });

            migrationBuilder.UpdateData(
                table: "UserSubscriptions",
                keyColumn: "UserSubscriptionId",
                keyValue: 3,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 11, 29, 22, 49, 25, 541, DateTimeKind.Local).AddTicks(311), new DateTime(2025, 11, 29, 22, 49, 25, 541, DateTimeKind.Local).AddTicks(301) });

            migrationBuilder.UpdateData(
                table: "UserSubscriptions",
                keyColumn: "UserSubscriptionId",
                keyValue: 4,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 11, 29, 22, 49, 25, 541, DateTimeKind.Local).AddTicks(352), new DateTime(2025, 11, 29, 22, 49, 25, 541, DateTimeKind.Local).AddTicks(341) });
        }
    }
}

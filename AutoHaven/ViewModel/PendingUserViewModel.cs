using System;

namespace AutoHaven.ViewModel
{
    public class PendingUserViewModel
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? NationalId { get; set; }
        public string? IdImagePath { get; set; }
    }
}
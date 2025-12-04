namespace AutoHaven.ViewModel
{
    public class AdminUsersViewModel
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; } = 0;

        public string Search { get; set; } = string.Empty;

        public List<UserRow> Users { get; set; } = new();

        public class UserRow
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string CompanyName { get; set; }
            public string AvatarUrl { get; set; } = string.Empty;
            public bool IsBanned { get; set; }
            public DateTime JoinedAt { get; set; }
        }
    }
}
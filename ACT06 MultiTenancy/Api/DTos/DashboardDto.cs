namespace ACT06_MultiTenancy.Api.DTos
{
    public class DashboardResponseDto
    {
        public DashboardUserDto User { get; set; } = new();
        public DashboardNotificationsDto Notifications { get; set; } = new();
        public DashboardStatsDto Stats { get; set; } = new();
        public List<DashboardLoanRowDto> ActiveLoansTable { get; set; } = new();
        public List<DashboardUpcomingReturnDto> UpcomingReturns { get; set; } = new();
    }

    public class DashboardUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class DashboardNotificationsDto
    {
        public int UnreadCount { get; set; }
        public List<DashboardNotificationDto> Recent { get; set; } = new();
    }

    public class DashboardNotificationDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class DashboardStatsDto
    {
        public int ActiveLoans { get; set; }
        public int WeeklyReturns { get; set; }
    }

    public class DashboardLoanRowDto
    {
        public int Id { get; set; }
        public string ArticleName { get; set; } = string.Empty;
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class DashboardUpcomingReturnDto
    {
        public int Id { get; set; }
        public string ArticleName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}

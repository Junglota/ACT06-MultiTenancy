namespace ACT06_MultiTenancy.Api.Models
{
    public record ChangePasswordRequest(string Username, string CurrentPassword, string NewPassword);
}

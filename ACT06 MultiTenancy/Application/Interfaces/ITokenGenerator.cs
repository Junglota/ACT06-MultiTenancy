using ACT06_MultiTenancy.Domain.Entities;

namespace ACT06_MultiTenancy.Application.Interfaces
{
    public interface ITokenGenerator
    {
        string Generate(User user);
    }
}

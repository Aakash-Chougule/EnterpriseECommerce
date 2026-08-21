using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Interfaces;

public interface IPermissionRepository
{
    Task<IReadOnlyList<Permission>>
        GetAllAsync();

    Task<IReadOnlyList<Permission>>
        GetByNamesAsync(
            IEnumerable<string> names);
}
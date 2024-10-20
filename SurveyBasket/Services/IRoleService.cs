using SurveyBasket.Contracts.Roles;

namespace SurveyBasket.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleResponse>> GetAllAsync(bool? includeDisabled = false, CancellationToken cancellationToken = default);
        Task<Result<RoleDeatilResponse>> GetAsync(string id);
        Task<Result<RoleDeatilResponse>> AddAsync(RoleRequest request);
        Task<Result> UpdateAsync(string id, RoleRequest request);
        Task<Result> ToggleStatusAsync(string id);
    }
}

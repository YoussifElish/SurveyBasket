using SurveyBasket.Contracts.Users;

namespace SurveyBasket.Services
{
    public interface IUserService
    {
        Task<Result<UserProfileResponse>> GetProfileAsync(string userId);
        Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request);
        Task<Result> UpdatePasswordAsync(string userId, ChangePasswordRequest request);
        Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<UserResponse>> GetAsync(string id, CancellationToken cancellationToken = default);
        Task<Result<UserResponse>> AddAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken = default);
        Task<Result> ToggleStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<Result> UnlockAsync(string id, CancellationToken cancellationToken = default);
    }
}

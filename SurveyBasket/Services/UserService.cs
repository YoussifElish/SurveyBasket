using SurveyBasket.Contracts.Users;

namespace SurveyBasket.Services
{
    public class UserService(UserManager<ApplicationUser> userManager, IRoleService roleService, ApplicationDbContext context) : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IRoleService _roleService = roleService;
        private readonly ApplicationDbContext _context = context;

        public async Task<Result<UserProfileResponse>> GetProfileAsync(string userId)
        {
            var user = await _userManager.Users.Where(x => x.Id == userId).ProjectToType<UserProfileResponse>().SingleAsync();
            return Result.Success(user);
        }


        public async Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request)
        {

            await _userManager.Users.Where(x => x.Id == userId).ExecuteUpdateAsync(setters =>
            setters
            .SetProperty(x => x.FirstName, request.FirstName)
            .SetProperty(x => x.LastName, request.LastName)
            );
            return Result.Success();
        }

        public async Task<Result> UpdatePasswordAsync(string userId, ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var currentPassword = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (currentPassword.Succeeded)
                return Result.Success();
            var error = currentPassword.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        public async Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await (from u in _context.Users
                   join ur in _context.UserRoles
                   on u.Id equals ur.UserId
                   join r in _context.Roles
                   on ur.RoleId equals r.Id into roles
                   where !roles.Any(x => x.Name == DefaultRoles.Member)
                   select new
                   {
                       u.Id,
                       u.FirstName,
                       u.LastName,
                       u.Email,
                       u.IsDisabled,
                       Roles = roles.Select(x => x.Name!).ToList()
                   }
                   ).GroupBy(u => new { u.Id, u.FirstName, u.LastName, u.Email, u.IsDisabled }).Select(u => new UserResponse(
                       u.Key.Id,
                       u.Key.FirstName,
                       u.Key.LastName,
                       u.Key.Email,
                       u.Key.IsDisabled,
                       u.SelectMany(x => x.Roles)
                       ))
                     .ToListAsync(cancellationToken);




        public async Task<Result<UserResponse>> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            if (await _userManager.FindByIdAsync(id) is not { } user)
                return Result.Failure<UserResponse>(UserErrors.NotFound);

            var userRoles = await _userManager.GetRolesAsync(user);

            var response = (user, userRoles).Adapt<UserResponse>();
            return Result.Success(response);

        }
        public async Task<Result<UserResponse>> AddAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            var emailIsExist = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);
            if (emailIsExist)
                return Result.Failure<UserResponse>(UserErrors.DuplicatedEmail);
            var allowedRoles = await _roleService.GetAllAsync(cancellationToken: cancellationToken);
            if (request.Roles.Except(allowedRoles.Select(x => x.Name)).Any())
                return Result.Failure<UserResponse>(RoleErrors.InvalidPermissions);

            var user = request.Adapt<ApplicationUser>();
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRolesAsync(user, request.Roles);
                var response = (user, request.Roles).Adapt<UserResponse>();
                return Result.Success(response);
            }

            var error = result.Errors.First();
            return Result.Failure<UserResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

        }

        public async Task<Result> UpdateAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken = default)
        {
            if (await _userManager.FindByIdAsync(id) is not { } user)
                return Result.Failure(UserErrors.NotFound);
            var emailIsExist = await _userManager.Users.AnyAsync(x => x.Email == request.Email && x.Id != id, cancellationToken);
            if (emailIsExist)
                return Result.Failure<UserResponse>(UserErrors.DuplicatedEmail);
            var allowedRoles = await _roleService.GetAllAsync(cancellationToken: cancellationToken);
            if (request.Roles.Except(allowedRoles.Select(x => x.Name)).Any())
                return Result.Failure<UserResponse>(RoleErrors.InvalidPermissions);
            user = request.Adapt(user);
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _context.UserRoles.Where(x => x.UserId == id).ExecuteDeleteAsync(cancellationToken);
                await _userManager.AddToRolesAsync(user, request.Roles);
                return Result.Success();
            }

            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

        }
        public async Task<Result> ToggleStatusAsync(string id, CancellationToken cancellationToken = default)
        {
            if (await _userManager.FindByIdAsync(id) is not { } user)
                return Result.Failure(UserErrors.NotFound);

            user.IsDisabled = !user.IsDisabled;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return Result.Success();
            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }
        public async Task<Result> UnlockAsync(string id, CancellationToken cancellationToken = default)
        {
            if (await _userManager.FindByIdAsync(id) is not { } user)
                return Result.Failure(UserErrors.NotFound);
            var result = await _userManager.SetLockoutEndDateAsync(user, null);
            if (result.Succeeded)
                return Result.Success();
            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }
    }
}

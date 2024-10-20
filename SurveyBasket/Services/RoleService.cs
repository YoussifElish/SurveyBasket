using SurveyBasket.Contracts.Roles;

namespace SurveyBasket.Services
{
    public class RoleService(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context) : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
        private readonly ApplicationDbContext _context = context;

        public async Task<IEnumerable<RoleResponse>> GetAllAsync(bool? includeDisabled = false, CancellationToken cancellationToken = default)
        => await _roleManager.Roles.Where(x => !x.IsDefault && (!x.IsDeleted || (includeDisabled.HasValue && includeDisabled.Value))).ProjectToType<RoleResponse>().ToListAsync(cancellationToken);



        public async Task<Result<RoleDeatilResponse>> GetAsync(string id)
        {
            if (await _roleManager.FindByIdAsync(id) is not { } role)
                return Result.Failure<RoleDeatilResponse>(RoleErrors.InavlidRole);

            var permissions = await _roleManager.GetClaimsAsync(role);

            var response = new RoleDeatilResponse(role.Id, role.Name!, role.IsDeleted, permissions.Select(x => x.Value));

            return Result.Success(response);
        }


        public async Task<Result<RoleDeatilResponse>> AddAsync(RoleRequest request)
        {
            var roleIsExist = await _roleManager.RoleExistsAsync(request.Name);
            if (roleIsExist)
                return Result.Failure<RoleDeatilResponse>(RoleErrors.DuplicatedRole);

            var allowedPermissions = Permissions.GetAllPermission();

            if (request.Permissions.Except(allowedPermissions).Any())
                return Result.Failure<RoleDeatilResponse>(RoleErrors.InavlidRole);

            var role = new ApplicationRole
            {
                Name = request.Name,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                var permissions = request.Permissions.Select(x => new IdentityRoleClaim<string>
                {
                    ClaimType = Permissions.Type,
                    ClaimValue = x,
                    RoleId = role.Id
                });

                await _context.RoleClaims.AddRangeAsync(permissions);
                await _context.SaveChangesAsync();
                var response = new RoleDeatilResponse(role.Id, role.Name!, role.IsDeleted, request.Permissions);
                return Result.Success(response);
            }

            var error = result.Errors.First();
            return Result.Failure<RoleDeatilResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));


        }

        public async Task<Result> UpdateAsync(string id, RoleRequest request)
        {
            var roleIsExist = await _roleManager.Roles.AnyAsync(x => x.Name == request.Name && x.Id != id);
            if (roleIsExist)
                return Result.Failure<RoleDeatilResponse>(RoleErrors.DuplicatedRole);
            if (await _roleManager.FindByIdAsync(id) is not { } role)
                return Result.Failure<RoleDeatilResponse>(RoleErrors.InavlidRole);

            var allowedPermissions = Permissions.GetAllPermission();

            if (request.Permissions.Except(allowedPermissions).Any())
                return Result.Failure<RoleDeatilResponse>(RoleErrors.InavlidRole);

            role.Name = request.Name;


            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                var currentPermissions = await _context.RoleClaims.Where(x => x.RoleId == role.Id && x.ClaimType == Permissions.Type).Select(x => x.ClaimValue).ToListAsync();

                var newPermissions = request.Permissions.Except(currentPermissions).Select(x => new IdentityRoleClaim<string>
                {
                    ClaimType = Permissions.Type,
                    ClaimValue = x,
                    RoleId = role.Id
                });

                var removedPermissions = currentPermissions.Except(request.Permissions);

                await _context.RoleClaims.Where(x => x.RoleId == role.Id && removedPermissions.Contains(x.ClaimValue)).ExecuteDeleteAsync();

                await _context.RoleClaims.AddRangeAsync(newPermissions);
                await _context.SaveChangesAsync();
                return Result.Success();
            }

            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));


        }

        public async Task<Result> ToggleStatusAsync(string id)
        {
            if (await _roleManager.FindByIdAsync(id) is not { } role)
                return Result.Failure<RoleDeatilResponse>(RoleErrors.InavlidRole);
            role.IsDeleted = !role.IsDeleted;
            await _context.SaveChangesAsync();
            return Result.Success();

        }

    }
}

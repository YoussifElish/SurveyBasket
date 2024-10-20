
using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using SurveyBasket.Authentication;
using SurveyBasket.Helpers;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace SurveyBasket.Services
{
    public class AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider, SignInManager<ApplicationUser> signInManager, ILogger<AuthService> logger, IEmailSender emailSender, IHttpContextAccessor httpContextAccessor, ApplicationDbContext context) : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IJwtProvider _jwtProvider = jwtProvider;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly ILogger<AuthService> _logger = logger;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ApplicationDbContext _context = context;
        private readonly int _refreshTokenExpiryDays = 14;

        public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken)
        {

            if (await _userManager.FindByEmailAsync(email) is not { } user)
                return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

            if (user.IsDisabled)
                return Result.Failure<AuthResponse>(UserErrors.DisabledUser);

            var result = await _signInManager.PasswordSignInAsync(user, password, false, true);

            if (result.Succeeded)
            {
                var (userRoles, userPermissions) = await GetUserRolesAndPermissions(user, cancellationToken);

                var (token, ExpiresIn) = _jwtProvider.GenerateToken(user, userRoles, userPermissions);

                var refreshToken = GenerteRefreshToken();
                var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

                user.RefreshTokens.Add(new RefreshToken
                {
                    Token = refreshToken,
                    ExpiresOn = refreshTokenExpiration,
                });
                await _userManager.UpdateAsync(user);
                var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, ExpiresIn, refreshToken, refreshTokenExpiration);
                return Result.Success<AuthResponse>(response);
            }

            var error = result.IsNotAllowed ? UserErrors.EmailNotConfirmed : result.IsLockedOut ? UserErrors.LockedUser : UserErrors.InvalidCredentials;

            return Result.Failure<AuthResponse>(error);

        }
        public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken)
        {
            var userId = _jwtProvider.ValidateToken(token);

            if (userId is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidToken);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Failure<AuthResponse>(UserErrors.InavlidUser);
            if (user.IsDisabled)
                return Result.Failure<AuthResponse>(UserErrors.DisabledUser);
            if (user.LockoutEnd > DateTime.UtcNow)
                return Result.Failure<AuthResponse>(UserErrors.LockedUser);



            var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

            if (userRefreshToken is null)
                return Result.Failure<AuthResponse>(UserErrors.InavlidUser);

            userRefreshToken.RevokedOn = DateTime.UtcNow;

            var (userRoles, userPermissions) = await GetUserRolesAndPermissions(user, cancellationToken);

            var (newToken, ExpiresIn) = _jwtProvider.GenerateToken(user, userRoles, userPermissions);

            var newRefreshToken = GenerteRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                ExpiresOn = refreshTokenExpiration,
            });
            await _userManager.UpdateAsync(user);
            var result = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, newToken, ExpiresIn, newRefreshToken, refreshTokenExpiration);
            return Result<AuthResponse>.Success(result);

        }
        public async Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken)
        {
            var userId = _jwtProvider.ValidateToken(token);

            if (userId is null)
                return Result.Failure(UserErrors.InvalidToken);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Failure(UserErrors.InavlidUser);

            var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

            if (userRefreshToken is null)
                return Result.Failure(UserErrors.InavlidUser);

            userRefreshToken.RevokedOn = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return Result.Success();
        }


        public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
        {
            var emailIsExist = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);
            if (emailIsExist)
                return Result.Failure(UserErrors.DuplicatedEmail);
            var user = request.Adapt<ApplicationUser>();
            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                _logger.LogInformation("Confirmation Code: {Code}", code);
                await SendConfimartionEmail(user, code);


                return Result.Success();
            }

            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));


        }


        public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request)
        {
            if (await _userManager.FindByIdAsync(request.UserId) is not { } user)
                return Result.Failure(UserErrors.InvalidCode);

            if (user.EmailConfirmed)
                return Result.Failure(UserErrors.DuplicatedConfirmation);


            var code = request.Code;
            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            }
            catch (FormatException)
            {
                return Result.Failure(UserErrors.InvalidCode);
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, DefaultRoles.Member);
                return Result.Success();
            }
            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

        }

        public async Task<Result> ResendConfirmEmailAsync(ResendConfirmationEmailRequest request)
        {
            if (await _userManager.FindByEmailAsync(request.Email) is not { } user)
                return Result.Success();

            if (user.EmailConfirmed)
                return Result.Failure(UserErrors.DuplicatedConfirmation);


            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            _logger.LogInformation("Confirmation Code: {Code}", code);
            await SendConfimartionEmail(user, code);


            return Result.Success();

        }

        public async Task<Result> SentResetPasswordCodeAsync(string email)
        {
            if (await _userManager.FindByEmailAsync(email) is not { } user)
                return Result.Success();
            if (!user.EmailConfirmed)
                return Result.Failure(UserErrors.EmailNotConfirmed);

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            _logger.LogInformation("Reset Code: {Code}", code);
            await SendResetPasswordEmail(user, code);
            return Result.Success();

        }
        public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null || !user.EmailConfirmed)
                return Result.Failure(UserErrors.InvalidCode);
            IdentityResult result;
            try
            {
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
                result = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);

            }
            catch (FormatException)
            {
                result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
            }

            if (result.Succeeded)
                return Result.Success();
            var error = result.Errors.First();
            return Result.Failure(new(error.Code, error.Description, StatusCodes.Status401Unauthorized));


        }

        private static string GenerteRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }



        private async Task SendResetPasswordEmail(ApplicationUser user, string code)
        {
            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;
            var emailBody = EmailBodyBuider.GenerateEmailBody("ForgetPassword", new Dictionary<string, string>
                {
                    {"{{Product_Name}}",user.FirstName},
                    {"{{name}}",user.FirstName},
                    {"{{action_url}}",$"{origin}/auth/forgetPassword?Email={user.Email}&code={code}"}
                });
            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "✅Survey Basket: Reset Password", emailBody));

            await Task.CompletedTask;

        }
        private async Task SendConfimartionEmail(ApplicationUser user, string code)
        {
            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;
            var emailBody = EmailBodyBuider.GenerateEmailBody("EmailConfirmation", new Dictionary<string, string>
                {
                    {"{{name}}",user.FirstName},
                    {"{{action_url}}",$"{origin}/auth/emailConfirmation?userId={user.Id}&code={code}"}
                });
            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "✅Survey Basket: Email Confirmation", emailBody));

            await Task.CompletedTask;

        }
        private async Task<(IEnumerable<string> roles, IEnumerable<string> permissions)> GetUserRolesAndPermissions(ApplicationUser user, CancellationToken cancellationToken)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            //var userPermissions = await _context.Roles.Join(_context.RoleClaims, role => role.Id, claim => claim.RoleId, (role, claim) => new { role, claim }).Where(x => userRoles.Contains(x.role.Name!)).Select(x => x.claim.ClaimValue!).Distinct().ToListAsync(cancellationToken);

            var userPermissions = await (from r in _context.Roles
                                         join p in _context.RoleClaims
                                         on r.Id equals p.RoleId
                                         where userRoles.Contains(r.Name!) && r.IsDeleted == false
                                         select p.ClaimValue!
                ).Distinct().ToListAsync(cancellationToken);

            return (userRoles, userPermissions);

        }

    }
}

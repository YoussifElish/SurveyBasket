using Microsoft.AspNetCore.RateLimiting;

namespace SurveyBasket.Controllers

{
    [Route("[controller]")]
    [ApiController]
    [EnableRateLimiting("ipLimit")]

    public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly ILogger<AuthController> _logger = logger;

        [HttpPost("")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Logging with Email :{email} and password :{password}", request.email, request.password);
            var authResult = await _authService.GetTokenAsync(request.email, request.password, cancellationToken);
            return authResult.IsSuccess ? Ok(authResult.Value) : authResult.ToProblem();


        }



        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var authResult = await _authService.GetRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);
            return authResult.IsFailure ? BadRequest("Invalid Token") : Ok(authResult.Value);

        }


        [HttpPut("revoke-refresh-token")]
        public async Task<IActionResult> RevokeRefreshTokenAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var isRevoked = await _authService.RevokeRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);
            return isRevoked.IsSuccess ? Ok() : BadRequest(UserErrors.OperationFailed);

        }
        [HttpPost("register")]

        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var authResult = await _authService.RegisterAsync(request, cancellationToken);
            return authResult.IsSuccess ? Ok() : authResult.ToProblem();

        }
        [HttpPost("confirm-email")]

        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            var authResult = await _authService.ConfirmEmailAsync(request);
            return authResult.IsSuccess ? Ok() : authResult.ToProblem();

        }

        [HttpPost("resend-confirmation-email")]

        public async Task<IActionResult> ResendConfirmationMail([FromBody] ResendConfirmationEmailRequest request)
        {
            var authResult = await _authService.ResendConfirmEmailAsync(request);
            return authResult.IsSuccess ? Ok() : authResult.ToProblem();

        }

        [HttpPost("forget-password")]

        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
        {
            var authResult = await _authService.SentResetPasswordCodeAsync(request.Email);
            return authResult.IsSuccess ? Ok() : authResult.ToProblem();

        }

        [HttpPost("reset-password")]

        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var authResult = await _authService.ResetPasswordAsync(request);
            return authResult.IsSuccess ? Ok() : authResult.ToProblem();

        }



    


    }
}

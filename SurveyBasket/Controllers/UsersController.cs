﻿
using SurveyBasket.Contracts.Users;

namespace SurveyBasket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpGet("")]
        [HasPermission(Permissions.GetUsers)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {

            return Ok(await _userService.GetAllAsync(cancellationToken));
        }


        [HttpGet("{id}")]
        [HasPermission(Permissions.GetUsers)]
        public async Task<IActionResult> Get([FromRoute] string id, CancellationToken cancellationToken)
        {

            var result = await _userService.GetAsync(id, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }


        [HttpPost("")]
        [HasPermission(Permissions.AddUsers)]
        public async Task<IActionResult> Add([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
        {

            var result = await _userService.AddAsync(request, cancellationToken);
            return result.IsSuccess ? CreatedAtAction(nameof(Get), new { result.Value.Id }, result.Value) : result.ToProblem();
        }


        [HttpPut("{id}")]
        [HasPermission(Permissions.AddUsers)]
        public async Task<IActionResult> Add(string id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
        {

            var result = await _userService.UpdateAsync(id, request, cancellationToken);
            return result.IsSuccess ? NoContent() : result.ToProblem();
        }


        [HttpPut("{id}/toggle-status")]
        [HasPermission(Permissions.UpdateUsers)]
        public async Task<IActionResult> Toggle(string id, CancellationToken cancellationToken)
        {

            var result = await _userService.ToggleStatusAsync(id, cancellationToken);
            return result.IsSuccess ? NoContent() : result.ToProblem();
        }

        [HttpPut("{id}/unlock")]
        [HasPermission(Permissions.UpdateUsers)]
        public async Task<IActionResult> Unlock(string id, CancellationToken cancellationToken)
        {

            var result = await _userService.UnlockAsync(id, cancellationToken);
            return result.IsSuccess ? NoContent() : result.ToProblem();
        }

    }


}

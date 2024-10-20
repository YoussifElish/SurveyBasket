﻿using SurveyBasket.Contracts.Polls;

namespace SurveyBasket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PollsController(IPollService pollService) : ControllerBase
    {
        private readonly IPollService _pollService = pollService;

        [HttpGet("")]
        [HasPermission(Permissions.GetPolls)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            return Ok(await _pollService.GetAllAsync(cancellationToken));
        }

        [HttpGet("current")]
        [Authorize(Roles = DefaultRoles.Member)]

        public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
        {
            return Ok(await _pollService.GetCurrentAsync(cancellationToken));
        }


        [HttpGet("{id}")]
        [HasPermission(Permissions.GetPolls)]

        public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await _pollService.GetAsync(id, cancellationToken);

            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }



        [HttpPost("")]
        [HasPermission(Permissions.AddPolls)]

        public async Task<IActionResult> Add([FromBody] PollRequest request, CancellationToken cancellationToken)
        {

            var result = await _pollService.AddAsync(request, cancellationToken);

            return result.IsSuccess ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value) : result.ToProblem();

        }

        [HttpPut("{id}")]
        [HasPermission(Permissions.UpdatePolls)]

        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PollRequest request, CancellationToken cancellationToken)
        {

            var result = await _pollService.UpdateAsync(id, request, cancellationToken);
            return result.IsSuccess ? NoContent() : result.ToProblem();


        }

        [HttpDelete("{id}")]
        [HasPermission(Permissions.DeltePolls)]

        public async Task<IActionResult> Delete([FromRoute] int id)
        {

            var isDeleted = await _pollService.DeleteAsync(id);
            if (isDeleted.IsFailure)
                return NotFound(PollErrors.PollNotFound);
            return NoContent();
        }

        [HttpPut("{id}/togglePublish")]
        [HasPermission(Permissions.DeltePolls)]
        public async Task<IActionResult> TogglePublish([FromRoute] int id, CancellationToken cancellationToken)
        {

            var isUpdated = await _pollService.TogglePublishStatusAsync(id, cancellationToken);
            if (isUpdated.IsFailure)
                return NotFound(PollErrors.PollNotFound);
            return NoContent();
        }
    }

}

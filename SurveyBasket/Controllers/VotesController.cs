using Microsoft.AspNetCore.RateLimiting;
using SurveyBasket.Contracts.Votes;

namespace SurveyBasket.Controllers

{
    [Route("api/polls/{pollId}/vote")]
    [ApiController]
    [Authorize(Roles = DefaultRoles.Member)]
    [EnableRateLimiting("concurrency")]
    public class VotesController(IQuestionService questionService, IVoteService voteService) : ControllerBase
    {
        private readonly IQuestionService _questionService = questionService;
        private readonly IVoteService _voteService = voteService;

        [HttpGet("")]
        public async Task<IActionResult> start([FromRoute] int pollId, CancellationToken cancellationToken)
        {
            var userId = "0a0df7f2-9eb0-4627-aeea-328378225fd4";// User.GetUserId();

            var result = await _questionService.GetAvailableAsync(pollId, userId!, cancellationToken);

            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }

        [HttpPost()]

        public async Task<IActionResult> Vote([FromRoute] int pollId, [FromBody] VoteRequest request, CancellationToken cancellationToken)
        {
            var result = await _voteService.AddAsync(pollId, User.GetUserId()!, request, cancellationToken);
            return result.IsSuccess ? Created() : result.ToProblem();
        }

    }
}

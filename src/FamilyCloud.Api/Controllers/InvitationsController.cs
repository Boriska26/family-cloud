using FamilyCloud.Application.Dtos;
using FamilyCloud.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyCloud.Api.Controllers;

[ApiController]
[Route("api/invitations")]
public class InvitationsController : ControllerBase
{
    private readonly AcceptInvitationService _acceptInvitationService;

    public InvitationsController(AcceptInvitationService acceptInvitationService)
    {
        _acceptInvitationService = acceptInvitationService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<string>> Create(
        [FromBody] CreateInvitationRequestDto createInvitationRequestDto,
        [FromServices] CreateInvitationService createInvitationService,
        CancellationToken cancellationToken)
    {
        string token = await createInvitationService.CreateAsync(createInvitationRequestDto.PersonId, cancellationToken);

        return Ok(new CreateInvitationResponseDto(token));
    }

    [HttpPost("accept")]
    [AllowAnonymous]
    public async Task<IActionResult> Accept(
        [FromBody] AcceptInvitationRequestDto acceptInvitationRequestDto,
        CancellationToken cancellationToken)
    {
        await _acceptInvitationService.AcceptAsync(acceptInvitationRequestDto, cancellationToken);

        return NoContent();
    }
}
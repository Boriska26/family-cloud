using FamilyCloud.Application.Dtos;
using FamilyCloud.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyCloud.Api.Controllers;

[ApiController]
[Route("api/persons")]
public class PersonController : ControllerBase
{
    private readonly PersonService _personService;

    public PersonController(PersonService personService)
    {
        _personService = personService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PersonDto>> CreateAsync(
        [FromBody] PersonRequestDto personRequestDto,
        CancellationToken cancellationToken)
    {
        var person = await _personService.CreateAsync(personRequestDto, cancellationToken);

        var result = new CreatedResult($"/api/persons/{person.Id}", person);

        return result;
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<PersonDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var person = await _personService.GetByIdAsync(id, cancellationToken);
        if (person == null)
        {
            return NotFound("Персона не найдена");
        }

        return Ok(person);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<PersonDto>> Update(
        Guid id,
        [FromBody] PersonUpdateDto personUpdateDto,
        CancellationToken cancellationToken)
    {
        PersonDto personDto = await _personService.UpdateAsync(id, personUpdateDto, cancellationToken);

        return Ok(personDto);
    }
}
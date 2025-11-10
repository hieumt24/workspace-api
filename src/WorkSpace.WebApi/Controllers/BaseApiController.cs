using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WorkSpace.WebApi.Controllers;
[ApiController]
[Route("api/v1/")]
public abstract class BaseApiController : Controller
{
    private IMediator _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
}
using WorkSpace.Application.DTOs.Email;

namespace WorkSpace.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendAsync(EmailRequest request);
}
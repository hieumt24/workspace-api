using MediatR;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Bookings.Commands;

public class ConfirmPaymentCommand : IRequest<Response<bool>>
{
    public required string BookingCode { get; set; }
    public required string PaymentProvider { get; set; }
    public required string PaymentRef { get; set; }
    public decimal PaidAmount { get; set; }
    public string Currency { get; set; } = "VND";
}
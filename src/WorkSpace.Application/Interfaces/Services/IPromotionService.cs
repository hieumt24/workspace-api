using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Services;

public interface IPromotionService
{
    /// <summary>
    /// Validates and applies promotion code to calculate discount
    /// </summary>
    /// <param name="promotionCode">Promotion code to apply</param>
    /// <param name="userId">User applying the promotion</param>
    /// <param name="totalAmount">Total amount before discount</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple with (IsValid, DiscountAmount, Promotion, ErrorMessage)</returns>
    Task<(bool IsValid, decimal DiscountAmount, Promotion? Promotion, string? ErrorMessage)> 
        ValidateAndCalculateDiscountAsync(
            string promotionCode, 
            int userId, 
            decimal totalAmount, 
            CancellationToken cancellationToken = default);
    Task RecordPromotionUsageAsync(int promotionId, int bookingId, int userId, decimal discountAmount, CancellationToken cancellationToken = default);
}


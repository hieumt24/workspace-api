using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Services;

public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _promotionRepo;
    private readonly WorkSpaceContext _context;

    public PromotionService(IPromotionRepository promotionRepo, WorkSpaceContext context)
    {
        _promotionRepo = promotionRepo;
        _context = context;
    }

    public async Task<(bool IsValid, decimal DiscountAmount, Promotion? Promotion, string? ErrorMessage)> 
        ValidateAndCalculateDiscountAsync(
            string promotionCode, 
            int userId, 
            decimal totalAmount, 
            CancellationToken cancellationToken = default)
    {
       
        var promotion = await _context.Promotions
            .FirstOrDefaultAsync(p => p.Code == promotionCode, cancellationToken);

        if (promotion == null)
        {
            return (false, 0, null, "Promotion code not found.");
        }

       
        if (!promotion.IsActive)
        {
            return (false, 0, null, "Promotion is not active.");
        }

       
        var now = DateTime.UtcNow;
        if (now < promotion.StartDate)
        {
            return (false, 0, null, "Promotion has not started yet.");
        }

        if (now > promotion.EndDate)
        {
            return (false, 0, null, "Promotion has expired.");
        }

      
        if (promotion.UsageLimit > 0 && promotion.UsedCount >= promotion.UsageLimit)
        {
            return (false, 0, null, "Promotion usage limit reached.");
        }

  
        decimal discountAmount = 0;

        if (promotion.DiscountType?.ToLower() == "percent")
        {
            
            discountAmount = totalAmount * (promotion.DiscountValue / 100);
        }
        else if (promotion.DiscountType?.ToLower() == "amount")
        {
         
            discountAmount = promotion.DiscountValue;
        }
        else
        {
            return (false, 0, null, "Invalid promotion discount type.");
        }

       
        if (discountAmount > totalAmount)
        {
            discountAmount = totalAmount;
        }

       
        if (discountAmount < 0)
        {
            discountAmount = 0;
        }

        return (true, discountAmount, promotion, null);
    }

    public async Task RecordPromotionUsageAsync(
        int promotionId, 
        int bookingId, 
        int userId, 
        decimal discountAmount, 
        CancellationToken cancellationToken = default)
    {
      
        var promotionUsage = new PromotionUsage
        {
            PromotionId = promotionId,
            BookingId = bookingId,
            UserId = userId,
            DiscountAmount = discountAmount,
            UsedAt = DateTime.UtcNow
        };

        await _context.PromotionUsages.AddAsync(promotionUsage, cancellationToken);

      
        var promotion = await _context.Promotions.FindAsync(new object[] { promotionId }, cancellationToken);
        if (promotion != null)
        {
            promotion.UsedCount++;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}


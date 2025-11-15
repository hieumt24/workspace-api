//using AutoMapper;
//using MediatR;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using WorkSpace.Application.Interfaces.Repositories;
//using WorkSpace.Application.Interfaces.Services;
//using WorkSpace.Application.Wrappers;
//using WorkSpace.Domain.Entities;

//namespace WorkSpace.Application.Features.Bookings.Commands;

//public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Response<int>>
//{
//    private readonly IBookingRepository _bookingRepo;
//    private readonly IAvailabilityService _availability;
//    private readonly IBookingPricingService _pricing;
//    private readonly IPromotionService _promotionService;
//    private readonly IBlockedTimeSlotRepository _blockedTimeSlotRepo;
//    private readonly UserManager<AppUser> _userManager;
//    private readonly IMapper _mapper;


//    public CreateBookingCommandHandler(
//        IBookingRepository bookingRepo,
//        IAvailabilityService availability,
//        IBookingPricingService pricing,
//        IPromotionService promotionService,
//        IBlockedTimeSlotRepository blockedTimeSlotRepo,
//        UserManager<AppUser> userManager,
//        IMapper mapper)
//    {
//        _bookingRepo = bookingRepo;
//        _availability = availability;
//        _pricing = pricing;
//        _promotionService = promotionService;
//        _blockedTimeSlotRepo = blockedTimeSlotRepo;
//        _userManager = userManager;
//        _mapper = mapper;
//    }


//    public async Task<Response<int>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
//    {
//        var m = request.Model;

//        // Update user profile if new info provided
//        if (!string.IsNullOrWhiteSpace(m.FirstName) || !string.IsNullOrWhiteSpace(m.LastName) ||
//            !string.IsNullOrWhiteSpace(m.PhoneNumber))
//        {
//            var user = await _userManager.FindByIdAsync(m.CustomerId.ToString());
//            if (user != null)
//            {
//                bool needsUpdate = false;

//                if (!string.IsNullOrWhiteSpace(m.FirstName) && string.IsNullOrWhiteSpace(user.FirstName))
//                {
//                    user.FirstName = m.FirstName;
//                    needsUpdate = true;
//                }

//                if (!string.IsNullOrWhiteSpace(m.LastName) && string.IsNullOrWhiteSpace(user.LastName))
//                {
//                    user.LastName = m.LastName;
//                    needsUpdate = true;
//                }

//                if (!string.IsNullOrWhiteSpace(m.PhoneNumber) && string.IsNullOrWhiteSpace(user.PhoneNumber))
//                {
//                    user.PhoneNumber = m.PhoneNumber;
//                    needsUpdate = true;
//                }

//                if (needsUpdate)
//                {
//                    await _userManager.UpdateAsync(user);
//                }
//            }
//        }

//        var available =
//            await _availability.IsAvailableAsync(m.WorkspaceId, m.StartTimeUtc, m.EndTimeUtc, cancellationToken);
//        if (!available) return new Response<int>("Time range is not available.");

//        var quote = await _pricing.QuoteAsync(m.WorkspaceId, m.StartTimeUtc, m.EndTimeUtc, m.NumberOfParticipants,
//            cancellationToken);

//        // Apply promotion if provided
//        decimal discountAmount = 0;
//        Promotion? appliedPromotion = null;
//        var finalAmount = quote.FinalAmount;

//        if (!string.IsNullOrWhiteSpace(m.PromotionCode))
//        {
//            var (isValid, discount, promotion, errorMessage) = await _promotionService
//                .ValidateAndCalculateDiscountAsync(m.PromotionCode, m.CustomerId, quote.FinalAmount, cancellationToken);

//            if (!isValid)
//            {
//                return new Response<int>(errorMessage ?? "Invalid promotion code.");
//            }

//            discountAmount = discount;
//            appliedPromotion = promotion;
//            finalAmount = quote.FinalAmount - discountAmount;

//            // Ensure final amount is not negative
//            if (finalAmount < 0) finalAmount = 0;
//        }

//        var booking = new Booking
//        {
//            BookingCode = $"BK-{Guid.NewGuid().ToString("N")[..10].ToUpper()}",
//            CustomerId = m.CustomerId,
//            WorkSpaceRoomId = m.WorkspaceId,
//            StartTimeUtc = m.StartTimeUtc,
//            EndTimeUtc = m.EndTimeUtc,
//            NumberOfParticipants = m.NumberOfParticipants,
//            SpecialRequests = m.SpecialRequests,
//            Currency = m.Currency,
//            TotalPrice = quote.TotalPrice,
//            TaxAmount = quote.TaxAmount,
//            ServiceFee = quote.ServiceFee,
//            FinalAmount = finalAmount,
//            BookingStatusId = 1 // PendingPayment,
//        };

//        //var model = await _bookingRepo.AddAsync(booking, cancellationToken);

//        // IMPORTANT: Block time slot immediately to prevent double booking
//        await _blockedTimeSlotRepo.CreateBlockedTimeSlotForBookingAsync(
//            m.WorkspaceId,
//            3,
//            m.StartTimeUtc,
//            m.EndTimeUtc,
//            cancellationToken);

//        // Record promotion usage if applied
//        if (appliedPromotion != null)
//        {
//            await _promotionService.RecordPromotionUsageAsync(
//                appliedPromotion.Id,
//                3,
//                m.CustomerId,
//                discountAmount,
//                cancellationToken);
//        }

//        var message = discountAmount > 0
//            ? $"Booking created with {discountAmount:N0} VND discount applied. Complete payment to confirm."
//            : "Booking created with PendingPayment. Complete payment to confirm.";

//        return new Response<int>(3, message);
//    }
//}
using MediatR;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Reviews.Commands.CreateReview
{
    public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<Review> _reviewRepository;
        private readonly IGenericRepositoryAsync<Booking> _bookingRepository;
        private readonly IDateTimeService _dateTimeService;

        public CreateReviewCommandHandler(
            IGenericRepositoryAsync<Review> reviewRepository,
            IGenericRepositoryAsync<Booking> bookingRepository,
            IDateTimeService dateTimeService)
        {
            _reviewRepository = reviewRepository;
            _bookingRepository = bookingRepository;
            _dateTimeService = dateTimeService;
        }

        public async Task<Response<int>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
        
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);

            if (booking == null)
            {
                throw new ApiException($"Booking with ID {request.BookingId} not found.");
            }

            if (booking.CustomerId != request.UserId)
            {
                throw new ApiException("You are not authorized to review this booking.");
            }

            if (booking.IsReviewed)
            {
                throw new ApiException("This booking has already been reviewed.");
            }

            var review = new Review
            {
                BookingId = booking.Id, 
                UserId = request.UserId,
                WorkSpaceRoomId = booking.WorkSpaceRoomId,
                Rating = request.Dto.Rating,
                Comment = request.Dto.Comment, 
                IsVerified = false,
                IsPublic = true,
                CreateUtc = _dateTimeService.NowUtc
            };

            await _reviewRepository.AddAsync(review, cancellationToken);

            booking.IsReviewed = true;
            await _bookingRepository.UpdateAsync(booking, cancellationToken);

            return new Response<int>(review.Id, "Review created successfully.");
        }
    }
}
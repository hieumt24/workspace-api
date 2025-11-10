// src/WorkSpace.Application/Features/Reviews/Commands/ModerateReviewCommand.cs
using MediatR;
using Microsoft.EntityFrameworkCore; 
using WorkSpace.Application.Exceptions; 
using WorkSpace.Application.Interfaces.Repositories; 
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities; 
namespace WorkSpace.Application.Features.Reviews.Commands;

public class ModerateReviewCommand : IRequest<Response<int>>
{
    public int ReviewId { get; set; }
    public bool IsVerified { get; set; }
    public bool IsPublic { get; set; }
}

public class ModerateReviewCommandHandler : IRequestHandler<ModerateReviewCommand, Response<int>>
{
    private readonly IGenericRepositoryAsync<Review> _reviewRepository;

    public ModerateReviewCommandHandler(IGenericRepositoryAsync<Review> reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Response<int>> Handle(ModerateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);

        if (review == null)
        {
            throw new ApiException($"Review with ID {request.ReviewId} not found.");
        }

        review.IsVerified = request.IsVerified;
        review.IsPublic = request.IsPublic;

        try
        {
            await _reviewRepository.UpdateAsync(review, cancellationToken);
            return new Response<int>(review.Id, "Review status updated successfully.");
        }
        catch (DbUpdateException ex)
        {

            throw new ApiException($"Failed to update review status. Error: {ex.InnerException?.Message ?? ex.Message}");
        }
    }
}
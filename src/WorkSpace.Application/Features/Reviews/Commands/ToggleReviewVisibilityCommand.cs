using MediatR;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Reviews.Commands;

public record ToggleReviewVisibilityCommand(int ReviewId) : IRequest<Response<bool>>;

public class ToggleReviewVisibilityCommandHandler : IRequestHandler<ToggleReviewVisibilityCommand, Response<bool>>
{
    private readonly IGenericRepositoryAsync<Review> _reviewRepository;

    public ToggleReviewVisibilityCommandHandler(IGenericRepositoryAsync<Review> reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Response<bool>> Handle(ToggleReviewVisibilityCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);

        if (review == null)
        {
            throw new ApiException($"Review with ID {request.ReviewId} not found.");
        }


        review.IsPublic = !review.IsPublic;

        await _reviewRepository.UpdateAsync(review, cancellationToken);

        string statusMessage = review.IsPublic ? "Visible (Shown)" : "Hidden";
        return new Response<bool>(review.IsPublic, $"Review is now {statusMessage}.");
    }
}
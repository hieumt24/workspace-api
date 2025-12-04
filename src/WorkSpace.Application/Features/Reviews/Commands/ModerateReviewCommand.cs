using MediatR;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Reviews.Commands;

public record ApproveReviewCommand(int ReviewId) : IRequest<Response<int>>;


public record HideReviewCommand(int ReviewId) : IRequest<Response<int>>;

public record ShowReviewCommand(int ReviewId) : IRequest<Response<int>>;


public class ReviewModerationHandler :
    IRequestHandler<ApproveReviewCommand, Response<int>>,
    IRequestHandler<HideReviewCommand, Response<int>>,
    IRequestHandler<ShowReviewCommand, Response<int>>
{
    private readonly IGenericRepositoryAsync<Review> _reviewRepository;

    public ReviewModerationHandler(IGenericRepositoryAsync<Review> reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }


    public async Task<Response<int>> Handle(ApproveReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await GetReviewById(request.ReviewId, cancellationToken);

        review.IsVerified = true;
  

        await _reviewRepository.UpdateAsync(review, cancellationToken);
        return new Response<int>(review.Id, "Đã duyệt đánh giá thành công.");
    }


    public async Task<Response<int>> Handle(HideReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await GetReviewById(request.ReviewId, cancellationToken);

        review.IsPublic = false;

        await _reviewRepository.UpdateAsync(review, cancellationToken);
        return new Response<int>(review.Id, "Đã ẩn đánh giá thành công.");
    }


    public async Task<Response<int>> Handle(ShowReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await GetReviewById(request.ReviewId, cancellationToken);

        review.IsPublic = true;

        await _reviewRepository.UpdateAsync(review, cancellationToken);
        return new Response<int>(review.Id, "Đã bỏ ẩn (hiển thị) đánh giá thành công.");
    }

    private async Task<Review> GetReviewById(int id, CancellationToken ct)
    {
        var review = await _reviewRepository.GetByIdAsync(id, ct);
        if (review == null)
        {
            throw new ApiException($"Không tìm thấy đánh giá với ID {id}.");
        }
        return review;
    }
}
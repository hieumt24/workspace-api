using MediatR;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Promotions.Commands.ActivatePromotion
{
    public class ActivatePromotionCommand : IRequest<Response<bool>>
    {
        public int PromotionId { get; set; }
        public int RequestUserId { get; set; }
        public bool IsOwnerAction { get; set; }
    }

    public class ActivatePromotionCommandHandler : IRequestHandler<ActivatePromotionCommand, Response<bool>>
    {
        private readonly IPromotionRepository _promotionRepo;
        private readonly IHostProfileAsyncRepository _hostRepo;

        public ActivatePromotionCommandHandler(IPromotionRepository promotionRepo, IHostProfileAsyncRepository hostRepo)
        {
            _promotionRepo = promotionRepo;
            _hostRepo = hostRepo;
        }

        public async Task<Response<bool>> Handle(ActivatePromotionCommand request, CancellationToken cancellationToken)
        {
            var promotion = await _promotionRepo.GetByIdAsync(request.PromotionId, cancellationToken);
            if (promotion == null) throw new ApiException("Không tìm thấy mã khuyến mãi.");

            if (request.IsOwnerAction)
            {
                var hostProfile = await _hostRepo.GetHostProfileByUserId(request.RequestUserId, cancellationToken);
                if (hostProfile == null || promotion.HostId != hostProfile.Id)
                {
                    throw new ApiException("Bạn không có quyền kích hoạt mã này.");
                }
            }
            else
            {
                if (promotion.HostId != null) throw new ApiException("Admin chỉ được kích hoạt mã hệ thống.");
            }

            promotion.IsActive = true;
            await _promotionRepo.UpdateAsync(promotion, cancellationToken);

            return new Response<bool>(true, "Đã kích hoạt.");
        }
    }
}
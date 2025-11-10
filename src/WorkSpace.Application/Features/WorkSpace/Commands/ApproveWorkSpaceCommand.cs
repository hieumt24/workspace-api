using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.WorkSpace.Commands;

public record ApproveWorkSpaceCommand(ApproveWorkSpaceDto Model) : IRequest<Response<bool>>;

public class ApproveWorkSpaceHandler(IWorkSpaceRepository repository) 
    : IRequestHandler<ApproveWorkSpaceCommand, Response<bool>>
{
    public async Task<Response<bool>> Handle(ApproveWorkSpaceCommand request, CancellationToken cancellationToken)
    {
        var workSpace = await repository.GetByIdAsync(request.Model.WorkSpaceId, cancellationToken);
        
        if (workSpace == null)
        {
            return new Response<bool>($"Không tìm thấy workspace với Id {request.Model.WorkSpaceId}");
        }

        if (request.Model.IsApproved)
        {
            workSpace.IsVerified = true;
            workSpace.IsActive = true;
        }
        else
        {
            workSpace.IsVerified = false;
            workSpace.IsActive = false;
            // Có thể lưu RejectionReason vào log hoặc notification nếu cần
        }

        workSpace.LastModifiedUtc = DateTime.UtcNow;

        try
        {
            await repository.UpdateAsync(workSpace, cancellationToken);
            
            var message = request.Model.IsApproved 
                ? "Duyệt workspace thành công" 
                : "Từ chối workspace thành công";
                
            return new Response<bool>(true, message);
        }
        catch (DbUpdateException ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return new Response<bool>($"Cập nhật workspace thất bại. {msg}");
        }
    }
}


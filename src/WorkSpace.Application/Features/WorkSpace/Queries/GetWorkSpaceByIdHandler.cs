
using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.WorkSpace.Queries
{
    public record GetWorkSpaceByIdQuery(int Id) : IRequest<Response<WorkSpaceDetailDto>>;

    public class GetWorkSpaceByIdHandler(IWorkSpaceRepository repository,
        IMapper mapper) : IRequestHandler<GetWorkSpaceByIdQuery, Response<WorkSpaceDetailDto>>
    {
        public async Task<Response<WorkSpaceDetailDto>> Handle(GetWorkSpaceByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
            if (entity == null)
            {
                return new Response<WorkSpaceDetailDto>($"WorkSpace with Id {request.Id} not found.");
            }
            var dto = mapper.Map<WorkSpaceDetailDto>(entity);
            return new Response<WorkSpaceDetailDto>(dto);
        }
    }
}
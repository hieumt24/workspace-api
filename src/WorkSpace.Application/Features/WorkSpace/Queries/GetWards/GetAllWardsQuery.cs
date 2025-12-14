using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces;

namespace WorkSpace.Application.Features.WorkSpace.Queries.GetWards;


public record GetAllWardsQuery : IRequest<IEnumerable<string>>;

public class GetAllWardsHandler : IRequestHandler<GetAllWardsQuery, IEnumerable<string>>
{
    private readonly IApplicationDbContext _context;

    public GetAllWardsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<string>> Handle(GetAllWardsQuery request, CancellationToken cancellationToken)
    {
       
        var wards = await _context.Workspaces
            .Include(w => w.Address)
            .Where(w => w.IsActive && w.IsVerified && w.Address != null && !string.IsNullOrEmpty(w.Address.Ward))
            .Select(w => w.Address!.Ward)
            .Distinct()
            .OrderBy(w => w)
            .ToListAsync(cancellationToken);

        return wards ?? new List<string>();
    }
}
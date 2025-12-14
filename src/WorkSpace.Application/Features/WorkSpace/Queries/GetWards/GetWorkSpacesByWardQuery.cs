using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces;
using System.Globalization;
using System.Text;

namespace WorkSpace.Application.Features.WorkSpace.Queries.GetByWard;


public class WorkSpaceSimpleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Address { get; set; }
    public decimal MinPrice { get; set; }
    public string? Thumbnail { get; set; }
    public double Rating { get; set; }
}

public record GetWorkSpacesByWardQuery(string WardName) : IRequest<IEnumerable<WorkSpaceSimpleDto>>;

public class GetWorkSpacesByWardHandler : IRequestHandler<GetWorkSpacesByWardQuery, IEnumerable<WorkSpaceSimpleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWorkSpacesByWardHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkSpaceSimpleDto>> Handle(GetWorkSpacesByWardQuery request, CancellationToken cancellationToken)
    {
     
        var searchTerm = request.WardName.Replace("-", " ").ToLower();

        var query = _context.Workspaces
            .Include(w => w.Address)
            .Include(w => w.WorkSpaceImages)
            .Include(w => w.WorkSpaceRooms)
                .ThenInclude(r => r.Reviews)
            .Where(w => w.IsActive && w.IsVerified && w.Address != null)
            .AsNoTracking();

        var allWorkspaces = await query.ToListAsync(cancellationToken);

        var filteredWorkspaces = allWorkspaces
            .Where(w => w.Address?.Ward != null && RemoveDiacritics(w.Address.Ward.ToLower()).Contains(RemoveDiacritics(searchTerm)))
            .Select(w => new WorkSpaceSimpleDto
            {
                Id = w.Id,
                Title = w.Title,
                Address = $"{w.Address!.Street}, {w.Address.Ward}, {w.Address.State ?? ""}",
                MinPrice = w.WorkSpaceRooms.Any() ? w.WorkSpaceRooms.Min(r => r.PricePerHour) : 0,
                Thumbnail = w.WorkSpaceImages.FirstOrDefault()?.ImageUrl,
                Rating = w.WorkSpaceRooms.SelectMany(r => r.Reviews).Any()
                         ? Math.Round(w.WorkSpaceRooms.SelectMany(r => r.Reviews).Average(r => r.Rating), 1)
                         : 0
            })
            .ToList();

        return filteredWorkspaces;
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}
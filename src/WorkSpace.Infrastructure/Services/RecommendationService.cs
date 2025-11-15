using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Recommendations;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.Infrastructure.Services;

public class RecommendationService : IRecommendationService
{
    private readonly WorkSpaceContext _context;
    public RecommendationService(WorkSpaceContext context)
    {
        _context = context;
    }
    public async Task<UserPreferenceDto> AnalyzeUserPreferencesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var bookings = await _context.Bookings
            .Include(b => b.WorkSpaceRoom)
            .ThenInclude(r => r.WorkSpace)
            .ThenInclude(w => w.Address)
            .Include(b => b.WorkSpaceRoom)
            .ThenInclude(r => r.WorkSpaceRoomAmenities)
            .ThenInclude(a => a.Amenity)
            .Where(b => b.CustomerId == userId)
            .Where(b => b.BookingStatus.Name == "Completed" || b.BookingStatus.Name == "Confirmed")
            .OrderByDescending(b => b.CreateUtc)
            .Take(50)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        if (!bookings.Any())
        {
            return new UserPreferenceDto { UserId = userId };
        }
        var preference = new UserPreferenceDto
        {
            UserId = userId,
            TotalBookings = bookings.Count
        };
        
         // Analyze location preferences
        var wardGroups = bookings
            .Where(b => b.WorkSpaceRoom?.WorkSpace?.Address?.Ward != null)
            .GroupBy(b => b.WorkSpaceRoom.WorkSpace.Address.Ward)
            .OrderByDescending(g => g.Count())
            .ToList();

        preference.FrequentWards = wardGroups.Select(g => g.Key).Take(5).ToList();
        preference.MostFrequentWard = wardGroups.FirstOrDefault()?.Key;

        // Analyze price preferences
        var prices = bookings
            .Select(b => b.WorkSpaceRoom.PricePerDay)
            .Where(p => p > 0)
            .ToList();

        if (prices.Any())
        {
            preference.AveragePricePerDay = prices.Average();
            preference.MinPriceBooked = prices.Min();
            preference.MaxPriceBooked = prices.Max();
        }

        // Analyze capacity preferences
        var capacities = bookings
            .Select(b => b.NumberOfParticipants)
            .Where(c => c > 0)
            .ToList();

        if (capacities.Any())
        {
            preference.AverageCapacity = (int)Math.Ceiling(capacities.Average());
            preference.MaxCapacity = capacities.Max();
        }

        // Analyze amenity preferences
        var amenityCounts = bookings
            .SelectMany(b => b.WorkSpaceRoom.WorkSpaceRoomAmenities)
            .Where(a => a.Amenity != null)
            .GroupBy(a => a.Amenity.Name)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => g.Key)
            .ToList();

        preference.PreferredAmenities = amenityCounts;

        // Analyze workspace type preferences
        preference.PreferredWorkSpaceTypes = bookings
            .Where(b => b.WorkSpaceRoom?.WorkSpace?.WorkSpaceTypeId != null)
            .GroupBy(b => b.WorkSpaceRoom.WorkSpace.WorkSpaceTypeId.Value)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToList();

        // Analyze booking duration
        var durations = bookings
            .Select(b => (b.EndTimeUtc - b.StartTimeUtc).TotalHours)
            .Where(h => h > 0)
            .ToList();

        if (durations.Any())
        {
            preference.AverageBookingDurationHours = (int)Math.Ceiling(durations.Average());
        }

        // Analyze day of week preferences
        preference.PreferredDaysOfWeek = bookings
            .GroupBy(b => b.StartTimeUtc.DayOfWeek)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToList();

        return preference;
    }

    public async Task<(List<RecommendedWorkSpaceDto> Recommendations, int TotalCount)> GetPersonalizedRecommendationsAsync(GetRecommendationsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Analyze user preferences
        var userPreference = await AnalyzeUserPreferencesAsync(request.UserId, cancellationToken);

        // Step 2: Get candidate workspaces
        var query = _context.Workspaces
            .Include(w => w.Address)
            .Include(w => w.Host)
                .ThenInclude(h => h.User)
            .Include(w => w.WorkSpaceType)
            .Include(w => w.WorkSpaceRooms)
                .ThenInclude(r => r.WorkSpaceRoomImages)
            .Include(w => w.WorkSpaceRooms)
                .ThenInclude(r => r.WorkSpaceRoomAmenities)
                    .ThenInclude(a => a.Amenity)
            .Include(w => w.WorkSpaceRooms)
                .ThenInclude(r => r.Reviews)
            .Where(w => w.IsActive && w.IsVerified)
            .Where(w => w.WorkSpaceRooms.Any(r => r.IsActive && r.IsVerified))
            .AsNoTracking()
            .AsQueryable();

        // Apply filters from request
        if (!string.IsNullOrWhiteSpace(request.PreferredWard))
        {
            query = query.Where(w => w.Address.Ward == request.PreferredWard);
        }
        else if (userPreference.MostFrequentWard != null)
        {
            // Prioritize user's frequent location
            query = query.Where(w => 
                w.Address.Ward == userPreference.MostFrequentWard ||
                userPreference.FrequentWards.Contains(w.Address.Ward));
        }

        if (request.DesiredCapacity.HasValue)
        {
            query = query.Where(w => w.WorkSpaceRooms.Any(r => 
                r.Capacity >= request.DesiredCapacity.Value && r.IsActive && r.IsVerified));
        }
        else if (userPreference.AverageCapacity > 0)
        {
            query = query.Where(w => w.WorkSpaceRooms.Any(r => 
                r.Capacity >= userPreference.AverageCapacity && r.IsActive && r.IsVerified));
        }

        if (request.MaxBudgetPerDay.HasValue)
        {
            query = query.Where(w => w.WorkSpaceRooms.Any(r => 
                r.PricePerDay <= request.MaxBudgetPerDay.Value && r.IsActive && r.IsVerified));
        }

        // Filter by availability if time specified
        if (request.DesiredStartTime.HasValue && request.DesiredEndTime.HasValue)
        {
            var start = request.DesiredStartTime.Value;
            var end = request.DesiredEndTime.Value;

            query = query.Where(w => w.WorkSpaceRooms.Any(room =>
                room.IsActive && room.IsVerified &&
                !room.Bookings.Any(b =>
                    b.StartTimeUtc < end && b.EndTimeUtc > start &&
                    (b.BookingStatus.Name == "Confirmed" || 
                     b.BookingStatus.Name == "InProgress" || 
                     b.BookingStatus.Name == "Pending")) &&
                !room.BlockedTimeSlots.Any(slot =>
                    slot.StartTime < end && slot.EndTime > start)
            ));
        }

        var workspaces = await query.ToListAsync(cancellationToken);

        // Step 3: Calculate scores and create recommendations
        var recommendations = new List<RecommendedWorkSpaceDto>();

        foreach (var workspace in workspaces)
        {
            var score = await CalculateWorkspaceScore(workspace, userPreference);
            
            var recommendation = MapToRecommendedDto(workspace, score, userPreference);
            recommendations.Add(recommendation);
        }

        // Step 4: Sort by score and paginate
        var sortedRecommendations = recommendations
            .OrderByDescending(r => r.RecommendationScore)
            .ThenByDescending(r => r.AverageRating)
            .ThenByDescending(r => r.TotalReviews)
            .ToList();

        var totalCount = sortedRecommendations.Count;

        var pagedRecommendations = sortedRecommendations
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return (pagedRecommendations, totalCount);
    }

    public async Task<List<RecommendedWorkSpaceDto>> GetTrendingWorkSpacesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var workspaces = await _context.Workspaces
            .Include(w => w.Address)
            .Include(w => w.Host)
                .ThenInclude(h => h.User)
            .Include(w => w.WorkSpaceType)
            .Include(w => w.WorkSpaceRooms)
                .ThenInclude(r => r.WorkSpaceRoomImages)
            .Include(w => w.WorkSpaceRooms)
                .ThenInclude(r => r.WorkSpaceRoomAmenities)
                    .ThenInclude(a => a.Amenity)
            .Include(w => w.WorkSpaceRooms)
                .ThenInclude(r => r.Reviews)
            .Include(w => w.WorkSpaceRooms)
                .ThenInclude(r => r.Bookings)
            .Where(w => w.IsActive && w.IsVerified)
            .Where(w => w.WorkSpaceRooms.Any(r => r.IsActive && r.IsVerified))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var trendingList = workspaces.Select(w =>
        {
            var recentBookings = w.WorkSpaceRooms
                .SelectMany(r => r.Bookings)
                .Count(b => b.CreateUtc >= thirtyDaysAgo);

            var avgRating = w.WorkSpaceRooms
                .SelectMany(r => r.Reviews)
                .Where(r => r.IsPublic)
                .Select(r => (double)r.Rating)
                .DefaultIfEmpty(0)
                .Average();

            var totalReviews = w.WorkSpaceRooms
                .SelectMany(r => r.Reviews)
                .Count(r => r.IsPublic);

            // Trending score: combines recent bookings, rating, and reviews
            var trendingScore = (recentBookings * 2.0) + (avgRating * 10) + (totalReviews * 0.5);

            return new { Workspace = w, TrendingScore = trendingScore, AvgRating = avgRating, TotalReviews = totalReviews };
        })
        .OrderByDescending(x => x.TrendingScore)
        .Take(count)
        .ToList();

        return trendingList.Select(x => MapToRecommendedDto(
            x.Workspace, 
            x.TrendingScore, 
            null,
            "Trending workspace with high booking volume and ratings"
        )).ToList();
    }

    public async Task<double> CalculateRecommendationScoreAsync(int workspaceId, UserPreferenceDto userPreference,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _context.Workspaces
            .Include(w => w.Address)
            .Include(w => w.WorkSpaceRooms)
            .ThenInclude(r => r.WorkSpaceRoomAmenities)
            .ThenInclude(a => a.Amenity)
            .Include(w => w.WorkSpaceRooms)
            .ThenInclude(r => r.Reviews)
            .FirstOrDefaultAsync(w => w.Id == workspaceId, cancellationToken);

        if (workspace == null)
            return 0;

        return await CalculateWorkspaceScore(workspace, userPreference);
    }
    
    private async Task<double> CalculateWorkspaceScore(
        Domain.Entities.WorkSpace workspace,
        UserPreferenceDto userPreference)
    {
        double score = 0;
        var matchedFeatures = new List<string>();

        // If no user history, use base popularity score
        if (userPreference.TotalBookings == 0)
        {
            var averageRating = workspace.WorkSpaceRooms
                .SelectMany(r => r.Reviews)
                .Where(r => r.IsPublic)
                .Select(r => (double)r.Rating)
                .DefaultIfEmpty(0)
                .Average();

            return averageRating * 20; // Base score from rating
        }

        // 1. Location Match (Weight: 30 points)
        if (workspace.Address?.Ward != null && 
            userPreference.FrequentWards.Contains(workspace.Address.Ward))
        {
            var locationScore = workspace.Address.Ward == userPreference.MostFrequentWard ? 30 : 20;
            score += locationScore;
        }

        // 2. Price Match (Weight: 25 points)
        var avgPricePerDay = workspace.WorkSpaceRooms
            .Where(r => r.IsActive)
            .Select(r => r.PricePerDay)
            .DefaultIfEmpty(0)
            .Average();

        if (avgPricePerDay > 0 && userPreference.AveragePricePerDay > 0)
        {
            var priceDiff = Math.Abs(avgPricePerDay - userPreference.AveragePricePerDay);
            var priceRange = userPreference.MaxPriceBooked - userPreference.MinPriceBooked;
            
            if (priceRange > 0)
            {
                var priceScore = 25 * (1 - Math.Min(priceDiff / priceRange, 1));
                // Convert decimal to double
                score += (double)priceScore;
            }
            else if (priceDiff < userPreference.AveragePricePerDay * 0.2m) // Within 20%
            {
                score += 25;
            }
        }

        // 3. Capacity Match (Weight: 15 points)
        var hasMatchingCapacity = workspace.WorkSpaceRooms
            .Any(r => r.Capacity >= userPreference.AverageCapacity && 
                     r.Capacity <= userPreference.MaxCapacity + 5);

        if (hasMatchingCapacity)
        {
            score += 15;
        }

        // 4. Amenities Match (Weight: 20 points)
        if (userPreference.PreferredAmenities.Any())
        {
            var workspaceAmenities = workspace.WorkSpaceRooms
                .SelectMany(r => r.WorkSpaceRoomAmenities)
                .Where(a => a.Amenity != null)
                .Select(a => a.Amenity.Name)
                .Distinct()
                .ToList();

            var matchedAmenities = userPreference.PreferredAmenities
                .Intersect(workspaceAmenities)
                .Count();

            var amenityScore = (double)matchedAmenities / userPreference.PreferredAmenities.Count * 20;
            score += amenityScore;
        }

        // 5. Workspace Type Match (Weight: 10 points)
        if (workspace.WorkSpaceTypeId.HasValue && 
            userPreference.PreferredWorkSpaceTypes.Contains(workspace.WorkSpaceTypeId.Value))
        {
            score += 10;
        }

        // 6. Rating Bonus (Weight: Up to 20 points)
        var avgRating = workspace.WorkSpaceRooms
            .SelectMany(r => r.Reviews)
            .Where(r => r.IsPublic)
            .Select(r => (double)r.Rating)
            .DefaultIfEmpty(0)
            .Average();

        score += avgRating * 4; // Max 20 points for 5-star rating

        // 7. Review Count Bonus (Weight: Up to 10 points)
        var reviewCount = workspace.WorkSpaceRooms
            .SelectMany(r => r.Reviews)
            .Count(r => r.IsPublic);

        score += Math.Min(reviewCount / 10.0 * 10, 10); // Max 10 points

        return Math.Round(score, 2);
    }

    private RecommendedWorkSpaceDto MapToRecommendedDto(
        Domain.Entities.WorkSpace workspace,
        double score,
        UserPreferenceDto? userPreference,
        string? customReason = null)
    {
        var activeRooms = workspace.WorkSpaceRooms.Where(r => r.IsActive && r.IsVerified).ToList();

        var dto = new RecommendedWorkSpaceDto
        {
            WorkSpaceId = workspace.Id,
            Title = workspace.Title,
            Description = workspace.Description,
            Ward = workspace.Address?.Ward,
            Street = workspace.Address?.Street,
            Latitude = workspace.Address?.Latitude ?? 0,
            Longitude = workspace.Address?.Longitude ?? 0,
            TotalRooms = workspace.WorkSpaceRooms.Count,
            AvailableRooms = activeRooms.Count,
            HostName = workspace.Host?.User?.GetFullName(),
            IsHostVerified = workspace.Host?.IsVerified ?? false,
            RecommendationScore = score
        };

        if (activeRooms.Any())
        {
            dto.MinPricePerDay = activeRooms.Min(r => r.PricePerDay);
            dto.MaxPricePerDay = activeRooms.Max(r => r.PricePerDay);
            dto.AveragePricePerDay = activeRooms.Average(r => r.PricePerDay);
            dto.MinCapacity = activeRooms.Min(r => r.Capacity);
            dto.MaxCapacity = activeRooms.Max(r => r.Capacity);

            var allReviews = activeRooms.SelectMany(r => r.Reviews).Where(r => r.IsPublic).ToList();
            if (allReviews.Any())
            {
                dto.AverageRating = allReviews.Average(r => r.Rating);
                dto.TotalReviews = allReviews.Count;
            }

            dto.AvailableAmenities = activeRooms
                .SelectMany(r => r.WorkSpaceRoomAmenities)
                .Where(a => a.Amenity != null)
                .Select(a => a.Amenity.Name)
                .Distinct()
                .ToList();

            dto.ImageUrls = activeRooms
                .SelectMany(r => r.WorkSpaceRoomImages)
                .Select(img => img.ImageUrl)
                .Distinct()
                .ToList();

            dto.ThumbnailUrl = dto.ImageUrls.FirstOrDefault();
        }

        // Build recommendation reason
        if (customReason != null)
        {
            dto.RecommendationReason = customReason;
        }
        else if (userPreference != null)
        {
            var reasons = new List<string>();

            if (workspace.Address?.Ward == userPreference.MostFrequentWard)
            {
                reasons.Add($"In your favorite area: {workspace.Address.Ward}");
                dto.MatchedFeatures.Add("Preferred Location");
            }

            if (dto.AveragePricePerDay > 0 && userPreference.AveragePricePerDay > 0)
            {
                var priceDiff = Math.Abs(dto.AveragePricePerDay - userPreference.AveragePricePerDay);
                if (priceDiff < userPreference.AveragePricePerDay * 0.2m)
                {
                    reasons.Add("Matches your typical budget");
                    dto.MatchedFeatures.Add("Price Range Match");
                }
            }

            var matchedAmenities = userPreference.PreferredAmenities
                .Intersect(dto.AvailableAmenities)
                .ToList();

            if (matchedAmenities.Any())
            {
                reasons.Add($"Has your preferred amenities: {string.Join(", ", matchedAmenities.Take(3))}");
                dto.MatchedFeatures.Add("Amenities Match");
            }

            if (dto.AverageRating >= 4.5)
            {
                reasons.Add($"Highly rated ({dto.AverageRating:F1}★)");
                dto.MatchedFeatures.Add("High Rating");
            }

            dto.RecommendationReason = reasons.Any() 
                ? string.Join(" • ", reasons) 
                : "Quality workspace matching your preferences";
        }

        return dto;
    }
}
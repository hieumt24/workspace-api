using System.Text.Json;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using WorkSpace.Application.DTOs.AIChatbot;
using WorkSpace.Application.DTOs.Recommendations;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.Infrastructure.Services;

public class AIChatbotService : IAIChatbotService
{
    private readonly IRecommendationService _recommendationService;
    private readonly ChatClient _chatClient;
    private readonly IConfiguration _configuration;

    public AIChatbotService(IRecommendationService recommendationService,
        IConfiguration configuration)
    {
        _recommendationService = recommendationService;
        _configuration = configuration;

        var apiKey = _configuration["OpenAI:ApiKey"];
        _chatClient = new ChatClient("gpt-4o", apiKey);
    }

    public async Task<ChatbotResponseDto> ProcessUserMessageAsync(
        ChatbotRequestDto request,
        CancellationToken cancellationToken = default)
    {
   
        var intent = await ExtractIntentAsync(request.Message, request.UserId, cancellationToken);
        
        var parsedStartTime = intent.GetParsedStartTime();
        var parsedEndTime = intent.GetParsedEndTime();

    
        List<RecommendedWorkSpaceDto>? recommendations = null;

        if (intent.Intent == "search_workspace")
        {
            var recommendRequest = new GetRecommendationsRequestDto
            {
                UserId = request.UserId,
                PreferredWard = intent.Ward,
                DesiredCapacity = intent.Capacity,
                MaxBudgetPerDay = intent.MaxBudget,
                DesiredStartTime = parsedStartTime,
                DesiredEndTime = parsedEndTime,
                RequiredAmenities = intent.Amenities,
                PageNumber = 1,
                PageSize = 10
            };

            var (recommendedWorkspaces, _) = await _recommendationService
                .GetPersonalizedRecommendationsAsync(recommendRequest, cancellationToken);

            recommendations = recommendedWorkspaces;
        }

    
        var responseMessage = await GenerateResponseAsync(
            request.Message,
            intent,
            recommendations,
            request.ConversationHistory,
            cancellationToken);

        return new ChatbotResponseDto
        {
            Message = responseMessage,
            Recommendations = recommendations,
            ExtractedIntent = intent
        };
    }

    public async Task<ExtractedIntentDto> ExtractIntentAsync(
    string userMessage,
    int userId,
    CancellationToken cancellationToken = default)
{
    var userPreferences = await _recommendationService
        .AnalyzeUserPreferencesAsync(userId, cancellationToken);

    var currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    
    var systemPrompt = $@"Bạn là trợ lý AI chuyên phân tích yêu cầu tìm kiếm workspace.
Thời gian hiện tại: {currentDateTime}

Nhiệm vụ: Trích xuất thông tin từ tin nhắn người dùng và trả về JSON.

Thông tin người dùng (để tham khảo):
- Vùng thường đặt: {userPreferences.MostFrequentWard ?? "chưa có"}
- Giá trung bình: {userPreferences.AveragePricePerDay:N0} VND/ngày
- Sức chứa trung bình: {userPreferences.AverageCapacity} người
- Tiện ích ưa thích: {string.Join(", ", userPreferences.PreferredAmenities.Take(5))}

Trả về JSON với format:
{{
  ""intent"": ""search_workspace"",
  ""ward"": ""tên phường/quận hoặc null"",
  ""capacity"": số người hoặc null,
  ""maxBudget"": giá tối đa/ngày hoặc null,
  ""startTime"": ""mô tả thời gian (ví dụ: 'ngày mai 9:00', 'hôm nay 14:00', '2025-01-21 09:00')"",
  ""endTime"": ""mô tả thời gian"",
  ""amenities"": [""WiFi"", ""Máy chiếu""] hoặc []
}}

Quy tắc xử lý thời gian:
- ""ngày mai 9h"" → ""ngày mai 9:00""
- ""hôm nay chiều 2h"" → ""hôm nay 14:00""
- ""tuần sau"" → ""tuần sau 9:00""
- ""21/01/2025 9h"" → ""2025-01-21 09:00""
- Nếu không nói rõ giờ, mặc định 9:00 (sáng)
- Nếu nói ""chiều"" thêm 12 giờ (ví dụ: 2h chiều = 14:00)

Ví dụ:
Input: ""Tôi cần workspace 10 người gần Quận 1 ngày mai từ 9h đến 5h chiều""
Output:
{{
  ""intent"": ""search_workspace"",
  ""ward"": ""Quận 1"",
  ""capacity"": 10,
  ""startTime"": ""ngày mai 9:00"",
  ""endTime"": ""ngày mai 17:00""
}}

Input: ""Tìm phòng họp hôm nay""
Output:
{{
  ""intent"": ""search_workspace"",
  ""startTime"": ""hôm nay 9:00"",
  ""endTime"": ""hôm nay 17:00""
}}

CHỈ trả về JSON, không thêm text khác.";

    var messages = new List<ChatMessage>
    {
        new SystemChatMessage(systemPrompt),
        new UserChatMessage(userMessage)
    };

    var chatCompletionOptions = new ChatCompletionOptions
    {
        Temperature = 0.3f,
        ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
    };

    var completion = await _chatClient.CompleteChatAsync(messages, chatCompletionOptions, cancellationToken);
    var jsonResponse = completion.Value.Content[0].Text;


    var intent = JsonSerializer.Deserialize<ExtractedIntentDto>(
        jsonResponse, 
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
    ) ?? new ExtractedIntentDto();

    return intent;
}

    public Task<GuestChatbotResponseDto> ProcessGuestMessageAsync(GuestChatbotRequestDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async Task<string> GenerateResponseAsync(
        string userMessage,
        ExtractedIntentDto intent,
        List<RecommendedWorkSpaceDto>? recommendations,
        List<ChatbotMessageDto> conversationHistory,
        CancellationToken cancellationToken)
    {
        var systemPrompt = @"Bạn là trợ lý AI thân thiện của hệ thống đặt workspace.
            Nhiệm vụ: Trả lời người dùng về các workspace được đề xuất bằng tiếng Việt tự nhiên, thân thiện.

            Quy tắc:
            1. Nếu có recommendations: Giới thiệu ngắn gọn các workspace phù hợp nhất (2-3 workspace đầu tiên)
            2. Nếu không có recommendations: Hỏi thêm thông tin cần thiết (vị trí, ngày, số người...)
            3. Giọng điệu: Thân thiện, chuyên nghiệp, hữu ích
            4. Format: Ngắn gọn, dễ đọc, có bullet points nếu cần
            5. Luôn kết thúc bằng câu hỏi để khuyến khích tương tác

            KHÔNG đề cập đến điểm số (RecommendationScore), chỉ nói về lý do phù hợp.";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt)
        };

      
        foreach (var msg in conversationHistory.TakeLast(5)) 
        {
            if (msg.Role == "user")
                messages.Add(new UserChatMessage(msg.Content));
            else if (msg.Role == "assistant")
                messages.Add(new AssistantChatMessage(msg.Content));
        }

   
        messages.Add(new UserChatMessage(userMessage));

        if (recommendations?.Any() == true)
        {
            var top3 = recommendations.Take(3).ToList();
            var context = $@"Đã tìm thấy {recommendations.Count} workspace phù hợp. Top 3:

                {string.Join("\n\n", top3.Select((ws, i) => $@"{i + 1}. {ws.Title}
                   - Vị trí: {ws.Ward}, {ws.Street}
                   - Giá: {ws.AveragePricePerDay:N0} VND/ngày
                   - Sức chứa: {ws.MinCapacity}-{ws.MaxCapacity} người
                   - Đánh giá: {ws.AverageRating:F1}★ ({ws.TotalReviews} reviews)
                   - Lý do phù hợp: {ws.RecommendationReason}"))}

                Hãy giới thiệu các workspace này cho người dùng.";

            messages.Add(new SystemChatMessage(context));
        }
        else
        {
            messages.Add(new SystemChatMessage(
                "Không tìm thấy workspace phù hợp. Hỏi người dùng thêm thông tin: vị trí, ngày giờ, số người, ngân sách."));
        }

        var completion = await _chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        return completion.Value.Content[0].Text;
    }
}
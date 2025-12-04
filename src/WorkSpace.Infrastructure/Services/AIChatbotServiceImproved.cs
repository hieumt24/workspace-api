using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using WorkSpace.Application.DTOs.AIChatbot;
using WorkSpace.Application.DTOs.Recommendations;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.Infrastructure.Services;

public class AIChatbotServiceImproved : IAIChatbotService
{
    private readonly IRecommendationService _recommendationService;
    private readonly IChatbotConversationRepository _conversationRepository;
    private readonly ChatClient _chatClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AIChatbotServiceImproved> _logger;

    public AIChatbotServiceImproved(
        IRecommendationService recommendationService,
        IChatbotConversationRepository conversationRepository,
        IConfiguration configuration,
        ILogger<AIChatbotServiceImproved> logger)
    {
        _recommendationService = recommendationService;
        _conversationRepository = conversationRepository;
        _configuration = configuration;
        _logger = logger;

        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }
        
        _chatClient = new ChatClient("gpt-4o", apiKey);
    }

    public async Task<ChatbotResponseDto> ProcessUserMessageAsync(
        ChatbotRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                throw new InvalidMessageException("Message cannot be empty");
            }

            if (request.Message.Length > 5000)
            {
                throw new InvalidMessageException("Message is too long (max 5000 characters)");
            }

            _logger.LogInformation(
                "Processing chatbot message for UserId: {UserId}, ConversationId: {ConversationId}",
                request.UserId, request.ConversationId);

    
            var conversation = request.ConversationId.HasValue
                ? await _conversationRepository.GetChatbotConversationWithMessagesAsync(
                    request.ConversationId.Value, cancellationToken)
                : await _conversationRepository.GetChatbotConversationAsync(
                    request.UserId, cancellationToken);

            if (conversation == null)
            {
                throw new ConversationNotFoundException(request.ConversationId ?? 0);
            }

         
            ExtractedIntentDto intent;
            try
            {
                intent = await ExtractIntentAsync(request.Message, request.UserId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract intent from message: {Message}", request.Message);
                
              
                intent = new ExtractedIntentDto
                {
                    Intent = "general_query"
                };
            }

            var parsedStartTime = intent.GetParsedStartTime();
            var parsedEndTime = intent.GetParsedEndTime();

            List<RecommendedWorkSpaceDto>? recommendations = null;
            
            if (intent.Intent == "search_workspace")
            {
                try
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get recommendations for UserId: {UserId}", request.UserId);
                
                }
            }

          
            await _conversationRepository.AddMessageAsync(
                conversation.Id,
                "user",
                request.Message,
                JsonSerializer.Serialize(intent),
                cancellationToken: cancellationToken);

            var recentMessages = await _conversationRepository.GetRecentMessagesAsync(
                conversation.Id,
                count: 10,
                cancellationToken);

            var conversationHistory = recentMessages
                .OrderBy(m => m.CreateUtc)
                .Select(m => new ChatbotMessageDto
                {
                    Role = m.Role,
                    Content = m.Content,
                    Timestamp = m.CreateUtc.DateTime
                })
                .ToList();

         
            string responseMessage;
            try
            {
                responseMessage = await GenerateResponseAsync(
                    request.Message,
                    intent,
                    recommendations,
                    conversationHistory,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate AI response");
                
                // Fallback response
                responseMessage = recommendations?.Any() == true
                    ? $"Tôi đã tìm thấy {recommendations.Count} workspace phù hợp cho bạn. Vui lòng xem danh sách bên dưới."
                    : "Xin lỗi, tôi đang gặp sự cố kỹ thuật. Vui lòng thử lại sau hoặc cung cấp thêm thông tin về workspace bạn cần tìm.";
            }

            // Step 7: Save assistant message to conversation
            await _conversationRepository.AddMessageAsync(
                conversation.Id,
                "assistant",
                responseMessage,
                recommendationCount: recommendations?.Count,
                cancellationToken: cancellationToken);

            // Step 8: Auto-generate conversation title from first message
            if (conversation.MessageCount == 0 && string.IsNullOrEmpty(conversation.Title))
            {
                var title = GenerateConversationTitle(request.Message);
                await _conversationRepository.UpdateConversationTitleAsync(
                    conversation.Id, title, cancellationToken);
            }

            _logger.LogInformation(
                "Successfully processed message for ConversationId: {ConversationId}",
                conversation.Id);

            return new ChatbotResponseDto
            {
                Success = true,
                Message = responseMessage,
                Recommendations = recommendations,
                ExtractedIntent = intent,
                ConversationId = conversation.Id,
                MessageCount = conversation.MessageCount + 2 // +2 for user + assistant messages just added
            };
        }
        catch (ChatbotException ex)
        {
            _logger.LogWarning(ex, "Chatbot exception: {ErrorCode}", ex.ErrorCode);
            
            return new ChatbotResponseDto
            {
                Success = false,
                ErrorMessage = ex.Message,
                ErrorCode = ex.ErrorCode,
                Message = GetUserFriendlyErrorMessage(ex)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ProcessUserMessageAsync");
            
            return new ChatbotResponseDto
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred",
                ErrorCode = "INTERNAL_ERROR",
                Message = "Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại sau."
            };
        }
    }

    public async Task<ExtractedIntentDto> ExtractIntentAsync(
        string userMessage,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
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

            ChatCompletion completion;
            try
            {
                completion = await _chatClient.CompleteChatAsync(
                    messages, 
                    chatCompletionOptions, 
                    cancellationToken);
            }
            catch (Exception ex)
            {
                throw new OpenAIServiceException(
                    "Failed to call OpenAI API for intent extraction", ex);
            }

            var jsonResponse = completion.Content[0].Text;

            // Parse JSON response with error handling
            ExtractedIntentDto? intent;
            try
            {
                intent = JsonSerializer.Deserialize<ExtractedIntentDto>(
                    jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse intent JSON: {JsonResponse}", jsonResponse);
                throw new IntentExtractionException(
                    "Failed to parse AI response", userMessage, ex);
            }

            return intent ?? new ExtractedIntentDto();
        }
        catch (ChatbotException)
        {
            throw; // Re-throw chatbot exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ExtractIntentAsync");
            throw new IntentExtractionException(
                "Failed to extract intent from message", userMessage, ex);
        }
    }

    private async Task<string> GenerateResponseAsync(
        string userMessage,
        ExtractedIntentDto intent,
        List<RecommendedWorkSpaceDto>? recommendations,
        List<ChatbotMessageDto> conversationHistory,
        CancellationToken cancellationToken)
    {
        try
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

            // Add conversation history (last 5 messages for context)
            foreach (var msg in conversationHistory.TakeLast(5))
            {
                if (msg.Role == "user")
                    messages.Add(new UserChatMessage(msg.Content));
                else if (msg.Role == "assistant")
                    messages.Add(new AssistantChatMessage(msg.Content));
            }

            // Add current context
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

            var completion = await _chatClient.CompleteChatAsync(
                messages, 
                cancellationToken: cancellationToken);
                
            return completion.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            throw new OpenAIServiceException(
                "Failed to generate AI response", ex);
        }
    }

    private string GenerateConversationTitle(string firstMessage)
    {
        // Simple title generation from first message (max 50 chars)
        var title = firstMessage.Length > 50 
            ? firstMessage.Substring(0, 47) + "..." 
            : firstMessage;
            
        return title;
    }

    private string GetUserFriendlyErrorMessage(ChatbotException ex)
    {
        return ex.ErrorCode switch
        {
            "OPENAI_SERVICE_ERROR" => "Xin lỗi, dịch vụ AI tạm thời gặp sự cố. Vui lòng thử lại sau.",
            "INTENT_EXTRACTION_ERROR" => "Tôi không hiểu rõ yêu cầu của bạn. Bạn có thể nói rõ hơn được không?",
            "CONVERSATION_NOT_FOUND" => "Không tìm thấy cuộc hội thoại. Vui lòng bắt đầu cuộc hội thoại mới.",
            "INVALID_MESSAGE" => ex.Message,
            _ => "Đã có lỗi xảy ra. Vui lòng thử lại."
        };
    }
}
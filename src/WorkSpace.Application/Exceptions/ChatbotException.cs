namespace WorkSpace.Application.Exceptions;

public class ChatbotException : Exception
{
    public string ErrorCode { get; set; }
    
    public ChatbotException(string message, string errorCode = "CHATBOT_ERROR") 
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public ChatbotException(string message, Exception innerException, string errorCode = "CHATBOT_ERROR") 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
public class OpenAIServiceException : ChatbotException
{
    public OpenAIServiceException(string message, Exception? innerException = null) 
        : base(message, innerException ?? new Exception(), "OPENAI_SERVICE_ERROR")
    {
    }
}

public class IntentExtractionException : ChatbotException
{
    public string UserMessage { get; set; }
    
    public IntentExtractionException(string message, string userMessage, Exception? innerException = null) 
        : base(message, innerException ?? new Exception(), "INTENT_EXTRACTION_ERROR")
    {
        UserMessage = userMessage;
    }
}


public class ConversationNotFoundException : ChatbotException
{
    public int ConversationId { get; set; }
    
    public ConversationNotFoundException(int conversationId) 
        : base($"Conversation {conversationId} not found or access denied", "CONVERSATION_NOT_FOUND")
    {
        ConversationId = conversationId;
    }
}


public class InvalidMessageException : ChatbotException
{
    public InvalidMessageException(string message) 
        : base(message, "INVALID_MESSAGE")
    {
    }
}
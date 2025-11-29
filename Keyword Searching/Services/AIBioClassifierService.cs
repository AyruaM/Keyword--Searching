using OpenAI;
using OpenAI.Chat;

public class AIBioClassifierService
{
    private readonly ChatClient _chatClient;

    // ChatClient MUST be injected
    public AIBioClassifierService(ChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<bool> IsBioRelatedAsync(string keyword)
    {
        string prompt = $@"
        Classify the following keyword as BIO-RELATED or NOT BIO-RELATED.
        
        BIO-RELATED:
        - Information about a person: age, height, birthday, family, wife, husband, parents, children, net worth, wiki, biography, religion, caste.
        
        NOT BIO-RELATED:
        - Objects, devices, companies, animals, products, etc.
        - Examples: iPhone age, dog birthday, BMW height.
        
        Keyword: ""{keyword}""
        
        Respond ONLY YES or NO.
        ";

        ChatCompletion completion = await _chatClient.CompleteChatAsync(prompt);

        string result = completion.Content[0].Text.Trim().ToUpperInvariant();

        return result == "YES";
    }
}

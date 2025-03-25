using Dapr.AI.Conversation;
using Dapr.Workflow;

namespace MeteringDemoWorkflowApp
{
    public class CallLLM : WorkflowActivity<LLMInput, LLMOutput>
    {
        private readonly ILogger _logger;
        private readonly DaprConversationClient _conversationClient;

        public CallLLM(DaprConversationClient conversationClient, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CallLLM>();
            _conversationClient = conversationClient;
        }

        public override async Task<LLMOutput> RunAsync(WorkflowActivityContext context, LLMInput input)
        {
            _logger.LogInformation("Calling LLM with prompts: {Prompts}.", input.Prompts);
            var conversationInputs = new List<DaprConversationInput>();
            foreach (var prompt in input.Prompts)
            {
                conversationInputs.Add( new (prompt, DaprConversationRole.Generic));
            }
            
            var conversationResponse = await _conversationClient.ConverseAsync(
                Constants.CONVERSATION_COMPONENT,
                conversationInputs);

           return new LLMOutput(conversationResponse.Outputs.Select(o => o.Result), IsSuccess: true);
        }
    }

    public record LLMInput(string[] Prompts, string CustomerId);
    public record LLMOutput(IEnumerable<string> Response, bool IsSuccess, string Message = "");
}
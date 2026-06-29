using ASB.Agent.v1.Implementations;
using ASB.Agent.v1.Interfaces;
using ASB.Agent.v1.Models;
using ASB.Agent.v1.Plugins;
using ASB.Services.v1.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace ASB.Agent.v1;

public static class AgentRegistrationExtensions
{
    public static IServiceCollection AddAgentServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration sections
        services.Configure<OllamaSettings>(configuration.GetSection("Ollama"));
        services.Configure<QdrantSettings>(configuration.GetSection("Qdrant"));

        var ollamaEndpoint = configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
        var chatModel = configuration["Ollama:ChatModel"] ?? "phi4-mini";

        // Register HttpClient for RagService (used for Qdrant + Ollama REST calls)
        services.AddHttpClient<IRagService, RagService>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        // Register Semantic Kernel with Ollama as the LLM provider
        services.AddScoped<Kernel>(sp =>
        {
            var builder = Kernel.CreateBuilder();

            // Add Ollama chat completion
            #pragma warning disable SKEXP0070  // Ollama connector is experimental
            builder.AddOllamaChatCompletion(
                modelId: chatModel,
                endpoint: new Uri(ollamaEndpoint));
            #pragma warning restore SKEXP0070

            // Increase Ollama HTTP timeout for slow CPU inference
            builder.Services.ConfigureHttpClientDefaults(b =>
                b.ConfigureHttpClient(c => c.Timeout = TimeSpan.FromMinutes(10)));


            // Register tool plugins so the LLM can call them
            var userService = sp.GetRequiredService<IUserService>();
            var roleService = sp.GetRequiredService<IRoleService>();
            var policyService = sp.GetRequiredService<IPolicyService>();
            var ragService = sp.GetRequiredService<IRagService>();

            builder.Plugins.AddFromObject(new UserPlugin(userService), "UserTools");
            builder.Plugins.AddFromObject(new RolePlugin(roleService), "RoleTools");
            builder.Plugins.AddFromObject(new PolicyPlugin(policyService), "PolicyTools");
            builder.Plugins.AddFromObject(new KnowledgeBasePlugin(ragService), "KnowledgeBase");

            return builder.Build();
        });

        // Register the agent orchestrator
        services.AddScoped<IAgentService, AgentService>();
        services.AddScoped<IPdfExtractionService, PdfExtractionService>();

        return services;
    }
}

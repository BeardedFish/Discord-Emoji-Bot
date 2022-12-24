using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordEmojiBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordEmojiBot;

internal class Program
{
    private IConfiguration? _config;
    private DiscordSocketClient? _discordSocketClient;
    private InteractionService? _interactionService;

    public void Initialize()
    {
        var _builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(path: "config.json");

        _config = _builder.Build();
    }

    private async Task OnDiscordSocketClientReadyAsync()
    {
#if DEBUG
        foreach (SocketGuild guild in _discordSocketClient?.Guilds!)
        {
            await _interactionService!.RegisterCommandsToGuildAsync(guild.Id);
        }
#else
        await _commands.RegisterCommandsGloballyAsync(true);
#endif
    }

    private ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton(_config)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(serviceProvider => new InteractionService(serviceProvider.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<CommandHandlerService>()
            .BuildServiceProvider();
    }

    public async Task MainAsync()
    {
        Initialize();

        using ServiceProvider services = ConfigureServices();

        _discordSocketClient = services.GetRequiredService<DiscordSocketClient>(); ;
        _interactionService = services.GetRequiredService<InteractionService>();

        _discordSocketClient.Ready += OnDiscordSocketClientReadyAsync;

        string discordAuthToken = _config!["discordToken"] ?? string.Empty;

        await _discordSocketClient.LoginAsync(TokenType.Bot, discordAuthToken);
        await _discordSocketClient.StartAsync();

        await services.GetRequiredService<CommandHandlerService>().InitializeAsync();

        await Task.Delay(Timeout.Infinite);
    }

    public static Task Main() => new Program().MainAsync();
}

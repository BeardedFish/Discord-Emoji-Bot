using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;

namespace DiscordEmojiBot.Services;

public class CommandHandlerService
{
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _serviceProvider;

    public CommandHandlerService(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
    {
        _discordSocketClient = client;
        _interactionService = commands;
        _serviceProvider = services;
    }

    public async Task InitializeAsync()
    {
        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);

        _discordSocketClient.InteractionCreated += HandleInteraction;

        _interactionService.SlashCommandExecuted += SlashCommandExecuted;
        _interactionService.ContextCommandExecuted += ContextCommandExecuted;
        _interactionService.ComponentCommandExecuted += ComponentCommandExecuted;
    }

    private Task ComponentCommandExecuted(ComponentCommandInfo arg1, IInteractionContext arg2, IResult arg3)
    {
        switch (arg3.Error)
        {
            case InteractionCommandError.UnmetPrecondition:
                break;
            case InteractionCommandError.UnknownCommand:
                break;
            case InteractionCommandError.BadArgs:
                break;
            case InteractionCommandError.Exception:
                break;
            case InteractionCommandError.Unsuccessful:
                break;
            default:
                break;
        }

        return Task.CompletedTask;
    }

    private Task ContextCommandExecuted(ContextCommandInfo arg1, IInteractionContext arg2, IResult arg3)
    {
        switch (arg3.Error)
        {
            case InteractionCommandError.UnmetPrecondition:
                break;
            case InteractionCommandError.UnknownCommand:
                break;
            case InteractionCommandError.BadArgs:
                break;
            case InteractionCommandError.Exception:
                break;
            case InteractionCommandError.Unsuccessful:
                break;
            default:
                break;
        }

        return Task.CompletedTask;
    }

    private Task SlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3)
    {
        switch (arg3.Error)
        {
            case InteractionCommandError.UnmetPrecondition:
                break;
            case InteractionCommandError.UnknownCommand:
                break;
            case InteractionCommandError.BadArgs:
                break;
            case InteractionCommandError.Exception:
                break;
            case InteractionCommandError.Unsuccessful:
                break;
            default:
                break;
        }

        return Task.CompletedTask;
    }

    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            SocketInteractionContext socketInteractionContext = new(_discordSocketClient, arg);
            await _interactionService.ExecuteCommandAsync(socketInteractionContext, _serviceProvider);
        }
        catch (Exception ex)
        {
            if (arg.Type == InteractionType.ApplicationCommand)
            {
                await arg.GetOriginalResponseAsync().ContinueWith(async msg => await msg.Result.DeleteAsync());
            }
        }
    }
}

using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text;

namespace DiscordEmojiBot.Services;

public sealed class EmojiCommandServiceModule : InteractionModuleBase<SocketInteractionContext>
{
    public const string EmojiTextFilePath = "emojis.txt";

    private static readonly string[] EmojiList = File.ReadAllLines(EmojiTextFilePath);
    private static readonly string TextFilesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TextFiles");
    private static readonly Random Random = new();

    private CommandHandlerService _commandHandlerService;




    public EmojiCommandServiceModule(CommandHandlerService commandHandlerService)
    {
        _commandHandlerService = commandHandlerService;
    }

    private static int ComputeLevenshteinDistance(string source, string target)
    {
        if (source is null || target is null)
        {
            return 0;
        }

        if ((source.Length == 0) || (target.Length == 0))
        {
            return 0;
        }

        if (source == target)
        {
            return source.Length;
        }

        int sourceWordCount = source.Length;
        int targetWordCount = target.Length;

        if (sourceWordCount == 0)
        {
            return targetWordCount;
        }

        if (targetWordCount == 0)
        {
            return sourceWordCount;
        }

        int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

        for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++);
        for (int i = 0; i <= targetWordCount; distance[0, i] = i++);

        for (int i = 1; i <= sourceWordCount; i++)
        {
            for (int j = 1; j <= targetWordCount; j++)
            {
                int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
            }
        }

        return distance[sourceWordCount, targetWordCount];
    }

    private static double CalculateSimilarity(string source, string target)
    {
        if (source is null || target is null)
        {
            return 0.0;
        }

        if (source.Length == 0 || target.Length == 0)
        {
            return 0.0;
        }

        if (source == target)
        {
            return 1.0;
        }

        int stepsToSame = ComputeLevenshteinDistance(source, target);
        
        return (1.0 - (stepsToSame / (double)Math.Max(source.Length, target.Length)));
    }


    private async Task SendMessageResponse(string text)
    {
        if (text.Length > 2000)
        {
            if (!Directory.Exists(TextFilesDirectory))
            {
                Directory.CreateDirectory(TextFilesDirectory);
            }

            string emojifiedTextFileName = Path.ChangeExtension(Path.GetTempFileName().Split('\\').Last(), ".txt");
            string emojifiedTextFilePath = $@"{TextFilesDirectory}\{emojifiedTextFileName}";

            File.WriteAllText(emojifiedTextFilePath, text);

            await RespondWithFileAsync(emojifiedTextFilePath);

            File.Delete(emojifiedTextFilePath);

            return;
        }

        await RespondAsync(text);
    }

    [SlashCommand("emojify", "Add random emojis to your text. :weary:")]
    public async Task EmojifyText(string text)
    {
        StringBuilder stringBuilder = new();
        string[] words = text.Split();

        for (int i = 0; i < words.Length; i++)
        {
            string currentWord = words[i];
            bool shouldAppendEmoji = Random.Next(0, 2) == 1;

            stringBuilder.Append(currentWord);

            if (currentWord.Length > 3 && shouldAppendEmoji)
            {
                stringBuilder.Append(' ');
                stringBuilder.Append($":{EmojiList[Random.Next(0, EmojiList.Length)]}:");
            }

            if (i < words.Length - 1)
            {
                stringBuilder.Append(' ');
            }
        }

        await SendMessageResponse(stringBuilder.ToString());
    }

    [SlashCommand("english-to-emojis", "Replaces English words with emojis. :eyes:")]
    public async Task EnglishToEmoji(string text)
    {
        StringBuilder stringBuilder = new();
        string[] words = text.Split();

        for (int i = 0; i < words.Length; i++)
        {
            string currentWord = words[i];
            string[] possibleEmojis = EmojiList.Where(emoji => CalculateSimilarity(currentWord.ToLower(), emoji) > 0.70).ToArray();
            
            if (possibleEmojis.Length > 0)
            {
                stringBuilder.Append($":{possibleEmojis[Random.Next(0, possibleEmojis.Length)]}:");
            }
            else
            {
                stringBuilder.Append(currentWord);
            }

            if (i < words.Length - 1)
            {
                stringBuilder.Append(' ');
            }
        }

        await SendMessageResponse(stringBuilder.ToString());
    }

    //[SlashCommand("stats", "I find out your bot stats. :blush:")]

    //public async Task EmojiStats(IUser user)
    //{
    //    var socketUser = (user as SocketGuildUser);
    //    string roleList = String.Join(",\n", socketUser.Roles.Where(x => !x.IsEveryone).Select(x => x.Mention));

    //    var embedBuiler = new EmbedBuilder()
    //    .WithAuthor(user.ToString(), user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
    //    .WithTitle($"{user.Username} Roles")
    //    .WithDescription(roleList)
    //    .WithColor(Color.Green)
    //    .WithCurrentTimestamp();

    //    //stringBuilder.Append(' ');
    //    await RespondAsync(embed: embedBuiler.Build());

    //}

    [SlashCommand("stats", "I find out your bot stats. :blush:")]

    //find a way to get guild info 
    public async Task EmojiStats(IChannel channel)
    {
        StringBuilder stringBuilder = new();
        //var guild = _commandHandlerService as SocketGuild;
        var socketChannel = (channel as SocketChannel);

        
        stringBuilder.Append(channel);
        await SendMessageResponse(stringBuilder.ToString());

    }
}

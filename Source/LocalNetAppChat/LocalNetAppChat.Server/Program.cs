using System.Text.Json;
using CommandLineArguments;
using LocalNetAppChat.Domain.Shared;

var parser = new Parser(
    new ICommandLineOption[] {
        new StringCommandLineOption("--listenOn", "localhost"),
        new Int32CommandLineOption("--port", 5000),
        new BoolCommandLineOption("--https"),
        new StringCommandLineOption("--key", "1234")
    });

if (!parser.TryParse(args, true)) {
    Console.WriteLine("Unfortunately there have been problems with the command line arguments.");
    Console.WriteLine("");
    return;
}

var serverKey = parser.TryGetOptionWithValue<string>("--key");

var messageList = new MessageList();

var hostingUrl = HostingUrlGenerator.GenerateUrl(
    parser.GetOptionWithValue<string>("--listenOn") ?? "",
    parser.GetOptionWithValue<int>("--port"), 
    parser.GetBoolOption("--https"));

var builder = WebApplication.CreateBuilder(new[] { "--urls", hostingUrl });
var app = builder.Build();
    
app.MapGet("/", () => "LocalNetAppChat Server!");

app.MapGet("/receive", (string key, string clientName) =>
{
    if (key != serverKey)
    {
        return "Access Denied";
    }
    
    var messages = messageList.GetMessagesForClient(clientName);
    Console.WriteLine($"- [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] client {clientName} has requested messages... sending {messages.Length} messages");
    return JsonSerializer.Serialize(messages);
});

app.MapPost("/send", (string key, LnacMessage message) =>
{
    if (key != serverKey)
    {
        return "Access Denied";
    }
    // format a timestamp "YYYY-MM-DD HH:MM:SS"
    Console.WriteLine($"- [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] client {message.Name} has sent us a new message...");
    messageList.Add(
        new ReceivedMessage(
            DateTime.Now,
            message
            )
        );
    return "Ok";
});

app.Run();
﻿using LocalNetAppChat.Bot.Plugins.ScriptExecution;
using LocalNetAppChat.Domain.Bots.ClientCommands;
using LocalNetAppChat.Domain.Clientside;
using LocalNetAppChat.Domain.Clientside.ServerApis;
using LocalNetAppChat.Domain.Shared;
using LocalNetAppChat.Domain.Shared.Inputs;
using LocalNetAppChat.Domain.Shared.Outputs;

namespace LocalNetAppChat.Bot
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            IOutput output = new ConsoleOutput();
            IInput input = new ConsoleInput(); 
            
            var parser = new ServerConnectionCommandLineParser();
            var commandLineParametersResult = parser.Parse(args);

            if (!commandLineParametersResult.IsSuccess)
            {
                output.WriteLine("Unfortunately there have been problems with the command line arguments.");
                return;
            }

            var parameters = commandLineParametersResult.Value;

            var apiAccessor = new WebApiServerApiAccessor(
                parameters.Server, parameters.Port, parameters.Https, parameters.IgnoreSslErrors,
                parameters.Key,
                parameters.ClientName
            );
            
            ILnacClient lnacClient = new LnacClient(
                apiAccessor,
                parameters.ClientName);

            var publicClientCommands = new ClientCommandCollection();
            if (!Plugins.DefaultFunctionality.DefaultPlugin.AddCommands(publicClientCommands, args))
            {
                output.WriteLine("Unfortunately there have been problems with the command line arguments.");
            }

            var privateClientCommands = new ClientCommandCollection();

            if (!ScriptExecutionPlugin.AddCommands(privateClientCommands, args))
            {
                output.WriteLine("Unfortunately there have been problems with the command line arguments.");
            }

            while (true)
            {
                try
                {
                    var messages = await lnacClient.GetMessages();
                    
                    foreach (var message in messages)
                    {
                        output.WriteLine(message);

                        if (CommandMessageTokenizer.IsCommandMessage(message.Message.Text)) 
                        {
                            if (IsAPrivateMessage(message))
                            {
                                Result<string> result = privateClientCommands.Execute(message.Message.Text);
                                await SendResultBack(lnacClient, message.Message.Name, result);
                                continue;
                            }

                            if (publicClientCommands.IsAKnownCommand(message.Message.Text))
                            {
                                await SendResultBack(
                                    lnacClient,
                                    message.Message.Name,
                                    publicClientCommands.Execute(message.Message.Text));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    output.WriteLine(e.Message + ": Retry...");
                }
            }
        }

        private async static Task SendResultBack(ILnacClient lnacClient, string sender, Result<string> result)
        {
            if (result.IsSuccess)
            {
                await lnacClient.SendMessage($"/msg {sender} {result.Value}");
                return;
            }

            await lnacClient.SendMessage($"/msg {sender} {result.Error}");
        }

        private static bool IsAPrivateMessage(ReceivedMessage message)
        {
            if (!string.IsNullOrWhiteSpace(message.Receiver))
                return true;
            return false;
        }
    }
}
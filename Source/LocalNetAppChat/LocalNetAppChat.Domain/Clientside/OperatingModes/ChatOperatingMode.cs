using System.Net;
using System.ServiceModel.Channels;
using System.Text.Json;
using LocalNetAppChat.Domain.Clientside.ServerApis;
using LocalNetAppChat.Domain.Shared;
using LocalNetAppChat.Domain.Shared.Inputs;
using LocalNetAppChat.Domain.Shared.Outputs;

namespace LocalNetAppChat.Domain.Clientside.OperatingModes;

public class ChatOperatingMode : IOperatingMode
{
    public bool IsResponsibleFor(ClientSideCommandLineParameters parameters)
    {
        return parameters.Chat;
    }

    public async Task Run(ClientSideCommandLineParameters parameters, IOutput output, ILnacServer lnacServer, IInput input)
    {
        output.WriteLine($"Connecting to server {lnacServer}...");
        while (true)
        {
            try
            {
                var receivedMessages = lnacServer.GetMessages();
                
                foreach (var receivedMessage in receivedMessages)
                {
                    output.WriteLine(receivedMessage);
                }
                
                if (input.IsInputWaiting())
                {
                    var message = input.GetInput();
                    await lnacServer.SendMessage(message);
                }
            }
            catch (Exception e)
            {
                output.WriteLine(e.Message + ": Retry in 1s...");
            }

            Thread.Sleep(1000);
        }
    }
}
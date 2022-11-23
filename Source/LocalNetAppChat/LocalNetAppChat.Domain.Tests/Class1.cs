﻿using NUnit.Framework;

namespace LocalNetAppChat.Domain.Tests;

[TestFixture]
public class MessageListTests
{
    [Test]
    public void We_can_add_Messages_to_the_list()
    {
        var messageList = new MessageList();
        messageList.Add(GetTestMessage());
    }
    
    private static Message GetTestMessage()
    {
        return new Message(Guid.NewGuid().ToString(), "NaseifBigBoss", "HeyThere",
            Array.Empty<string>(),
            true,
            "Message");
    }

    [Test]
    public void We_can_retrieve_messages()
    {
        var messageList = new MessageList();
        messageList.Add(GetTestMessage());

        messagesForClient = messageList.GetMessagesForClient("Blubberbär");

        Assert.IsNotEmpty(messagesForClient);
    }

    [Test]
    public void Calling_retrieve_remembers_the_last_message_that_has_been_retrieved_for_the_client()
    {
        Assert.Fail();
    }
}
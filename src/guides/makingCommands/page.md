# What are commands?

Commands are a way to give orders to the bot what it should do. Bots can of course have its selected list of orders and what they do. Commands are mostly used through messages, but Guilded.NET expands it further to event/document/announcement/media comments and forum replies.

# Creating commands

## Commands in Guilded.NET

Commands in Guilded.NET are methods that are executed once they are invoked through a message or a reply. To identify a command and give set of instructions for it, Guilded.NET uses `Guilded.NET.CommandAttribute`. There can only be one `CommandAttribute` per method and the method has to match delegate `Guilded.NET.CommandMethod<T>`. You can set name of the command, its aliases, usage, description, placement(comments or messages) using `CommandAttribute`. Let's explain what each property does:

- Aliases are alternatives names for the command that can also invoke it.
- Usage is a way to show how the command should be used. It has no use apart from help/command-list commands. Usage kind of has a syntax, but it doesn't need to be followed. Although it is recommended to do so:
    - `<argument name here>` — mandatory argument.
    - `[argument name here]` — optional argument.
- Description shortly describes what command does. It also not used outsides help/command-list command.
- Placement tells Guilded.NET where the command can be used (in the chat or comment section).

Guilded.NET commands work differently than how it works in other libraries though.

For instance, Guilded.NET commands can have formatting, and it'll be ignored (unless that formatting is set to escape the command. Inline code and spoilers escape commands by default). If you do `**!**ping`, `!~~ping~~`, `*!*~~ping~~`, it'll still invoke the `ping` command.

Guilded.NET also splits arguments based on formatting as well, not only by strings that split the command (spaces, tabs, newlines by default). This is because arguments in Guilded.NET aren't strings. Instead, they are leaves, mentions, channel mentions and hyperlinks. Since leaves can't have 2 texts with different formatting, the command arguments get split. That's also the case with hyperlinks and mentions: they don't get clumped together with the normal argument. This means that if you do `!ping **a**__b__[c](https://guilded.gg)`, there are 3 arguments and not 1. This kind of can be confusing and not work as other bots do, but it preserves all the information that has been given by Guilded and doesn't lose it by turning a message into a string. So you don't really lose things like mentions, channel mentions and hyperlinks. It also allows you to ignore formatting in the given arguments and doesn't require additional checking.

## Creating a command

Creating a command is pretty simple. You need to make sure your command has return type `void` and has arguments `BasicGuildedClient`, `CommandInfo`, `MessageCreatedEvent`/`ContentReplyCreatedEvent` (this depends on whether the command is reply/comment based or message based).

Let's explain the arguments:
- `BasicGuildedClient` — the bot client that received the command invoked the method.
- `CommandInfo` — information about command that has been used. It holds prefix that has been used, command name, arguments (leaves, mentions, channel mentions, links) and rest of the message (everything after paragraph that invoked the command. This includes images, code blocks, quote blocks and everything else).
- `MessageCreatedEvent` **__or__** `ContentReplyCreatedEvent` — the event that invoked the command.

# Creating commands

## Ping command

To create a command that does nothing, just create a method that meets the requirements mentioned above and `CommandAttribute`:

```cs
// ...class CommandList...
[Command("ping", "pong", Description = "Responds with `Pong!`")]
public static async void Ping(BasicGuildedClient client, CommandInfo info, MessageCreatedEvent messageCreated)
{
    // What command does goes here
}
```

This creates a command with name `ping` and aliases `pong`. It also sets the description as ``Responds with `Pong!` ``.
Now, the command doesn't actually do anything, so let's respond with `Pong!` whenever someone uses the command. This is how you do exactly that:

```cs
// Sends a message to channel where `ping`/`pong` command was used
await messageCreated.RespondAsync(
    // Generates a new message with content `Pong!`
    Message.Generate("Pong!")
);
```

Let's turn on the bot and type `!ping`. And... it doesn't do anything. That's because we need to make client fetch all the commands from the class where the command methods are. To do this, simply use `client.FetchCommands(typeof(CommandList))` (this method can be used even if the bot hasn't connected yet. The best time to fetch commands is right after the client is created). After you add that, turn on the bot and type `!ping`, it should respond with `Pong!`.

{.success-block}
> We have made the most basic command.

Time to make same command, but for forums and comment section. Command method is pretty much the same, apart from additional property in `Command` attribute, `Placement`. Add `Placement = CommandPlacement.Comments` if you want it to work in the comment section and forum replies. Then, change `MessageCreatedEvent` to `ContentReplyCreatedEvent`. Here's an example:

```cs
[Command("ping", "pong", Description = "Responds with `Pong!`", Placement = CommandPlacement.Comments)]
public static async void PingReply(BasicGuildedClient client, CommandInfo info, ContentReplyCreatedEvent replyCreated)
{
    // Stuff
}
```

Forum reply/comment commands can also match the same name and aliases, and it won't break anything.

Time to make it reply with something. To make it reply, it's a bit different:
```cs
// Sends a reply to the post where the command was invoked
await replyCreated.ReplyAsync(
    // This ignores markdown.
    // `markdown-plain-text` only works with messages, not replies,
    // thus it's recommended to use nodes and leaves for content
    MessageContent.GenerateText("Pong!")
);
```

{.note-block}
> Notice the differences:
> - `RespondAsync` becomes `ReplyAsync`
> - `Message` becomes `MessageContent`
> - `Generate` becomes `GenerateText`

{.warning-block}
> Since `markdown-plain-text` doesn't properly work outside messages, it is recommended to use nodes and leaves instead of it. This means that in `GenerateText` method, markdown will be ignored.

Turn on the bot, create a forum post and reply with `!ping` in that post. The bot should respond with `Pong!`.

## Using arguments

As previously pointed out, arguments aren't strings in Guilded.NET. Instead, they are list of leaves, link nodes, mention nodes, channel mention nodes and emotes. All of those nodes are under umbrella of inline nodes. Block nodes are things like quote blocks, code blocks, images, lists, etc. Arguments won't contain any block nodes, so you don't need to check if it's a block node. These are all under one list with the type `IMessageObject`. `IMessageObject` only contains `Object` property, so you can check if it's a leaf or a node based on that.

To check if it's a leaf, you can do `arg.Object == MsgObject.Leaf`. To check if it's an inline node, you can use `arg.Object == MsgObject.Inline`.

Let's take this example:

```cs
[Command("hello", "hi", "hey", Description = "Says hello to a mentioned user.", Usage = "<user mention>")]
public static async void Hello(BasicGuildedClient client, CommandInfo info, MessageCreatedEvent messageCreated)
{
    // Gets first argument or default
    IMessageObject arg = info.Arguments.FirstOrDefault();
    // Checks if first argument exists and if it's inline node
    if(arg == default || arg?.Object != MsgObject.Inline) return;
    // Casts it as a node, because we already checked if it's inline node
    Node node = (Node)arg;
    // Makes sure that the type of the node is Mention
    if(node.Type != NodeType.Mention) return;
    // Gets it as a mention
    Mention mention = (Mention)node;
    // Checks if it's user mention and not a role or everyone mention.
    if(mention.MentionData.Type != "person") return;
    // Says hi to that user
    await messageCreated.RespondAsync(
        // We are going to use paragraph in this case instead of markdown-plain-text
        ParagraphNode.Generate(
            TextContainer.Generate("Hello, ")
            mention,
            TextContainer.Generate("!")
        )
    );
}
```

Now if we do `!hello a`, the command should be ignored. If we do `!hello @IdkGoodName`, it should respond with `Hello, @IdkGoodName!`.

If you want to get a normal argument, you can check if the argument is a leaf, cast it to `Leaf` and then get `.Text` property.

Channel mentions are separate node type to user, role and here/everyone mentions. It's `ChannelMention` instead. Thread mentions are kind of a thing(don't always work in Guilded official app) and they also fall under `ChannelMention` (at least currently). Hyperlinks aren't leaves. Instead, they are `LinkNode`, that is inline node.

## Rest of the message

You can also get everything after the paragraph that invoked the command. To use it, simply get `.Rest`. With this, you can get quote blocks and use it as 'big arguments' to have arguments across multiple lines. `.Rest` is just a normal `MessageContent` and has nothing else that is fancy in any way.
# Setting up .NET Core project

To start with the bot, we first need to set up our project.

1. Create a new folder. You can name it whatever you want, but for our tutorial, it's going to be called `GuildedNETTest`.
2. Once you create a new folder, either open the folder with git bash, bash or whatever terminal you have. If it doesn't let you open the directory with it, then open that terminal(e.g., CMD) and type `cd "path/to/directory/of/GuildedNETTest"`.
3. After you went there with your terminal, type `dotnet new console`. This will create new C# project with the name of the directory. If you want to name it something else besides directory's name, you can do `dotnet new console -n NameOfTheProject`.

Make sure not to close your terminal/CMD/bash for our next part.

# Installing the Guilded.NET

1. Go over to NuGet and find [Guilded.NET(Or click here)](https://www.nuget.org/packages/Guilded.NET/)[^1].
2. Select how you want to install your package.
3. Copy the given command and execute it with your terminal/bash/CMD.

Give it a little time, and it should install it.

[^1]: [Guilded.NET NuGet](https://www.nuget.org/packages/Guilded.NET/)

# Setting up basic ping pong bot

We are going to make a basic ping bot. You need to create a new account for the bot(Or use existing one). Be sure not to forget your password or email.

## Setup bot config

First thing we need to set up is our config. Create a new `config/config.json` file:
```json
{
    "email": "example@example.com",
    "password": "secretpassword",
    "prefix": "!"
}
```
Make sure you enter correct password and correct email, else the bot won't turn on, because it won't be able to log into that account.
Once we have it set up, go over to `Program.cs`. Add this code to `Main` method in `Program` class:
```cs
// Read JSON "config/config.json"
JObject config = JObject.Parse(File.ReadAllText("./config/config.json"));
// Get login info
string email = config["email"].Value<string>(),
    password = config["password"].Value<string>(),
    prefix = config["prefix"].Value<string>();
// Tells us that it's starting with specific prefix
Console.WriteLine($"Starting the bot with prefix '{prefix}'");
```
This reads our config file and then gets email, password and prefix from it.

## Creating new bot

Now we are going to make a new bot. Make sure the bot is declared in `using`, since it must be disposed at the end:

```cs
// Creates new client
using GuildedUserClient client = new GuildedUserClient(email, password, new GuildedClientConfig(GuildedClientConfig.BasicPrefix(prefix))))
// Stuff here
```

But if we launch the program, it just launches the program, does nothing and instantly stops. This is where you need to make asynchronous method to connect to the Guilded & stop the process from closing. Add this method:

{.note-block}
> If you have started with C# and don't understand what `///` or `//` does, it does literally nothing
> It's meant for explaining, documenting and commenting the code. If you do not like them, you can delete everything after `///` or `//`, including `///` or `//` themselves.

```cs
/// <summary>
/// Makes bot connect to Guilded and then stops it from shutting down.
/// </summary>
/// <param name="client">Client to connect</param>
/// <returns>Async task</returns>
static async Task StartAsync(GuildedUserClient client) {
    // Connects to Guilded
    await client.ConnectAsync();
    // Makes it stop forever, so the bot wouldn't instantly shutdown after connecting
    await Task.Delay(-1);
}
```

Once we have that method, call `StartAsync(client).GetAwaiter().GetResult();`.

{.warning-block}
> Put that method call at the end, not at the start.

Now the bot should log into the account and sit there. It may not show up as online and sometimes shows that it's idle, but it should be ok. Right now, if you want bot to be shown as online, you have to log into the bot's account through Guilded app/Guilded web app, turn the bot on and close Guilded app/Guilded web app.

# Using error and login events

Now, we need to know when bot is really online! Assign a method or lambda to the event `client.Connected`:
```cs
// When client is ready
client.Connected += (o, e) => Console.WriteLine($"I successfully logged in!\n - ID: {client.Me.Id}\n - Name: {client.Me.Username}");
```
Once the bot is connected, it should say:
```
I successfully logged in!
- ID: (ID of the user bot)
- Name: (Name of the user bot)
```
This way, you will know it surely logged on, and you will know its ID.
Now we need to know when an error occurs. Do the same as we did with connection event, but use `client.Error` instead:
```cs
// If client emits any errors
client.Error += (o, e) => Console.WriteLine($"Error [{e.Code}]: {e.ErrorMessage}");
```
Now launch the bot, and it should say that the bot successfully logged in.

# Making commands

Now we need to make commands for this bot. We will create a most basic command — `!ping`.
First, create a new static class. You can name it whatever you want(for example, `CommandList`).
```cs
/// <summary>
/// List of user bot commands.
/// </summary>
public static class CommandList {
    // Commands go here
}
```
Since we have command list, we need to get commands from it. To fetch commands from a type, call `client.FetchCommands` with `CommandList` type as an argument:
```cs
// Fetches all commands from specific type
client.FetchCommands(
    typeof(CommandList)
);
```
Make sure you have put it anywhere above `StartAsync` call.
Now we can add actual `ping` command. Add a new method to `CommandList` with `Command` attribute. Make sure it fits `CommandMethod` delegate, else Guilded.NET will throw an error at you. Here's how to make a new command:
```cs
/// <summary>
/// Responds with `Pong!`
/// </summary>
/// <param name="client">Client to post message with</param>
/// <param name="messageCreated">Message creation event</param>
/// <param name="command">Name of the command used</param>
/// <param name="arguments">Command arguments</param>
[Command("ping", "pong", Description = "Responds with `Pong!`")]
public static void Ping(BasicGuildedClient client, MessageCreatedEvent messageCreated, string command, IList<string> arguments) {
    // Do stuff here
}
```
This will create a new command with the name `ping`, alias `pong` and description ``Responds with `Pong!` ``. If you don't want a description or an alias, you can remove them, but don't remove the name itself.
However, even if the command works, it won't actually do anything. Now we need to respond to new created message using `messageCreated.RespondAsync`:
```cs
// Sends a message to channel where `ping`/`pong` command was used
await messageCreated.RespondAsync(
    // Generates a new message with content `Pong!`
    Message.Generate("Pong!")
);
```
- `!ping` should respond with `Pong!`, because we are invoking the command with name `ping`.
- `!ping abc` should respond with `Pong!`, because we are invoking the command with name `ping` and arguments [`abc`].
- `!pingabc` should NOT respond, because command with name `pingabc` does not exist.
- `!pong` should also respond, because we gave the command alias `pong`.


# Whole code

`Program.cs`:
```cs
using System;
using Newtonsoft.Json.Linq;
using Guilded.NET;
using System.IO;
using System.Threading.Tasks;

namespace GuildedNETTest {
    /// <summary>
    /// User bot client program.
    /// </summary>
    public class Program {
        /// <summary>
        /// Creates a new user bot client.
        /// </summary>
        /// <param name="args">Program arguments</param>
        static void Main(string[] args) {
            // Read JSON "config/config.json"
            JObject config = JObject.Parse(File.ReadAllText("./config/config.json"));
            // Get login info
            string email = config["email"].Value<string>(),
                password = config["password"].Value<string>(),
                prefix = config["prefix"].Value<string>();
            // Tells us that it's starting with specific prefix
            Console.WriteLine($"Starting the bot with prefix '{prefix}'");
            // Creates new client
            using GuildedUserClient client = new GuildedUserClient(email, password, new GuildedClientConfig(GuildedClientConfig.BasicPrefix(prefix)));
            // Fetches all commands from specific type
            client.FetchCommands(
                typeof(CommandList)
            );
            // If client emits any errors
            client.Error += (o, e) => Console.WriteLine($"Error [{e.Code}]: {e.ErrorMessage}");
            // When client is ready
            client.Connected += (o, e) => Console.WriteLine($"I successfully logged in!\n - ID: {client.Me.Id}\n - Name: {client.Me.Username}");
            // Start the bot
            StartAsync(client).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Makes bot connect to Guilded and then stops it from shutting down.
        /// </summary>
        /// <param name="client">Client to connect</param>
        /// <returns>Async task</returns>
        static async Task StartAsync(GuildedUserClient client) {
            // Connects to Guilded
            await client.ConnectAsync();
            // Makes it stop forever, so the bot wouldn't instantly shutdown after connecting
            await Task.Delay(-1);
        }
    }
}
```
`CommandList.cs`:
```cs
using Guilded.NET;
using Guilded.NET.Objects.Events;
using Guilded.NET.Objects.Chat;
using System.Collections.Generic;

namespace GuildedNETTest {
    /// <summary>
    /// List of user bot commands.
    /// </summary>
    public static class CommandList {
        /// <summary>
        /// Responds with Pong!
        /// </summary>
        /// <param name="client">Client to post message with</param>
        /// <param name="messageCreated">Message creation event</param>
        /// <param name="command">Name of the command used</param>
        /// <param name="arguments">Command arguments</param>
        [Command("ping", "pong", Description = "Responds with Pong!")]
        public static void Ping(BasicGuildedClient client, MessageCreatedEvent messageCreated, string command, IList<string> arguments) {
            // Sends a message to channel where `ping`/`pong` command was used
            await messageCreated.RespondAsync(
                // Generates a new message with content `Pong!`
                Message.Generate("Pong!")
            );
        }
    }
}
```
`config/config.json`:
```json
{
    "email": "example@example.com",
    "password": "secretpassword",
    "prefix": "!"
}
```

# The end

That's it! This is how you create a ping bot while using Guilded.NET. It takes a lot... Nevertheless, it still works!
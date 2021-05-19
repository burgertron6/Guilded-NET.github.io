{.note-block}
> I would recommend learning basics of C# before making this bot. You wouldn't be able to do much if you don't learn C#.

Make sure you installed .NET SDK[^1]. .NET 5 or above[^1] are recommended.

[^1]: Download [.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0), [.NET 6](https://dotnet.microsoft.com/download/dotnet/6.0)

## Creating a new bot

Since API isn't public yet, you need to create a new account that is going to be user-bot.

## Installing .NET Core template

### Installing manually

1. Open up your terminal
2. Create new folder where you know you won't touch(E.g., create new directory called `templates/Guilded.NET.Templates` in `dotnet` directory)
3. Go to that folder(`templates/Guilded.NET.Templates` directory) in your terminal
4. Type `git clone https://github.com/Guilded-NET/Guilded.NET.Templates.git .` [^2]
5. Go to `templates/Guilded.NET.Templates/` directory in your terminal
6. Type `dotnet new -i .`
Now you should be able to see new templates in `dotnet new`
[^2]: [Guilded.NET Templates GitHub](https://github.com/Guilded-NET/Guilded.NET.Templates)

### Installing

Type `dotnet new -i Guilded.NET.Templates::0.0.15` and templates should appear in `dotnet new`.

## Creating new project

1. Open up your terminal
2. Create new folder for your bot. Use the name of your bot. E.g., `JoesBot`, `EpicBot`, `DungeonBot`, `BirdBot`, `CatBot`.
3. Go to that folder in your terminal.
4. Type `dotnet new guilded.net.basicuser`. It will use the name of your folder. If you want to name it other way, use `dotnet new guilded.net.basicuser -n NameOfTheBot`
5. Change `config/config.json` file and write email & password of your user bot.

Now launch it with `dotnet run`. It should say that you have successfully logged in. Write `!ping` into the chat and the bot should respond.
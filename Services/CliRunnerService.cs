using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleTools;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Commands;
using Dorbit.Framework.Commands.Abstractions;
using Microsoft.AspNetCore.Builder;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class CliRunnerService
{
    private readonly IEnumerable<ICommand> _commands;

    public CliRunnerService(IEnumerable<ICommand> commands)
    {
        _commands = commands;
    }

    public Task RunAsync(WebApplication app)
    {
        return CreateMenus(app, _commands.Where(x => x.IsRoot));
    }

    private static Task CreateMenus(WebApplication app, IEnumerable<ICommand> commands, bool isRoot = true)
    {
        var menu = new ConsoleMenu(Array.Empty<string>(), level: 0);
        foreach (var command in commands.OrderBy(x => x.Order))
        {
            menu.Add(command.Message, () =>
            {
                var context = new CommandContextCli();
                var parameters = command.GetParameters(context);
                foreach (var parameter in parameters)
                {
                    Console.Write(parameter.Message);
                    if (parameter.DefaultValue is not null)
                    {
                        Console.Write($" (Default: {parameter.DefaultValue})");
                    }

                    Console.Write(":");
                    var input = Console.ReadLine();
                    context.Arguments.Add(parameter.Key, string.IsNullOrEmpty(input) ? parameter.DefaultValue : input);
                }

                var subCommands = command.GetSubCommands(context);
                if (subCommands.Count() > 0)
                {
                    CreateMenus(app, subCommands.ToList(), false);
                }

                try
                {
                    command.InvokeAsync(context);
                }
                catch (Exception ex)
                {
                    context.Error(ex.Message);
                }

                Console.Write("\nEnter any key to continue ...");
                Console.ReadLine();
            });
        }

        if (isRoot)
        {
            menu.Add("Run Webserver", () => app.Run());
        }

        menu.Add(isRoot ? "Exit" : "Back", () => menu.CloseMenu());

        menu.Configure(config =>
        {
            config.Selector = "-> ";
            config.Title = "Main menu";
            config.SelectedItemBackgroundColor = ConsoleColor.DarkCyan;
            config.EnableWriteTitle = true;
        });

        return menu.ShowAsync();
    }
}
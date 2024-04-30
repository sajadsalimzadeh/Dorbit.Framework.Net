using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    private CancellationToken _cancellationToken = default;

    public CliRunnerService(IEnumerable<ICommand> commands)
    {
        _commands = commands;
    }

    public Task RunAsync(WebApplication app)
    {
        return CreateMenus(app, _commands.Where(x => x.IsRoot));
    }

    private Task CreateMenus(WebApplication app, IEnumerable<ICommand> commands, bool isRoot = true)
    {
        var menu = new ConsoleMenu(Array.Empty<string>(), level: 0);
        foreach (var command in commands.OrderBy(x => x.Order))
        {
            menu.Add(command.Message, () => {
                var context = new CommandContextCli();
                var parameters = command.GetParameters(context);
                foreach (var parameter in parameters)
                {
                    context.Log(parameter.Message);
                    if (parameter.DefaultValue is not null)
                    {
                        context.Log($" (Default: {parameter.DefaultValue})");
                    }

                    context.Log(":");
                    var input = Console.ReadLine();
                    context.Arguments.Add(parameter.Key, string.IsNullOrEmpty(input) ? parameter.DefaultValue : input);
                }

                var subCommands = command.GetSubCommands(context).ToList();
                if (subCommands.Count() > 0)
                {
                    CreateMenus(app, subCommands, false).Wait();
                }

                try
                {
                    command.InvokeAsync(context).Wait();
                }
                catch (Exception ex)
                {
                    context.Error(ex.Message);
                }

                context.Log("\nEnter any key to continue ...");
                Console.ReadLine();
                command.AfterEnterAsync(context).Wait();
            });
        }

        if (isRoot)
        {
            menu.Add("Run Webserver", () => app.Run());
        }

        menu.Add(isRoot ? "Exit" : "Back", (m) => m.CloseMenu());

        menu.Configure(config =>
        {
            config.Selector = "-> ";
            config.Title = "Main menu";
            config.SelectedItemBackgroundColor = ConsoleColor.DarkCyan;
            config.EnableWriteTitle = true;
        });

        return menu.ShowAsync(_cancellationToken);
    }
}
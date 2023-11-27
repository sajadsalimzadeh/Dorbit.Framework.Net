using ConsoleTools;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Commands;
using Dorbit.Framework.Commands.Abstractions;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class CliRunnerService
{
    private readonly IEnumerable<ICommand> _commands;

    public CliRunnerService(IEnumerable<ICommand> commands)
    {
        _commands = commands;
    }

    public void Run()
    {
        CreateMenus(_commands.Where(x => x.IsRoot));
    }

    private static void CreateMenus(IEnumerable<ICommand> commands, bool isRoot = true)
    {
        var menu = new ConsoleMenu(Array.Empty<string>(), level: 0);
        foreach (var command in commands)
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
                    CreateMenus(subCommands.ToList(), false);
                }

                try
                {
                    command.Invoke(context);
                }
                catch (Exception ex)
                {
                    context.Error(ex.Message);
                }

                Console.Write("\nEnter any key to continue ...");
                Console.ReadLine();
            });
        }

        menu.Add(isRoot ? "Exit" : "Back", () => Environment.Exit(0))
            .Configure(config =>
            {
                config.Selector = "-> ";
                config.Title = "Main menu";
                config.SelectedItemBackgroundColor = ConsoleColor.DarkCyan;
                config.EnableWriteTitle = true;
            });

        menu.Show();
    }
}
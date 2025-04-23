﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Commands.Abstractions;
using Dorbit.Framework.Contracts.Commands;
using Dorbit.Framework.Extensions;

namespace Dorbit.Framework.Commands;

[ServiceRegister(Order = 99)]
public class ConfigurePostgresCommand : Command
{
    public override string Message => "Configure Postgres";

    public override IEnumerable<CommandParameter> GetParameters(ICommandContext context)
    {
        yield return new CommandParameter("Host", "Host", "127.0.0.1");
        yield return new CommandParameter("Port", "Port", "5432");
        yield return new CommandParameter("DB", "DB Name", "ehsaa");
        yield return new CommandParameter("Username", "Username", "postgres");
        yield return new CommandParameter("Password", "Password");
    }

    public override Task InvokeAsync(ICommandContext context)
    {
        var host = context.GetArgAsString("Host");
        var port = context.GetArgAsInt("Port");
        var db = context.GetArgAsString("DB");
        var username = context.GetArgAsString("Username");
        var password = context.GetArgAsString("Password");

        var connectionString =
            $"host={host};port={port};database={db};user id={username};password={password};pooling=true;minimum Pool Size=1;Maximum Pool Size=20;";

        var appSettingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.custom.json");
        var appSettings = JsonNode.Parse(File.ReadAllText(appSettingPath));
        appSettings["Connection"] = JsonSerializer.SerializeToNode(connectionString.GetEncryptedValue());
        File.WriteAllText(appSettingPath, appSettings.ToJsonString());
        
        context.Log($"\n\"{appSettingPath}\" changed.");
        context.Log("\nTo apply changes, you need to run the program again\n");

        return Task.CompletedTask;
    }

    public override Task AfterEnterAsync(ICommandContext context)
    {
        Environment.Exit(0);
        return base.AfterEnterAsync(context);
    }
}
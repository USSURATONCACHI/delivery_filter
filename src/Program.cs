﻿using log4net.Config;
using log4net;
using System.Reflection;
using System.CommandLine;


namespace DeliveryFilter;

internal class Program
{
    private static readonly ILog LOG = LogManager.GetLogger(typeof(Program));

    static int Main(string[] args)
    {
        if (Assembly.GetEntryAssembly() is Assembly asm) {
            var logRepository = LogManager.GetRepository(asm);
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }
        LOG.Info("Program starts");

        Endpoints ep = new Endpoints();
        RootCommand command = RootCommandBuilder.Build(ep);
        return command.Invoke(args);

    }

}
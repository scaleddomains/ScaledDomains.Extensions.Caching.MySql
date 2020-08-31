using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using MySql.Data.MySqlClient;

namespace ScaledDomains.Extensions.Caching.MySql.Tools
{
    public class Program
    {
        internal static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        internal static ManualResetEventSlim ResetEventSlim = new ManualResetEventSlim();

        public static int Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            var app = new CommandLineApplication
            {
                Name = "mysql-distributed-cache",
                FullName = "MySQL Server Distributed Cache Command Line Tool",
                Description =
                    "Command line tool to create tables and indexes in a MySQL Server database for distributed caching."
            };

            app.HelpOption("-? | -h | --help");

            app.VersionOption("-v | --version",
                () => typeof(Program).GetTypeInfo().Assembly.GetName().Version?.ToString());

            app.Command("create", createCommand =>
            {
                createCommand.Description = app.Description;
                createCommand.ExtendedHelpText =
                    "\nExample usage:\n mysql-distributed-cache create Server=example.com;Database=db;User=myUser;Password=mySecret; myTableName";

                var connectionStringArg = createCommand.Argument(
                    "[connectionString]", "The mysql connection string to connect to the database.");

                var tableNameArg = createCommand.Argument(
                    "[tableName]", "Name of the table to be created.");

                var forceOpt = createCommand.Option("-f | --force", "Force to create the table (if the table is already exist will remove it)",
                    CommandOptionType.NoValue);

                createCommand.HelpOption("-? | -h | --help");

                createCommand.OnExecute(async () =>
                {
                    if (string.IsNullOrWhiteSpace(connectionStringArg.Value) ||
                        string.IsNullOrWhiteSpace(tableNameArg.Value))
                    {
                        createCommand.ShowHelp("create");
                        Console.WriteLine("Invalid argument(s)!");
                        return 1;
                    }

                    Console.WriteLine("Creating cache table with a following parameters:");
                    Console.WriteLine($" {connectionStringArg.Name} : {connectionStringArg.Value}");
                    Console.WriteLine($" {tableNameArg.Name} : {tableNameArg.Value}");

                    try
                    {
                        await MySqlOperation.CreateTable(
                            connectionStringArg.Value,
                            tableNameArg.Value,
                            forceOpt.HasValue(),
                            CancellationTokenSource.Token);

                        Console.WriteLine($"Table ({tableNameArg.Value}) was created successfully and it is ready to use.");

                        return 0;
                    }
                    catch (TaskCanceledException)
                    {
                        Console.Error.WriteLine("Execution was cancelled!");
                    }
                    catch (MySqlException myEx)
                    {
                        Console.Error.WriteLine(myEx.Message);
                    }

                    Console.Error.WriteLine($"Table ({tableNameArg.Value}) was not created!");

                    return 1;
                });
            }, false);

            app.OnExecute(() =>
            {
                app.ShowHint();
                return 1;
            });

            return app.Execute(args);
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Cancelling...");
            CancellationTokenSource.Cancel();
            e.Cancel = true;
        }
    }
}

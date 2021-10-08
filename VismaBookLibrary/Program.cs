using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VismaBookLibrary.DAL;
using VismaBookLibrary.Models;
using VismaBookLibrary.Services;

namespace VismaBookLibrary
{
    class Program
    {
        static async Task Main()
        {
            var commandHandler = new CommandHandler(new UnitOfWork());

            Console.WriteLine("Type a command. For more information type \"help\".");

            string command = "";

            while (command.ToLower() != "exit")
            {
                command = Console.ReadLine();

                var messages = commandHandler.IdentifyCommand(command);

                if (messages != null)
                {
                    var answers = new List<string> { command.Split(' ')[0] };

                    foreach (var message in messages)
                    {
                        Console.Write(message);
                        answers.Add(Console.ReadLine());
                    }

                    await commandHandler.ProcessAnswersAsync(answers);
                }
            }
        }
    }
}

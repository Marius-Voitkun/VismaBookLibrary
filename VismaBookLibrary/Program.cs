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

            string command = "";

            while (command.ToLower() != "exit")
            {
                Console.WriteLine("\nType a command. For more information type \"help\".");
                Console.Write("> ");
                
                command = Console.ReadLine();

                var response = await commandHandler.IdentifyCommandAsync(command);

                if (response.Message != null)
                    Console.WriteLine("\n" + response.Message);

                if (response.NeedsAnswerCollecting == true)
                {
                    Console.WriteLine();
                    var answers = new List<string> { command.Split(' ')[0] };

                    foreach (var request in response.Requests)
                    {
                        Console.Write(request);
                        answers.Add(Console.ReadLine());
                    }

                    var secondaryResponse = await commandHandler.ProcessAnswersAsync(answers);
                    
                    if (secondaryResponse.Message != null)
                        Console.WriteLine("\n" + secondaryResponse.Message);
                }
            }
        }
    }
}

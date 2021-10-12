using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VismaBookLibrary.DAL;

namespace VismaBookLibrary
{
    static class Program
    {
        static async Task Main()
        {
            const short maxLendingPeriodInDays = 60;
            const byte maxNumberOfBooksPerReader = 3;
            
            var commandHandler = new CommandHandler(new UnitOfWork(), maxLendingPeriodInDays, maxNumberOfBooksPerReader);

            var command = "";

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

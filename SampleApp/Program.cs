using SampleApp.Infrastructure;
using SampleApp.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var repository = new SwitchableRepository<string>(new DummyRepositoryA(), new DummyRepositoryB());

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Requesting...");
                Console.ResetColor();

                Console.WriteLine(repository.GetModel());
                Console.ReadLine();
            }
        }
    }
}

using System;
using MIPSSim.Parser;
using MIPSSim.VM;
using System.IO;

namespace MIPSSim
{
    class Program
    {

        static void Main(string[] args)
        {
            VirturalMachine vm = new VirturalMachine();

            var file = File.ReadAllText("/home/shr/codes/courses/Orga/final/mips_sim/code.mips");
            vm.LoadInstructions(file);
            while (vm.Exec()) ;
            Console.WriteLine("Hello World!");
        }
    }
}

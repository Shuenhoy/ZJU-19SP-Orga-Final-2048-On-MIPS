using System;
using MIPSSim.Parser;
using MIPSSim.VM;
using MIPSSim.ASM;
using System.IO;

namespace MIPSSim
{
    class Program
    {

        static void Main(string[] args)
        {
            VirturalMachine vm = new VirturalMachine();

            var file = File.ReadAllText("/home/shr/codes/courses/Orga/final/mips_sim/code.mips");
            var a = ASM.ASM.InstsToCoe(Parser.Parser.Parse(file).insts);
            File.WriteAllText("/home/shr/codes/courses/Orga/final/mips_sim/code.coe", a);
            vm.LoadInstructions(file);
            while (vm.Exec()) ;
            Console.WriteLine("Hello World!");
        }
    }
}

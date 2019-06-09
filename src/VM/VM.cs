using System;
using System.Collections.Generic;
using MIPSSim.Parser;
using System.Runtime.InteropServices;

namespace MIPSSim.VM
{
    public class VirturalMachine
    {
        static string[] regLists = {
            "zero","at",
            "v0","v1",
            "a0","a1","a2","a3",
            "t0","t1","t2","t3","t4","t5","t6","t7",
            "s0","s1","s2","s3","s4","s5","s6","s7",
            "t8","t9",
            "k0","k1",
            "gp",
            "sp",
            "fp",
            "ra"
        };
        public long[] Regs = new long[32];
        Random rand = new Random();
        public void SetReg(int i, long value)
        {
            if (i != 0) Regs[i] = value;
        }
        public void SetReg(string i, long value)
        {
            SetReg(Array.IndexOf(regLists, i), value);
        }
        public long GetReg(string i)
        {
            return Regs[Array.IndexOf(regLists, i)];
        }
        Instruction[] instructions;
        Dictionary<string, int> labels;
        public byte[] RAM = new byte[4096];
        public int PC = 0;

        public void LoadInstructions(Instruction[] insts)
        {
            instructions = insts;
            PC = 0;
        }
        public void ExecSingleInstruction(string inst)
        {
            var oldPc = PC;
            var x = Parser.Parser.ParseLine(inst);
            switch (x)
            {
                case JInstruction j:
                    x = new JInstruction { Key = j.Key, ITarget = labels[j.Target] }.Into;
                    break;
                case IInstruction i:
                    if (i.Key == "beq" || i.Key == "bne")
                    {
                        x = new IInstruction { Key = i.Key, RS = i.RS, RT = i.RT, Imm = labels[i.Label] };
                    }
                    break;
            }
            ExecInst(x);
            if (PC == oldPc + 1) PC--;
        }
        public void SaveWord(int offset, int value)
        {
            var bytes = BitConverter.GetBytes(value);
            for (int j = 0; j < 4; j++)
                RAM[offset + j] = bytes[j];
        }
        public int LoadWord(int offset)
        {
            var ibytes = new byte[4];
            for (int j = 0; j < 4; j++)
                ibytes[j] = RAM[offset + j];
            return BitConverter.ToInt32(ibytes);
        }
        public void LoadInstructions(string inst)
        {
            var (insts, l) = Parser.Parser.Parse(inst);
            labels = l;
            LoadInstructions(insts);
        }
        public int Score = 0;
        public int[] State = new int[16];
        public int Button = 0;
        public bool Exec()
        {
            if (PC >= instructions.Length || PC < 0) return false;
            var inst = instructions[PC];
            ExecInst(inst);
            return true;
        }
        public void ExecInst(Instruction inst)
        {
            PC++;
            switch (inst)
            {
                case IInstruction i:
                    switch (i.Key)
                    {
                        case "addi":
                            SetReg(i.RT, Regs[i.RS] + i.Imm);
                            break;
                        case "lui":
                            SetReg(i.RT, i.Imm << 16);
                            break;
                        case "ori":
                            SetReg(i.RT, Regs[i.RS] | i.Imm);
                            break;
                        case "xori":
                            SetReg(i.RT, Regs[i.RS] ^ i.Imm);
                            break;
                        case "slti":
                            SetReg(i.RT, Regs[i.RS] < (int)i.Imm ? 1 : 0);
                            break;
                        case "bne":
                            if (Regs[i.RS] != Regs[i.RT]) PC = (int)i.Imm;
                            break;
                        case "beq":
                            if (Regs[i.RS] == Regs[i.RT]) PC = (int)i.Imm;
                            break;
                        case "sw":
                            var bytes = BitConverter.GetBytes((int)Regs[i.RT]);
                            for (int j = 0; j < 4; j++)
                                RAM[(int)i.Imm + Regs[i.RS] + j] = bytes[j];
                            break;
                        case "lw":
                            var ibytes = new byte[4];
                            for (int j = 0; j < 4; j++)
                                ibytes[j] = RAM[(int)i.Imm + Regs[i.RS] + j];
                            Regs[i.RT] = BitConverter.ToInt32(ibytes);
                            break;
                        default: throw new NotImplementedException();

                    }
                    break;
                case JInstruction j:
                    switch (j.Key)
                    {
                        case "j":
                            PC = j.ITarget;
                            break;
                        case "jal":
                            SetReg(31, PC);
                            PC = j.ITarget;
                            break;
                        default: throw new NotImplementedException();

                    }
                    break;
                case RInstruction r:
                    switch (r.Key)
                    {
                        case "add":
                            SetReg(r.RD, Regs[r.RS] + Regs[r.RT]);
                            break;
                        case "xor":
                            SetReg(r.RD, Regs[r.RS] ^ Regs[r.RT]);
                            break;
                        case "or":
                            SetReg(r.RD, Regs[r.RS] | Regs[r.RT]);
                            break;
                        case "nor":
                            SetReg(r.RD, ~(Regs[r.RS] | Regs[r.RT]));
                            break;
                        case "sll":
                            SetReg(r.RD, (Regs[r.RT] << r.Shamt));
                            break;
                        case "sllv":
                            SetReg(r.RD, (Regs[r.RS] << (int)Regs[r.RT]));
                            break;
                        case "jr":
                            PC = (int)Regs[31];
                            break;
                        default: throw new NotImplementedException();
                    }
                    break;
                case SInstruction s:
                    switch (s.Key)
                    {
                        case "gen_random":
                            Regs[2] = rand.Next((int)Regs[4] - 1);
                            break;
                        case "update_gpu":
                            var a = ((int)Regs[4]).ToString("X8");
                            var b = ((int)Regs[5]).ToString("X8");
                            Console.WriteLine(a.Substring(0, 4));
                            Console.WriteLine(a.Substring(4, 4));
                            Console.WriteLine(b.Substring(0, 4));
                            Console.WriteLine(b.Substring(4, 4));
                            Console.WriteLine("--------------------");
                            // var bytes1 = BitConverter.GetBytes((int)Regs[4]);
                            // var bytes2 = BitConverter.GetBytes((int)Regs[5]);
                            // for (int j = 0; j < 4; j++)
                            //     State[j] = bytes1[j];
                            // for (int j = 0; j < 4; j++)
                            //     State[j + 4] = bytes2[j];
                            break;
                        case "update_score":
                            Score = (int)Regs[4];
                            Console.WriteLine($"Now Score: {Score}");
                            break;
                        case "button":
                            Regs[2] = Convert.ToInt32(Console.ReadLine());

                            while (Regs[2] < 1 || Regs[2] > 4)
                                Regs[2] = Convert.ToInt32(Console.ReadLine());
                            break;
                        default: throw new NotImplementedException();


                    }
                    break;
            }
            return;
        }
    }
}
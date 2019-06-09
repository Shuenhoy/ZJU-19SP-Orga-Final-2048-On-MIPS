using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using MIPSSim.Parser;

namespace MIPSSim.ASM
{
    static public class ASM
    {
        static Dictionary<string, int> func = new Dictionary<string, int> {
            {"add", 0x20},
            {"xor", 0x26},
            {"or",  0x25},
            {"and", 0x24},
            {"nor", 0x27},
            {"sll", 0x00},
            {"jr",  0x08},
            {"sllv",0x05} // custom
        };


        static Dictionary<string, int> opCode = new Dictionary<string, int> {
            {"addi",        0x08},
            {"ori",         0x0d},
            {"slti",        0x0a},
            {"xori",        0x01}, // custom
            {"bne",         0x05},
            {"beq",         0x04},
            {"j",           0x02},
            {"jal",         0x03},
            {"lw",          0x23},
            {"sw",          0x2b},
            {"update_gpu",  0x07}, // custom
            {"update_score",0x08}, // custom
            {"button",      0x09},  // custom
            {"gen_random",      0x0c}  // custom

        };
        static uint RInst(int RS, int RT, int RD, int Shamt, int Func)
        {
            StringBuilder ret_string = new StringBuilder();
            ret_string.Append("000000");//.PadLeft(5, '0')
            ret_string.Append(Convert.ToString(RS, 2).PadLeft(5, '0'));
            ret_string.Append(Convert.ToString(RT, 2).PadLeft(5, '0'));
            ret_string.Append(Convert.ToString(RD, 2).PadLeft(5, '0'));
            ret_string.Append(Convert.ToString(Shamt, 2).PadLeft(5, '0'));
            ret_string.Append(Convert.ToString(Func, 2).PadLeft(6, '0'));

            return Convert.ToUInt32(ret_string.ToString(), 2);
        }
        static uint IInst(int Op, int RS, int RT, int Imm)
        {
            StringBuilder ret_string = new StringBuilder();
            ret_string.Append(Convert.ToString(Op, 2).PadLeft(6, '0'));

            ret_string.Append(Convert.ToString(RS, 2).PadLeft(5, '0'));
            ret_string.Append(Convert.ToString(RT, 2).PadLeft(5, '0'));
            var imm = Convert.ToString(Imm, 2).PadLeft(16, '0');
            ret_string.Append(imm.Substring(imm.Length - 16));

            return Convert.ToUInt32(ret_string.ToString(), 2);
        }
        static uint JInst(int Op, int Imm)
        {
            StringBuilder ret_string = new StringBuilder();
            ret_string.Append(Convert.ToString(Op, 2).PadLeft(6, '0'));
            ret_string.Append(Convert.ToString(Imm, 2).PadLeft(26, '0'));

            return Convert.ToUInt32(ret_string.ToString(), 2);
        }

        static uint AsmInst(Instruction inst)
        {
            switch (inst)
            {
                case IInstruction i:
                    return IInst(opCode[i.Key], i.RS, i.RT, (int)i.Imm);
                case JInstruction j:
                    return JInst(opCode[j.Key], j.ITarget);
                case RInstruction r:
                    return RInst(r.RS, r.RT, r.RD, r.Shamt, func[r.Key]);
                case SInstruction s:
                    return IInst(opCode[s.Key], 0, 0, 0);
                default: throw new NotImplementedException();
            }
        }
        static public string InstsToCoe(Instruction[] insts)
        {
            return $"memory_initialization_radix = 10;\nmemory_initialization_vector = {String.Join(",", insts.Select(x => AsmInst(x)))};";
        }
    }
}
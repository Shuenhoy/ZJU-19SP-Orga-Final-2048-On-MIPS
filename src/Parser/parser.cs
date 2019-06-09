using System;
using System.Linq;
using System.Collections.Generic;
using LanguageExt;
using LanguageExt.Parsec;
using static LanguageExt.Prelude;
using static LanguageExt.Parsec.Prim;
using static LanguageExt.Parsec.Char;
using static LanguageExt.Parsec.Expr;
using static LanguageExt.Parsec.Token;

namespace MIPSSim.Parser
{
    public static class Parser
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
        public static Parser<Unit> spaces1 = skipMany1(space);
        public static Parser<int> parseReg =
            from _0 in ch('$')
            from name in choice(regLists.Select(x => attempt(str(x))).ToSeq())
            select System.Array.IndexOf(regLists, name);

        public static Parser<string> identifier =
                from x in letter
                from y in many(choice(letter, digit, ch('_')))
                select x + String.Join("", y);

        public static Parser<Instruction> basicR(string key)
            =>
                from _0 in str(key)
                from _1 in spaces1
                from reg1 in parseReg
                from _2 in spaces
                from _3 in ch(',')
                from _4 in spaces
                from reg2 in parseReg
                from _5 in spaces
                from _6 in ch(',')
                from _7 in spaces
                from reg3 in parseReg
                select new RInstruction { Key = key, RD = reg1, RS = reg2, RT = reg3 }.Into;
        public static Parser<Instruction> basicRs =
            choice(Seq("xor", "or", "and", "add", "nor", "sll", "sllv")
                .Select(x => attempt(basicR(x))));

        public static Parser<Instruction> jr =
            from _0 in str("jr")
            from _1 in spaces1
            from reg1 in parseReg
            select new RInstruction { Key = "jr", RS = reg1 }.Into;

        public static Parser<int> intLit =
                from neg in optional(ch('-'))
                from d in asString(many1(digit))
                select Int32.Parse(d) * (neg.IsSome ? -1 : 1);
        public static Parser<int> hexLit =
            from _0 in str("0x")
            from d in asString(many1(either(digit, oneOf("abcdefABCDEF"))))
            select Int32.Parse(d, System.Globalization.NumberStyles.HexNumber);
        public static Parser<int> parseImm = either(attempt(hexLit), intLit);

        public static Parser<Instruction> basicI(string key)
            =>
                from _0 in str(key)
                from _1 in spaces1
                from reg1 in parseReg
                from _2 in spaces
                from _3 in ch(',')
                from _4 in spaces
                from reg2 in parseReg
                from _5 in spaces
                from _6 in ch(',')
                from _7 in spaces
                from imm in parseImm
                select new IInstruction { Key = key, RT = reg1, RS = reg2, Imm = imm }.Into;

        public static Parser<Instruction> sll(string key)
            =>
                from _0 in str(key)
                from _1 in spaces1
                from reg1 in parseReg
                from _2 in spaces
                from _3 in ch(',')
                from _4 in spaces
                from reg2 in parseReg
                from _5 in spaces
                from _6 in ch(',')
                from _7 in spaces
                from imm in parseImm
                select new RInstruction { Key = key, RD = reg1, RT = reg2, Shamt = imm }.Into;

        public static Parser<Instruction> lui
            =
                from _0 in str("lui")
                from _1 in spaces1
                from reg1 in parseReg
                from _2 in spaces
                from _3 in ch(',')
                from _4 in spaces

                from imm in parseImm
                select new IInstruction { Key = "lui", RT = reg1, Imm = imm }.Into;
        public static Parser<Instruction> branch(string key)
                =>
                    from _0 in str(key)
                    from _1 in spaces1
                    from reg1 in parseReg
                    from _2 in spaces
                    from _3 in ch(',')
                    from _4 in spaces
                    from reg2 in parseReg
                    from _5 in spaces
                    from _6 in ch(',')
                    from _7 in spaces
                    from label in identifier
                    select new IInstruction { Key = key, RT = reg1, RS = reg2, Label = label }.Into;
        public static Parser<Instruction> basicIs =
            choice(Seq("addi", "ori", "slti", "xori")
                .Select(x => attempt(basicI(x))));
        public static Parser<Instruction> branches =
            choice(Seq("bne", "beq")
                .Select(x => attempt(branch(x))));
        public static Parser<Instruction> jjal(string key)
            =>
                from _0 in str(key)
                from _1 in spaces1
                from label in identifier
                select new JInstruction { Key = key, Target = label }.Into;
        public static Parser<Instruction> lwsws =
            choice(Seq("lw", "sw")
                .Select(x => attempt(lwsw(x))));
        public static Parser<Instruction> jjals =
            choice(Seq("j", "jal")
                .Select(x => attempt(jjal(x))));
        public static Parser<Instruction> syscalls =
            from key in choice(Seq("gen_random", "update_gpu", "update_score", "button")
                .Select(x => attempt(str(x))))
            select new SInstruction { Key = key }.Into;
        public static Parser<Instruction> lwsw(string key)
            =>
                from _0 in str(key)
                from _1 in spaces1
                from reg1 in parseReg
                from _2 in spaces
                from _3 in ch(',')
                from _4 in spaces
                from imm in parseImm
                from _5 in ch('(')
                from reg2 in parseReg
                from _6 in ch(')')
                select new IInstruction { Key = key, RT = reg1, RS = reg2, Imm = imm }.Into;
        public static Parser<Unit> comment =
            from _0 in ch('#')
            from _1 in many(noneOf('\n'))
            select unit;
        public static Parser<Instruction> inst =
            choice(attempt(basicIs), attempt(jjals), attempt(jr), attempt(lwsws), attempt(sll("sll")), attempt(lui), attempt(syscalls), attempt(basicRs), branches);
        public static Parser<(Option<string> label, Option<Instruction> inst)> oneLine
            = from _00 in many(oneOf(" \n\t"))
              from label in optional(from l in identifier from _ in ch(':') select l)
              from _0 in spaces
              from inst in optional(inst)
              from _1 in spaces
              from _2 in optional(comment)
              from _3 in many(ch('\n'))
              select (label, inst);
        public static Instruction ParseLine(string input)
        {
            return parse(oneLine, input).Reply.Result.inst.First();
        }
        public static (Instruction[] insts, Dictionary<string, int> label) Parse(string input)
        {
            var lines = input.Split('\n');
            var insts = new List<Instruction>();
            var labels = new Dictionary<string, int>();
            foreach (var line in lines)
            {
                var (l, i) = parse(oneLine, line).Reply.Result;
                l.IfSome(x => labels[l.First()] = insts.Length());
                i.IfSome(x => insts.Add(x));
            }
            return (insts.Select(x =>
            {
                switch (x)
                {
                    case JInstruction j:
                        return new JInstruction { Key = j.Key, ITarget = labels[j.Target] }.Into;
                    case IInstruction i:
                        if (i.Key == "beq" || i.Key == "bne")
                        {
                            return new IInstruction { Key = i.Key, RS = i.RS, RT = i.RT, Imm = labels[i.Label] };
                        }
                        else
                        {
                            return x;
                        }
                    default: return x;
                }
            }).ToArray(), labels);
        }


    }
}
using System;
using System.IO;
using Xunit;
using System.Runtime.InteropServices;
using MIPSSim.Parser;
using LanguageExt;
using LanguageExt.Parsec;
using static LanguageExt.Prelude;
using static LanguageExt.Parsec.Prim;
using static LanguageExt.Parsec.Char;
using static LanguageExt.Parsec.Expr;
using static LanguageExt.Parsec.Token;

namespace MIPSSim.Test
{
    public class ParserTest
    {
        [Fact]
        public void Test1()
        {
            Assert.Equal(0, parse(Parser.Parser.parseReg, "$zero").Reply.Result);
            Assert.Equal(1, parse(Parser.Parser.parseReg, "$at").Reply.Result);

        }
        [Fact]
        public void Test2()
        {
            var res = (Parser.IInstruction)parse(Parser.Parser.basicIs, "addi $at, $zero, 1").Reply.Result;
            Assert.Equal("addi",
             res.Key);
            Assert.Equal(1, res.RT);
            Assert.Equal(0, res.RS);
            Assert.Equal(1, res.Imm);

        }
        [Fact]
        public void Test3()
        {
            var res = (Parser.IInstruction)parse(Parser.Parser.oneLine, "addi $a0, $zero, 1").Reply.Result.inst;
            Assert.Equal("addi",
             res.Key);
            Assert.Equal(4, res.RT);
            Assert.Equal(0, res.RS);
            Assert.Equal(1, res.Imm);

        }
        [Fact]
        public void Test4()
        {
            var res = (Parser.IInstruction)parse(Parser.Parser.oneLine, "bne $a0, $zero, test").Reply.Result.inst;
            Assert.Equal("bne",
             res.Key);
            Assert.Equal(4, res.RT);
            Assert.Equal(0, res.RS);
            Assert.Equal("test", res.Label);

        }
    }
}
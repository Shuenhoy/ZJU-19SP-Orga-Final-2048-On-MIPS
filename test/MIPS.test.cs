using System;
using System.IO;
using Xunit;
using System.Runtime.InteropServices;
using MIPSSim.Parser;
using MIPSSim.VM;

namespace MIPSSim.Test
{
    public class MIPSTest
    {
        VirturalMachine vm = new VirturalMachine();
        public MIPSTest()
        {
            var file = File.ReadAllText("/home/shr/codes/courses/Orga/final/mips_sim/code.mips");
            vm.LoadInstructions(file);
            vm.SetReg(31, -100);
        }
        void ApplyMergeCall(int[] input, int[] output, int dir, bool move, int score, bool win)
        {
            int baseAddr = 2048;
            for (int i = 0; i < 16; i++)
            {
                vm.SaveWord(baseAddr + i * 4, input[i]);
            }
            vm.SetReg("s0", dir);
            vm.ExecSingleInstruction("j apply_merge");
            while (vm.Exec()) ;
            int[] o = new int[16];
            for (int i = 0; i < 16; i++)
            {
                o[i] = vm.LoadWord(baseAddr + i * 4);
            }
            Assert.Equal(String.Join(",", output), String.Join(",", o));
            Assert.Equal(win, vm.GetReg("s7") != 0);
            Assert.Equal(move, vm.GetReg("s6") != 0);
            Assert.Equal(score, vm.GetReg("s5"));
        }

        void MergeCall(int v1, int v2, int v3, int v4, int r1, int r2, int r3, int r4,
        bool move, bool win, int score)
        {
            int baseAddr = 2048;

            vm.SaveWord(baseAddr, v1);
            vm.SaveWord(baseAddr + 4, v2);
            vm.SaveWord(baseAddr + 8, v3);
            vm.SaveWord(baseAddr + 12, v4);

            vm.SetReg("s6", 0);
            vm.SetReg("s7", 0);


            vm.ExecSingleInstruction("j merge");
            while (vm.Exec()) ;
            int a1 = vm.LoadWord(baseAddr);
            int a2 = vm.LoadWord(baseAddr + 4);
            int a3 = vm.LoadWord(baseAddr + 8);
            int a4 = vm.LoadWord(baseAddr + 12);

            Assert.Equal(r1, a1);
            Assert.Equal(r2, a2);
            Assert.Equal(r3, a3);
            Assert.Equal(r4, a4);
            Assert.Equal(win, vm.GetReg("s7") != 0);
            Assert.Equal(move, vm.GetReg("s6") != 0);
            Assert.Equal(score, vm.GetReg("s4"));
        }
        void clearMap()
        {

            int baseAddr = 2048;
            for (int i = 0; i < 16; i++)
            {
                vm.SaveWord(baseAddr + i * 4, 0);
            }
        }
        int zeroCount()
        {
            int baseAddr = 2048;
            int count = 0;
            for (int i = 0; i < 16; i++)
            {
                count += vm.LoadWord(baseAddr + i * 4) == 0 ? 1 : 0;
            }
            return count;
        }
        [Fact]
        void UpdateScoreTest()
        {
            vm.SetReg("a0", 3);
            vm.ExecSingleInstruction("update_score");
            Assert.Equal(3, vm.Score);
        }
        [Fact]
        void GenNumTest()
        {
            for (int j = 0; j < 10; j++)
            {
                clearMap();
                Assert.Equal(16, zeroCount());
                for (int i = 15; i >= 0; i--)
                {
                    vm.ExecSingleInstruction("j gen_num");
                    while (vm.Exec()) ;
                    Assert.Equal(i, zeroCount());
                    // vm.ExecSingleInstruction("j load_state_to_reg");
                    // vm.ExecSingleInstruction("update_gpu");

                }
            }


        }
        [Fact]
        void ApplyMergeTest()
        {
            var map1 = new[] {
                    0,0,0,0,
                    1,1,0,0,
                    1,1,1,1,
                    0,0,0,0
                };
            ApplyMergeCall(
                map1,
                new[] {
                    0,0,0,0,
                    1,1,0,0,
                    1,1,1,1,
                    0,0,0,0
                },
                dir: 0, move: false, score: 0, win: false);
            ApplyMergeCall(
               map1,
               new[] {
                    2,2,1,1,
                    0,0,0,0,
                    0,0,0,0,
                    0,0,0,0
               },
               dir: 1, move: true, score: 8, win: false);
            ApplyMergeCall(
               map1,
               new[] {
                    0,0,0,0,
                    0,0,0,0,
                    0,0,0,0,
                    2,2,1,1
               },
               dir: 2, move: true, score: 8, win: false);
            ApplyMergeCall(
               map1,
               new[] {
                    0,0,0,0,
                    2,0,0,0,
                    2,2,0,0,
                    0,0,0,0
               },
               dir: 3, move: true, score: 12, win: false);
            ApplyMergeCall(
               map1,
               new[] {
                    0,0,0,0,
                    0,0,0,2,
                    0,0,2,2,
                    0,0,0,0
               },
               dir: 4, move: true, score: 12, win: false);
            ApplyMergeCall(
                new[] {
                    10,0,0,0,
                    10,0,0,0,
                    1,1,1,1,
                    0,0,0,0
                },
                new[] {
                    11,1,1,1,
                    1,0,0,0,
                    0,0,0,0,
                    0,0,0,0
                },
                dir: 1, move: true, score: 2048, win: true);

        }
        void ApplyLoadStateToReg(params int[] input)
        {
            int baseAddr = 2048;
            for (int i = 0; i < 16; i++)
            {
                vm.SaveWord(baseAddr + i * 4, input[i]);
            }

            vm.ExecSingleInstruction("j load_state_to_reg");
            while (vm.Exec()) ;
            int a = 0, b = 0;
            for (int i = 0; i < 8; i++)
            {
                a = (a << 4) | vm.LoadWord(baseAddr + i * 4);
            }
            for (int i = 0; i < 8; i++)
            {
                b = (b << 4) | vm.LoadWord(baseAddr + i * 4 + 32);
            }
            Assert.Equal(a.ToString("X"), ((int)vm.Regs[4]).ToString("X"));
            Assert.Equal(b.ToString("X"), ((int)vm.Regs[5]).ToString("X"));

        }

        [Fact]
        void TestLoadStateToReg()
        {
            ApplyLoadStateToReg(
                0, 1, 0, 2,
                3, 4, 1, 2,
                0, 5, 1, 2,
                1, 4, 5, 1
            );
            ApplyLoadStateToReg(
                1, 1, 0, 2,
                3, 4, 1, 10,
                0, 0, 1, 2,
                5, 6, 1, 0
            );

            ApplyLoadStateToReg(
                1, 1, 9, 2,
                3, 4, 2, 10,
                0, 0, 8, 2,
                1, 5, 1, 3
            );
        }
        [Fact]
        void TestMerge()
        {
            int baseAddr = 2048;
            vm.SetReg("s0", baseAddr);
            vm.SetReg("s1", baseAddr + 4);
            vm.SetReg("s2", baseAddr + 8);
            vm.SetReg("s3", baseAddr + 12);
            MergeCall(0, 0, 0, 0, 0, 0, 0, 0, win: false, move: false, score: 0);
            MergeCall(0, 1, 0, 0, 1, 0, 0, 0, win: false, move: true, score: 0);
            MergeCall(0, 1, 0, 2, 1, 2, 0, 0, win: false, move: true, score: 0);
            MergeCall(0, 0, 0, 3, 3, 0, 0, 0, win: false, move: true, score: 0);
            MergeCall(1, 1, 0, 0, 2, 0, 0, 0, win: false, move: true, score: 4);
            MergeCall(0, 1, 0, 1, 2, 0, 0, 0, win: false, move: true, score: 4);
            MergeCall(1, 1, 1, 1, 2, 2, 0, 0, win: false, move: true, score: 8);
            MergeCall(2, 2, 0, 1, 3, 1, 0, 0, win: false, move: true, score: 8);
            MergeCall(1, 2, 3, 4, 1, 2, 3, 4, win: false, move: false, score: 0);
            MergeCall(10, 10, 0, 0, 11, 0, 0, 0, win: true, move: true, score: 2048);
        }


    }
}
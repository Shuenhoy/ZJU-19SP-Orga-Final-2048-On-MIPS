namespace MIPSSim.Parser
{
    public abstract class Instruction
    {
        public Instruction Into
        { get => this; }
    }
    public class RInstruction : Instruction
    {
        public string Key;
        public int RS, RT, RD;
        public int Shamt;
    }

    public class IInstruction : Instruction
    {
        public string Key;
        public int RS, RT;
        public long Imm;
        public string Label;
    }
    public class JInstruction : Instruction
    {
        public string Key;
        public string Target;
        public int ITarget;
    }
    public class SInstruction : Instruction
    {
        public string Key;
    }

    // public class XOR : RInstruction { }
    // public class OR : RInstruction { }
    // public class NOR : RInstruction { }
    // public class JR : RInstruction { }

    // public class SLL : RInstruction { }
    // public class ADD : RInstruction { }
    // public class SLLv : RInstruction { }

    // public class ADDi : IInstruction { }
    // public class BNE : IInstruction { }
    // public class BEQ : IInstruction { }
    // public class LUi : IInstruction { }
    // public class ORi : IInstruction { }

    // public class SLTi : IInstruction { }
    // public class SW : IInstruction { }
    // public class LW : IInstruction { }

    // public class J : JInstruction { }
    // public class JAL : JInstruction { }





}
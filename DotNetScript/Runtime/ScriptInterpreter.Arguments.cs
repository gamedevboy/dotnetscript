using System.Diagnostics;
using DotNetScript.Types.Reference;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DotNetScript.Runtime
{
    internal partial class ScriptInterpreter
    {
        private void Ldarg_S(ref Instruction instruction)
        {
            _runtimeContext.PushToStack(_runtimeContext.CurrentStackFrame.Arguments[((ParameterDefinition)instruction.Operand).Index]);
        }

        private void Ldarg(ref Instruction instruction)
        {
            var index = instruction.OpCode.Value - OpCodes.Ldarg_0.Value;

            Debug.Assert(index < _runtimeContext.CurrentStackFrame.Arguments.Length);

            _runtimeContext.PushToStack(_runtimeContext.CurrentStackFrame.Arguments[index]);
        }

        private void Ldarga(ref Instruction instruction)
        {
            _runtimeContext.PushToStack(new ScriptArrayReference(_runtimeContext.CurrentStackFrame.Arguments, ((ParameterDefinition) instruction.Operand).Index + (_runtimeContext.CurrentStackFrame.ScriptMethod.HasThis ? 1 : 0)));
        }
    }
}

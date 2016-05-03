using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotNetScript.Types;
using Mono.Cecil.Cil;

namespace DotNetScript.Runtime
{
    partial class ScriptInterpreter
    {
        private void ExecFinal(ref Instruction instruction)
        {
            var currInst = instruction;
            var nextInst = (Instruction)instruction.Operand;

            var method = _runtimeContext.CurrentStackFrame.ScriptMethod.MethodDefinition;

            Debug.Assert(method != null);
            Debug.Assert(method.HasBody && method.Body.HasExceptionHandlers);

            var finallyHandler = method.Body.ExceptionHandlers
                .Where(_ => _.HandlerType == ExceptionHandlerType.Finally && 
                _.TryStart.Offset < currInst.Offset &&
                _.TryEnd.Offset > currInst.Offset &&
                _.HandlerEnd.Offset <= nextInst.Offset)
                .OrderBy(_ => _.TryEnd.Offset - currInst.Offset)
                .FirstOrDefault();

            if (finallyHandler == null)
                return;

            instruction = finallyHandler.HandlerStart;
            Execute(ref instruction);
        }

        private bool HandleException(ScriptMethodBase method, Exception ex, ref Instruction instruction)
        {
            if (method.MethodDefinition.Body.HasExceptionHandlers)
            {
                var inst = instruction;

                var handlers = method.MethodDefinition.Body.ExceptionHandlers
                    .Where(_ =>
                    {
                        if (_.HandlerType != ExceptionHandlerType.Catch)
                            return false;

                        if (_.TryStart.Offset > inst.Offset)
                            return false;

                        if (_.TryEnd.Offset < inst.Offset)
                            return false;

                        return true;
                    })
                    .OrderBy(_ => _.TryEnd.Offset - _.TryStart.Offset);

                var handlerList = handlers.ToList();
                var bestMatch = FindBestExceptionMatch(ex, handlerList);

                if (bestMatch == null) return false;

                instruction = bestMatch.HandlerStart;
                return true;
            }

            return false;
        }

        private static ExceptionHandler FindBestExceptionMatch(Exception ex, IEnumerable<ExceptionHandler> handlerList)
        {
            ExceptionHandler bestMatch = null;
            var bestMathDist = int.MaxValue;

            var exceptionType = ex.GetType();

            foreach (var handler in handlerList)
            {
                var type = ScriptContext.GetType(handler.CatchType);
                if (type.HostType == exceptionType)
                {
                    bestMatch = handler;
                    break;
                }

                var typeDist = GetTypeDistance(exceptionType, type.HostType);

                if (typeDist >= bestMathDist) continue;

                bestMatch = handler;
                bestMathDist = typeDist;
            }

            return bestMatch;
        }

        private static int GetTypeDistance(Type exceptionType, Type type)
        {
            if (exceptionType == type)
                return 0;

            if (exceptionType.IsSubclassOf(type))
                return GetTypeDistance(exceptionType.BaseType, type) + 1;

            return int.MaxValue;
        }
    }
}

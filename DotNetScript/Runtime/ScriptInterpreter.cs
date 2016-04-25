using System;
using System.Diagnostics;
using System.Linq;
using DotNetScript.Types;
using DotNetScript.Types.Reference;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DotNetScript.Runtime
{
    internal sealed partial class ScriptInterpreter
    {
        private readonly RuntimeContext _runtimeContext;

        public ScriptInterpreter(RuntimeContext runtimeRuntimeContext)
        {
            _runtimeContext = runtimeRuntimeContext;
        }

        public object Invoke(ScriptMethodBase scriptMethod, object target, params object[] args)
        {
            var callArgs = args;

            if (scriptMethod.MethodDefinition.HasThis)
            {
                callArgs = new object[args.Length + 1];
                callArgs[0] = target;
                Array.Copy(args, 0, callArgs, 1, args.Length);
            }

            _runtimeContext.PushCallStack(scriptMethod, callArgs);

            var inst = scriptMethod.MethodDefinition.Body.Instructions[0];

            while (true)
            {
                try
                {
                    Execute(ref inst);
                    break;
                }
                catch (ScriptException scriptException)
                {
                    break;
                }
                //catch (Exception exception)
                //{
                //    throw exception;
                //}
            }

            return _runtimeContext.PopCallStack().Return();
        }

        private void Execute(ref Instruction instruction)
        {
            while (instruction != null)
            {
                Console.WriteLine($"{instruction.Offset.ToString("x2")} {instruction.OpCode} {instruction.Operand}");

                switch (instruction.OpCode.FlowControl)
                {
                    case FlowControl.Branch:
                    case FlowControl.Cond_Branch:
                        ExecBranch(ref instruction);
                        break;
                    case FlowControl.Next:
                        ExecNext(instruction);
                        instruction = instruction.Next;
                        break;
                    case FlowControl.Call:
                        ExecCall(instruction);
                        instruction = instruction.Next;
                        break;
                    case FlowControl.Return:
                        ExecReturn(instruction);
                        return;
                    case FlowControl.Throw:
                        ExecThrow(instruction);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void ExecThrow(Instruction instruction)
        {
            throw new NotImplementedException();
        }

        private void ExecReturn(Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Ret:
                case Code.Endfinally:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void ExecCall(Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Call:
                case Code.Callvirt:
                    Call(instruction);
                    break;
                case Code.Newobj:
                    Newobj(instruction);
                    break;
            }
        }

        private void Newobj(Instruction instruction)
        {
            var methodRef = (MethodReference)instruction.Operand;
            var typeRef = methodRef.DeclaringType;
            if (typeRef.IsArray)
            {
                var arrayType = (ArrayType) typeRef;
                var scriptType = ScriptContext.GetType(arrayType.ElementType);
                var hostArrayType = scriptType.HostType.MakeArrayType(arrayType.Rank);
                var callArgs = new ScriptCallArguments(methodRef.Parameters.Count,
                    methodRef.Parameters.Select(_ => ScriptContext.GetType(_.ParameterType)).ToArray());

                _runtimeContext.PushToStack(Activator.CreateInstance(hostArrayType, callArgs.Arguments));
            }
            else
            {
                var scriptType = ScriptContext.GetType(typeRef);
                var method = scriptType.GetMethod(methodRef);
                var callArgs = new ScriptCallArguments(method);
                var scriptObject = new ScriptObject(scriptType);

                if (scriptType.HostType.IsSubclassOf(typeof (Delegate)))
                {
                    var scriptDelegate = new ScriptDelegate(callArgs.Arguments[0],
                        (ScriptMethodBase) callArgs.Arguments[1]);
                    var invokeMethod = scriptType.TypeDefinition.Methods.FirstOrDefault(_ => _.Name == "Invoke");
                    if (invokeMethod != null)
                        method.Invoke(scriptObject, scriptDelegate,
                            ScriptDelegate.GetInvokePtr(invokeMethod.Parameters.Count));
                }
                else
                {
                    method.Invoke(scriptObject, callArgs.Arguments);
                }
                _runtimeContext.PushToStack(scriptObject);
            }
        }

        private void Call(Instruction instruction)
        {
            var methodRef = (MethodReference)instruction.Operand;

            var type = ScriptContext.GetType(methodRef.DeclaringType);

            var method = type.GetMethod(methodRef, _runtimeContext.CurrentStackFrame.ScriptMethod as ScriptMethodInfo);
            if (method == null) return;

            var callArgs = new ScriptCallArguments(method);
            var targetObject = methodRef.HasThis ? _runtimeContext.PopFromStack() : null;

            var ret = method.Invoke(targetObject, callArgs.Arguments);
            if( method.HasReturn )
                _runtimeContext.PushToStack(ret);
        }

        private void ExecNext(Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Unbox_Any:
                case Code.Nop:
                    break;
                case Code.Box:
                    {
                        //var obj = _runtimeContext.PopFromStack();
                        //var typeRef = instruction.Operand as TypeReference;
                        //var typeDef = typeRef.Resolve();
                        //if (typeDef != null)
                        //{
                        //    if (typeDef.IsEnum)
                        //    {

                        //    }
                        //}
                        //_runtimeContext.PushToStack(obj);
                    }
                    break;
                case Code.Pop:
                    Pop();
                    break;
                case Code.Initobj:
                    Initobj(instruction);
                    break;
                case Code.Newarr:
                    Newarr(instruction);
                    break;
                case Code.Add:
                    Add();
                    break;
                case Code.Sub:
                    Sub();
                    break;
                case Code.Mul:
                    Mul();
                    break;
                case Code.Div:
                    Div();
                    break;
                case Code.Rem:
                case Code.Rem_Un:
                    Rem();
                    break;
                case Code.And:
                    And();
                    break;
                case Code.Or:
                    Or();
                    break;
                case Code.Xor:
                    Xor();
                    break;
                case Code.Not:
                    Not();
                    break;
                case Code.Neg:
                    Neg();
                    break;
                case Code.Cgt:
                case Code.Cgt_Un:
                    Cgt();
                    break;
                case Code.Ceq:
                    Ceq();
                    break;
                case Code.Clt:
                case Code.Clt_Un:
                    Clt();
                    break;
                case Code.No:
                    break;
                case Code.Conv_I4:
                    Conv_I4();
                    break;
                case Code.Conv_R4:
                    Conv_R4();
                    break;
                case Code.Conv_R8:
                    Conv_R8();
                    break;
                case Code.Ldlen:
                    Ldlen();
                    break;
                case Code.Ldnull:
                    Ldnull();
                    break;
                case Code.Ldstr:
                    Ldstr(instruction);
                    break;
                case Code.Isinst:
                    Isinst(instruction);
                    break;
                case Code.Stloc_0:
                case Code.Stloc_1:
                case Code.Stloc_2:
                case Code.Stloc_3:
                    Stloc(instruction);
                    break;
                case Code.Stloc_S:
                    Stloc_S(instruction);
                    break;
                case Code.Stelem_I1:
                case Code.Stelem_I4:
                case Code.Stelem_I2:
                case Code.Stelem_Ref:
                case Code.Stelem_Any:
                    Stelem();
                    break;
                case Code.Ldelem_I1:
                case Code.Ldelem_I2:
                case Code.Ldelem_I4:
                case Code.Ldelem_I8:
                case Code.Ldelem_R4:
                case Code.Ldelem_R8:
                case Code.Ldelem_U1:
                case Code.Ldelem_U2:
                case Code.Ldelem_U4:
                    Ldelem();
                    break;
                case Code.Ldelem_Ref:
                    Ldelem_Ref();
                    break;
                case Code.Ldloc_0:
                case Code.Ldloc_1:
                case Code.Ldloc_2:
                case Code.Ldloc_3:
                    Ldloc(instruction);
                    break;
                case Code.Ldind_Ref:
                    break;
                case Code.Ldloc_S:
                    Ldloc_S(instruction);
                    break;
                case Code.Ldflda:
                    Ldflda(instruction);
                    break;
                case Code.Ldsflda:
                    Ldsflda(instruction);
                    break;
                case Code.Ldloca:
                case Code.Ldloca_S:
                    Ldloca(instruction);
                    break;
                case Code.Ldarg_0:
                case Code.Ldarg_1:
                case Code.Ldarg_2:
                case Code.Ldarg_3:
                    Ldarg(ref instruction);
                    break;
                case Code.Ldarg_S:
                    Ldarg_S(ref instruction);
                    break;
                case Code.Ldarga:
                case Code.Ldarga_S:
                    Ldarga(ref instruction);
                    break;
                case Code.Ldc_I4_0:
                case Code.Ldc_I4_1:
                case Code.Ldc_I4_2:
                case Code.Ldc_I4_3:
                case Code.Ldc_I4_4:
                case Code.Ldc_I4_5:
                case Code.Ldc_I4_6:
                case Code.Ldc_I4_7:
                case Code.Ldc_I4_8:
                    Ldc_I4(instruction);
                    break;
                case Code.Ldc_I4_M1:
                    Ldc_M1();
                    break;
                case Code.Ldc_I8:
                case Code.Ldc_R4:
                case Code.Ldc_R8:
                case Code.Ldc_I4:
                case Code.Ldc_I4_S:
                    Ldc_S(instruction);
                    break;
                case Code.Ldftn:
                    var methodRef = (MethodReference) instruction.Operand;
                    _runtimeContext.PushToStack(ScriptContext.GetMethod(methodRef));
                    //Debug.Assert(methodRef.IsGenericInstance == false);
                    //Debug.Assert(methodRef.Resolve() != null);
                    //ScriptObject.CheckType(methodRef.DeclaringType);
                    //_runtimeContext.PushToStack(methodRef.Resolve());
                    break;
                case Code.Ldtoken:
                    {
                        var typeDef = instruction.Operand as TypeReference;

                        //if (RuntimeContext.InScriptScope(typeDef))
                        //    _runtimeContext.PushToStack(typeDef);
                        //else
                        //    _runtimeContext.PushToStack(RuntimeContext.GetNativeType(typeDef).TypeHandle);
                    }
                    break;
                case Code.Dup:
                    Dup();
                    break;
                case Code.Ldfld:
                case Code.Ldsfld:
                    Ldfld(instruction, instruction.OpCode.Code == Code.Ldsfld);
                    break;
                case Code.Stfld:
                case Code.Stsfld:
                    Stfld(instruction, instruction.OpCode.Code == Code.Stsfld);
                    break;
                case Code.Stind_I1:
                case Code.Stind_I2:
                case Code.Stind_I4:
                case Code.Stind_I8:
                case Code.Stind_R4:
                case Code.Stind_R8:
                case Code.Stind_Ref:
                    Stind();
                    break;
                case Code.Castclass:
                    break;
                case Code.Constrained:
                    break;
                case Code.Shl:
                    Shl();
                    break;
                case Code.Shr:
                case Code.Shr_Un:
                    Shr();
                    break;
                default:
                    throw new NotImplementedException($"opcode {instruction.OpCode.Name} not implemented!");
            }
        }

        private void Ldloca(Instruction instruction)
        {
            _runtimeContext.PushToStack(new ScriptArrayReference(_runtimeContext.CurrentStackFrame.Locals, ((VariableDefinition)instruction.Operand).Index));
        }

        private void Ldflda(Instruction instruction)
        {
            _runtimeContext.PushToStack(new ScriptFieldReference(_runtimeContext.PopFromStack(), (FieldReference)instruction.Operand));
        }

        private void Ldsflda(Instruction instruction)
        {
            _runtimeContext.PushToStack(new ScriptFieldReference(null, (FieldReference)instruction.Operand));
        }

        private void Stloc_S(Instruction instruction)
        {
            _runtimeContext.CurrentStackFrame.Locals[((VariableDefinition)instruction.Operand).Index] = _runtimeContext.PopFromStack();
        }

        private void Stloc(Instruction instruction)
        {
            _runtimeContext.CurrentStackFrame.Locals[instruction.OpCode.Value - OpCodes.Stloc_0.Value] = _runtimeContext.PopFromStack();
        }

        private void Isinst(Instruction instruction)
        {
            var obj = _runtimeContext.PopFromStack();
            var typeRef = (TypeReference)instruction.Operand;

            var scriptType = ScriptContext.Get(typeRef.Module).TypeSystem.GetType(typeRef);
            _runtimeContext.PushToStack(scriptType.IsInstanceOfType(obj));
        }

        private void Stfld(Instruction instruction, bool isStatic)
        {
            var value = _runtimeContext.PopFromStack();
            var target = !isStatic ? _runtimeContext.PopFromStack() : null;
            var fieldRef = (FieldReference)instruction.Operand;
            var typeRef = fieldRef.DeclaringType;

            var scriptField = ScriptContext.Get(typeRef.Module)?.TypeSystem.GetType(typeRef)?.GetField(fieldRef);
            Debug.Assert(scriptField != null);
            scriptField?.SetValue(target, value);
        }

        private void Ldfld(Instruction instruction, bool isStatic)
        {
            var target = !isStatic ? _runtimeContext.PopFromStack() : null;
            var fieldRef = (FieldReference)instruction.Operand;
            var typeRef = fieldRef.DeclaringType;

            var scriptField = ScriptContext.Get(typeRef.Module)?.TypeSystem.GetType(typeRef)?.GetField(fieldRef);
            Debug.Assert(scriptField != null);
            if (scriptField != null) _runtimeContext.PushToStack(scriptField.GetValue(target));
        }

        private void Conv_R8()
        {
            dynamic value = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack((double)value);
        }

        private void Conv_R4()
        {
            dynamic value = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack((float)value);
        }

        private void Conv_I4()
        {
            dynamic target = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack((int)target);
        }

        private void Initobj(Instruction instruction)
        {
            var refObj = (IScriptReference)_runtimeContext.PopFromStack();
            var typeRef = (TypeReference)instruction.Operand;
            var scriptType = ScriptContext.Get(typeRef.Module)?.TypeSystem.GetType(typeRef);
            refObj.Value = scriptType?.CreateInstance();
        }

        private void Stind()
        {
            var value = _runtimeContext.PopFromStack();
            var refObj = _runtimeContext.PopFromStack() as IScriptReference;
            if (refObj != null) refObj.Value = value;
        }

        private void Stelem()
        {
            dynamic value = _runtimeContext.PopFromStack();
            dynamic index = _runtimeContext.PopFromStack();
            dynamic array = _runtimeContext.PopFromStack();
            if (array is char[])
                array[index] = (char)value;
            else
                array[index] = value;
        }

        private void Ldelem_Ref()
        {
            dynamic index = _runtimeContext.PopFromStack();
            dynamic value = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack(new ScriptArrayReference(value, index));
        }

        private void Ldelem()
        {
            dynamic index = _runtimeContext.PopFromStack();
            dynamic array = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack(array[index]);
        }

        private void Ceq()
        {
            dynamic op2 = _runtimeContext.PopFromStack();
            dynamic op1 = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack(IsLogicEqual(op1, op2));
        }

        private object IsLogicEqual(object op1, object op2)
        {
            throw new NotImplementedException();
        }

        private void Clt()
        {
            dynamic op2 = _runtimeContext.PopFromStack();
            dynamic op1 = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack(op1 < op2);
        }

        private void Newarr(Instruction instruction)
        {
            var typeRef = (TypeReference)instruction.Operand;
            var arrayType = ScriptContext.GetType(typeRef).HostType;
            var length = _runtimeContext.PopFromStack();

            _runtimeContext.PushToStack(Activator.CreateInstance(arrayType.MakeArrayType(1), length));
        }

        private void Pop()
        {
            _runtimeContext.PopFromStack();
        }

        private void Ldlen()
        {
            dynamic value = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack(value.Length);
        }

        private void Ldstr(Instruction instruction)
        {
            _runtimeContext.PushToStack(instruction.Operand);
        }

        private void Ldnull()
        {
            _runtimeContext.PushToStack(null);
        }

        private void Add()
        {
            dynamic op2 = _runtimeContext.PopFromStack();
            dynamic op1 = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack(op1 + op2);
        }

        private void Sub()
        {
            dynamic op2 = _runtimeContext.PopFromStack();
            dynamic op1 = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack(op1 - op2);
        }

        private void Mul()
        {
            dynamic op2 = _runtimeContext.PopFromStack();
            dynamic op1 = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack(op1 * op2);
        }

        private void Div()
        {
            dynamic op2 = _runtimeContext.PopFromStack();
            dynamic op1 = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack(op1 / op2);
        }

        private void Rem()
        {
            dynamic op2 = _runtimeContext.PopFromStack();
            dynamic op1 = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack(op1 % op2);
        }

        private void And()
        {
            try
            {
                dynamic op2 = _runtimeContext.PopFromStack();
                dynamic op1 = _runtimeContext.PopFromStack();
                if (op2 is Enum)
                    op2 = Convert.ChangeType(op2, op1.GetType());

                if (op1 is Enum)
                    op1 = Convert.ChangeType(op1, op2.GetType());

                _runtimeContext.PushToStack(op1 & op2);
            }
            catch
            {

            }
        }

        private void Or()
        {
            dynamic op2 = _runtimeContext.PopFromStack();
            dynamic op1 = _runtimeContext.PopFromStack();

            if (op2 is Enum)
                op2 = Convert.ChangeType(op2, op1.GetType());

            if (op1 is Enum)
                op1 = Convert.ChangeType(op1, op2.GetType());

            _runtimeContext.PushToStack(op1 | op2);
        }

        private void Xor()
        {
            dynamic op2 = _runtimeContext.PopFromStack();
            dynamic op1 = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack(op1 ^ op2);
        }

        private void Cgt()
        {
            dynamic op2 = _runtimeContext.PopFromStack();
            dynamic op1 = _runtimeContext.PopFromStack();

            if (op1 != null && op2 == null)
                _runtimeContext.PushToStack(true);
            else if (op1 == null && op2 != null)
                _runtimeContext.PushToStack(false);
            else
                _runtimeContext.PushToStack(op1 > op2);
        }

        private void Neg()
        {
            dynamic op = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack(-op);
        }

        private void Not()
        {
            dynamic op = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack(!op);
        }

        private void Ldloc_S(Instruction instruction)
        {
            _runtimeContext.PushToStack(_runtimeContext.CurrentStackFrame.Locals[(instruction.Operand as VariableDefinition).Index]);
        }

        private void Ldloc(Instruction instruction)
        {
            _runtimeContext.PushToStack(_runtimeContext.CurrentStackFrame.Locals[instruction.OpCode.Value - OpCodes.Ldloc_0.Value]);
        }

        private void Shr()
        {
            dynamic count = _runtimeContext.PopFromStack();
            dynamic value = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack(value >> count);
        }

        private void Shl()
        {
            dynamic count = _runtimeContext.PopFromStack();
            dynamic value = _runtimeContext.PopFromStack();
            _runtimeContext.PushToStack(value << count);
        }

        private void Dup()
        {
            _runtimeContext.PushToStack(_runtimeContext.PeekFromStack());
        }

        private void Ldc_S(Instruction instruction)
        {
            _runtimeContext.PushToStack(instruction.Operand);
        }

        private void Ldc_M1()
        {
            _runtimeContext.PushToStack(-1);
        }

        private void Ldc_I4(Instruction instruction)
        {
            _runtimeContext.PushToStack(instruction.OpCode.Value - OpCodes.Ldc_I4_0.Value);
        }

        private void ExecBranch(ref Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Leave:
                case Code.Leave_S:
                    var leaveInst = (Instruction)instruction.Operand;
                    ExecFinal(ref instruction);
                    instruction = leaveInst;
                    break;
                case Code.Br:
                case Code.Br_S:
                    instruction = (Instruction)instruction.Operand;
                    break;
                case Code.Brtrue:
                case Code.Brtrue_S:
                    instruction = IsTrue(_runtimeContext.PopFromStack()) ? (Instruction)instruction.Operand : instruction.Next;
                    break;
                case Code.Brfalse:
                case Code.Brfalse_S:
                    instruction = !IsTrue(_runtimeContext.PopFromStack()) ? (Instruction)instruction.Operand : instruction.Next;
                    break;
                case Code.Beq:
                case Code.Beq_S:
                    {
                        var obj2 = _runtimeContext.PopFromStack();
                        var obj1 = _runtimeContext.PopFromStack();
                        instruction = obj1.Equals(obj2) ? (Instruction)instruction.Operand : instruction.Next;
                        break;
                    }
                    break;
                case Code.Bne_Un:
                case Code.Bne_Un_S:
                    {
                        var obj2 = _runtimeContext.PopFromStack();
                        var obj1 = _runtimeContext.PopFromStack();

                        if (obj1 == null || obj2 == null)
                            instruction = obj1 != obj2 ? (Instruction)instruction.Operand : instruction.Next;
                        else
                            instruction = !obj1.Equals(obj2) ? (Instruction)instruction.Operand : instruction.Next;
                    }
                    break;
                case Code.Blt:
                case Code.Blt_S:
                case Code.Blt_Un:
                case Code.Blt_Un_S:
                    {
                        dynamic obj2 = _runtimeContext.PopFromStack();
                        dynamic obj1 = _runtimeContext.PopFromStack();

                        instruction = obj1 < obj2 ? (Instruction)instruction.Operand : instruction.Next;
                    }
                    break;
                case Code.Bgt:
                case Code.Bgt_S:
                case Code.Bgt_Un:
                case Code.Bgt_Un_S:
                    {
                        dynamic obj2 = _runtimeContext.PopFromStack();
                        dynamic obj1 = _runtimeContext.PopFromStack();

                        instruction = obj1 > obj2 ? (Instruction)instruction.Operand : instruction.Next;
                    }
                    break;
                case Code.Bge:
                case Code.Bge_S:
                case Code.Bge_Un:
                case Code.Bge_Un_S:
                    {
                        dynamic obj2 = _runtimeContext.PopFromStack();
                        dynamic obj1 = _runtimeContext.PopFromStack();

                        instruction = obj1 >= obj2 ? (Instruction)instruction.Operand : instruction.Next;
                    }
                    break;
                case Code.Ble:
                case Code.Ble_S:
                case Code.Ble_Un:
                case Code.Ble_Un_S:
                    {
                        dynamic obj2 = _runtimeContext.PopFromStack();
                        dynamic obj1 = _runtimeContext.PopFromStack();

                        instruction = obj1 <= obj2 ? (Instruction)instruction.Operand : instruction.Next;
                    }
                    break;
                case Code.Switch:
                    {
                        dynamic index = _runtimeContext.PopFromStack();
                        var inst = (Instruction[])instruction.Operand;
                        instruction = index < 0 ? instruction.Next : inst[index];
                    }
                    break;
                default:
                    throw new NotImplementedException($"{ instruction.OpCode.Name } not implemented !");
            }
        }

        private static bool IsTrue(object value)
        {
            return value != null;
        }
    }


}

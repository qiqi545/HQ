using System.Reflection;
using HQ.CodeGeneration.Internal.Extensions;
using Mono.Cecil.Cil;

namespace HQ.CodeGeneration
{
    public static class WeavingExtensions
    {
        public static void OnExit(this MethodInfo onMethod, MethodInfo onExit)
        {
            var method = onMethod.ToDefinition();
            if (!method.HasBody)
                return;

            var il = method.Body.GetILProcessor();
            foreach (var instr in method.Body.Instructions)
            {
                if (instr.OpCode != OpCodes.Ret)
                    continue;
                var name = il.Create(OpCodes.Ldstr, $"{onMethod.DeclaringType?.FullName}.{onMethod.Name}");
                il.InsertBefore(instr, name);
                il.InsertAfter(name, il.Create(OpCodes.Call, onExit.ToReference()));
                break; // picks the first exit only, but iterating past that is causing a crash
            }
        }
    }
}

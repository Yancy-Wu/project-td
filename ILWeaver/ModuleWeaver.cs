using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

namespace ILWeaver {
    public class ModuleWeaver : BaseModuleWeaver {
        TypeDefinition objType = null;
        TypeDefinition attrType = null;
        MethodDefinition weaveMethod = null;

        public override void Execute() {
            foreach (var type in ModuleDefinition.Types) {
                if (type.Name == WeaveConfig.WeaveClassName) objType = type.Resolve();
                if (type.Name == WeaveConfig.WeaveAttrName) attrType = type.Resolve();
            }
            foreach (var m in objType.Methods) {
                if (m.Name == WeaveConfig.WeaveMethodName) weaveMethod = m.Resolve();
            }
            foreach (var type in ModuleDefinition.Types) {
                if (!CheckBaseType(type, objType)) continue;
                ProcessPObject(type);
            }
        }

        private bool CheckBaseType(TypeDefinition type, TypeDefinition baseType) {
            if (type.FullName == baseType.FullName) return true;
            var curType = type.BaseType;
            while (curType != null) {
                if (curType.FullName == baseType.FullName) return true;
                curType = curType.Resolve().BaseType;
            }
            return false;
        }

        public override IEnumerable<string> GetAssembliesForScanning() {
            yield return "mscorlib";
            yield return "System";
        }

        public void ProcessPObject(TypeDefinition type) {
            foreach (var p in type.Properties) {
                foreach (var attr in p.CustomAttributes) {
                    if (CheckBaseType(attr.AttributeType.Resolve(), attrType)) {
                        ProcessField(type, p);
                        break;
                    }
                }
            }
        }

        static public GenericInstanceMethod MakeGenericMethod(MethodReference method, TypeReference[] genericArguments) {
            var _method = new GenericInstanceMethod(method);
            foreach (var arg in genericArguments) {
                _method.GenericArguments.Add(arg);
            }
            return _method;
        }

        public void ProcessField(TypeDefinition _, PropertyDefinition field) {
            MethodDefinition setattr = field.SetMethod;
            MethodDefinition getattr = field.GetMethod;

            // 整点局部变量.
            VariableDefinition oldItemVar = new VariableDefinition(getattr.ReturnType);
            setattr.Body.Variables.Add(oldItemVar);

            // 获取当前方法体中的第一个 IL 指令
            var processor = setattr.Body.GetILProcessor();
            var current = setattr.Body.Instructions.First();

            // 插入一个 Nop 指令，表示什么都不做
            var first = Instruction.Create(OpCodes.Nop);
            processor.InsertBefore(current, first);
            current = first;

            // 先读取旧的数据.
            foreach (var instruction in ReadAttr(getattr, oldItemVar)) {
                processor.InsertAfter(current, instruction);
                current = instruction;
            }

            // 在末尾插入监听事件调用.
            int index = setattr.Body.Instructions.Count - 2;
            current = setattr.Body.Instructions[index];
            foreach (var instruction in InsertPropEventListener(setattr, getattr.ReturnType, oldItemVar)) {
                processor.InsertAfter(current, instruction);
                current = instruction;
            }
        }

        private IEnumerable<Instruction> ReadAttr(MethodDefinition method, VariableDefinition oldItemVar) {
            yield return Instruction.Create(OpCodes.Nop);
            yield return Instruction.Create(OpCodes.Ldarg_0);               // 加载this.
            yield return Instruction.Create(OpCodes.Call, method);          // 获取旧值.
            yield return Instruction.Create(OpCodes.Stloc_S, oldItemVar);   // 存放旧值到局部变量.
        }

        private IEnumerable<Instruction> InsertPropEventListener(MethodDefinition _, TypeReference returnType, VariableDefinition oldItemVar) {
            yield return Instruction.Create(OpCodes.Nop);
            yield return Instruction.Create(OpCodes.Ldarg_0);               // this变量.
            yield return Instruction.Create(OpCodes.Ldarg_1);               // 新值.
            yield return Instruction.Create(OpCodes.Ldloc_S, oldItemVar);   // 旧值.
            yield return Instruction.Create(OpCodes.Call, MakeGenericMethod(weaveMethod, new TypeReference[] { returnType }));     // 调用函数.
        }
    }
}

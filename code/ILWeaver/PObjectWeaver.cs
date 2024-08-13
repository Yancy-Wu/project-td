using Fody;
using Game.Core.Meta;
using Game.Core.Objects;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Game.ILWeaver {
    internal class PObjectWeaver: BaseModuleWeaver {

        public override void Execute() {
            foreach (var type in ModuleDefinition.Types) {
                if (!type.GetType().IsAssignableTo(typeof(PObject))) continue;
                ProcessPObject(type);
            }
        }

        public override IEnumerable<string> GetAssembliesForScanning() {
            yield return "mscorlib";
            yield return "System";
        }

        public void ProcessPObject(TypeDefinition type) {
            foreach(var p in type.Properties) {
                foreach (var attr in p.CustomAttributes) {
                    if (attr.AttributeType.GetType() == typeof(GamePropertyAttribute)){
                        ProcessField(type, p);
                        break;
                    }
                }
            }
        }

        public void ProcessField(TypeDefinition type, PropertyDefinition field) {
            MethodDefinition setattr = field.SetMethod;
            MethodDefinition getattr = field.GetMethod;

            // 获取当前方法体中的第一个 IL 指令
            var processor = setattr.Body.GetILProcessor();
            var current = setattr.Body.Instructions.First();

            // 插入一个 Nop 指令，表示什么都不做
            var first = Instruction.Create(OpCodes.Nop);
            processor.InsertBefore(current, first);
            current = first;

            // 先读取旧的数据.
            foreach (var instruction in ReadAttr(getattr)) {
                processor.InsertAfter(current, instruction);
                current = instruction;
            }

            // 在末尾插入监听事件调用.
            foreach (var instruction in InsertPropEventListener()) {
                processor.InsertAfter(current, instruction);
                current = instruction;
            }
        }

        private IEnumerable<Instruction> ReadAttr(MethodDefinition method) {
            TypeReference paraType = method.Parameters[0].ParameterType;
            yield return Instruction.Create(OpCodes.Nop);
            yield return Instruction.Create(OpCodes.Ldarg_1);
            yield return Instruction.Create(OpCodes.Box);
            yield return Instruction.Create(OpCodes.Starg_S, 0xFF);  // 存放新值(装箱)放到0xFF局部变量.
            yield return Instruction.Create(OpCodes.Call, method);
            yield return Instruction.Create(OpCodes.Box);
            yield return Instruction.Create(OpCodes.Starg_S, 0xFE);  // 存放旧值(装箱)放到0xFE局部变量.
        }

        private IEnumerable<Instruction> InsertPropEventListener() {
            yield return Instruction.Create(OpCodes.Nop);
            yield return Instruction.Create(OpCodes.Ldarg_0);           // this变量.
            yield return Instruction.Create(OpCodes.Ldarga_S, 0xFF);    // 新值.
            yield return Instruction.Create(OpCodes.Ldarga_S, 0xFE);    // 旧值.
            yield return Instruction.Create(OpCodes.Call, ModuleDefinition.ImportReference(typeof(PObject).GetMethod(nameof(PObject.AfterSetProp))));
        }
    }
}

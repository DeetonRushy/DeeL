

namespace Runtime.Interpreting.Structs.Builtin;

public class FileStruct : BaseBuiltinStructDefinition
{
    public override string Name { get => "__Io"; }

    public FileStruct()
    {
        DefineBuiltinFunction("read", true, (p, self, args) =>
        {
            var arg = args[0].Take(p);
            if (arg is not string path)
                return new(Interpreter.Undefined);
            return new(File.ReadAllText(path));
        });
    }
}

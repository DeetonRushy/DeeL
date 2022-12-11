using Runtime.Parser;
using Runtime.Parser.Production;

namespace Runtime.Interpreting;

public class UserDefinedProperty
{
    private object _value;
    
    public string Name { get; set; }
    public bool IsStatic { get; set; }
    public TypeHint Type { get; set; }

    public UserDefinedProperty(Interpreter interpreter, PropertyDeclaration declaration)
    {
        if (declaration.Initializer is { } init)
        {
            _value = interpreter.Visit(init);
        }

        Name = declaration.Name;
        IsStatic = declaration.IsStatic;
        Type = declaration.Type;
    }

    public void Set(object value)
        => _value = value;

    public void SetChecked(Declaration declaration) => throw new NotImplementedException();

    public object Get() => _value;
}
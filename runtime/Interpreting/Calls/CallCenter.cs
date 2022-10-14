using System.Reflection;

namespace DL.Interpreting.Calls;

public class CallCenter
{
    private readonly List<ICallable> _functions;

    public CallCenter()
    {
        _functions = new List<ICallable>();

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.IsAssignableTo(typeof(ICallable))
                && type != typeof(ICallable))
            {
                try
                {
                    var instance = (ICallable?)Activator.CreateInstance(type);
                    _functions.Add(instance!);
                }
                catch
                {
                    // fuck it.
                }
            }
        }
    }

    public bool HasDefinition(string identifier)
        => _functions.Any(x => x.Identifier == identifier);

    public ICallable GetDefinition(string identifier)
        => _functions.Where(x => x.Identifier == identifier).First();

    public bool TryGetDefinition(string identifier, out ICallable function)
    {
        if (!HasDefinition(identifier))
        {
            function = null!;
            return false;
        }

        function = GetDefinition(identifier);
        return true;
    }
}
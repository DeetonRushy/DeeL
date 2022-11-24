
namespace Runtime.Interpreting.Extensions;

// Things here can be used to make life easier when interpreting stuff.

/*
 Lets say you're interpreting an addition. You receive two variables to add together.
 
 Instead of doing -- if (Value is XXX instance) over and over to verify types.
 You can do if (!var.Value.IsIntegral()) { error ... } before heavy logic.
 */

public static class RuntimeObjectExtensions
{
    public static bool IsIntegral(this object obj)
    {
        // The two integer types in DL.
        return obj is decimal or long;
    }

    public static bool IsStringy(this object obj)
        => obj is string;

    public static bool IsDLObject(this object obj)
        => obj?.GetType()?.FullName?.Contains("Runtime") ?? false;
}

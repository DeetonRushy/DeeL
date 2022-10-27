using System.Diagnostics.CodeAnalysis;
using Runtime.Lexer;

namespace Runtime.Parser;

public static class DVariables
{
    /// <summary>
    /// All globally defined symbols must be inserted here in order to be processed.
    /// </summary>
    public static readonly IDictionary<string, (DToken, object)> Variables 
        = new Dictionary<string, (DToken, object)>()
        {
            { "$CurrentWorkingDirectory", (DToken.MakeVar(TokenType.String), Directory.GetCurrentDirectory()) },
            // TODO: add some sort of pre-compilation directive that defines version
            { "$DLVersion", (DToken.MakeVar(TokenType.String), "0.1.1") }
        };

    public static (DToken, object) GetValueFor(string identifier)
        => Variables[identifier];
    public static bool TryGetValueFor(string identifier, [NotNullWhen(true)] out (DToken, object) value)
    {
        return Variables.TryGetValue(identifier, out value);
    }

    /// <summary>
    /// Add a global variable. Any variable that is registered can be processed as identifiers
    /// during parsing. The parser should replace any identifier named <paramref name="symbol"/>
    /// and replace it with <paramref name="value"/>.
    /// </summary>
    /// <param name="symbol">The identifier to be used</param>
    /// <param name="value">The value to be used internally</param>
    public static void AddGlobalSymbol(string symbol, (DToken, object) value)
    {
        Variables[symbol] = value;
    }

    public static bool GlobalSymbolExists(string symbol)
        => Variables.ContainsKey(symbol);
}
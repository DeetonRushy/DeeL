namespace DL.Lexer;

/// <summary>
/// Represents as a span of source characters in a DL token.
/// </summary>
public class DSpan
{
    /// <summary>
    /// The config files source contents, for use in <see cref="Contents"/>
    /// </summary>
    public static string? SourceContents { get; set; }

    /// <summary>
    /// The start index of the lexeme.
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    /// The ending index of the lexeme.
    /// </summary>
    public int End { get; set; }

    /// <summary>
    /// Gather the source contents from <see cref="SourceContents"/> from <see cref="Start"/> to <see cref="End"/>
    /// </summary>
    /// <returns>The contents</returns>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="SourceContents"/> is <c>null</c> or if the index' are incorrect.</exception>
    public string Contents()
    {
        if (SourceContents is null)
        {
            throw new InvalidOperationException("DSpan.SourceContents has not been set before calling Contents()");
        }

        if (Start >= SourceContents.Length || 0 > Start)
            throw new InvalidOperationException("invalid span");
        if (End >= SourceContents.Length || 0 > End)
            throw new InvalidOperationException("invalid span");

        return SourceContents[Start..End];
    }
}
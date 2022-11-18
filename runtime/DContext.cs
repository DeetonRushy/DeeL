using Runtime.Interpreting;
using Runtime.Parser.Errors;

namespace Runtime;

public class DContext
{
    public List<DError> Errors { get; init; } = null!;
}
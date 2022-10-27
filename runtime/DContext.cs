using Runtime.Interpreting;
using Runtime.Parser.Errors;

namespace Runtime;

public class DContext
{
    public IConfig Config { get; init; } = null!;
    public List<DError> Errors { get; init; } = null!;
}
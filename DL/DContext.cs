using DL.Interpreting;
using DL.Parser.Errors;

namespace DL;

public class DContext
{
    public IConfig Config { get; init; } = null!;
    public List<DError> Errors { get; init; } = null!;
}
namespace DL.Lexer;

/// <summary>
/// Represents a type of <see cref="DToken"/>
/// </summary>
public enum TokenType
{
    /// <summary>
    /// String literal wrapped in either ' or ". This should be parsed individually into a <see cref="System.String"/>
    /// </summary>
    String,

    /// <summary>
    /// Number literal with no decimal places. This can be anywhere from <see cref="long.MinValue"/> to <see cref="long.MaxValue"/>
    /// </summary>
    Number,

    /// <summary>
    /// `true` or `false`.
    /// </summary>
    Boolean,

    /// <summary>
    /// A number with a decimal point. This can be anywhere from <see cref="decimal.MinValue"/> to <see cref="decimal.MaxValue"/> 
    /// </summary>
    Decimal,

    /// <summary>
    /// The opening brace of a dictionary. `{`.
    /// </summary>
    DictOpen,

    /// <summary>
    /// The closing brace of a dictionary. `}`.
    /// </summary>
    DictClose,

    /// <summary>
    /// The opening bracket of a list. `[`
    /// </summary>
    ListOpen,

    /// <summary>
    /// The closing bracket of a list. `]`
    /// </summary>
    ListClose,

    /// <summary>
    /// The equals symbol. `=`.
    /// </summary>
    Equals,

    /// <summary>
    /// `null`. Converted into a custom type. (dont actually make the value null.)
    /// </summary>
    Null,

    /// <summary>
    /// The colon symbol. `:`. Used for defining members of a dictionary.
    /// </summary>
    Colon,

    /// <summary>
    /// The comma symbol. `,`. Used for seperating list and dictionary items.
    /// </summary>
    Comma,

    /// <summary>
    /// The semi-colon symbol. `;`. Used to end a line.
    /// </summary>
    SemiColon,

    /// <summary>
    /// A comment.
    /// </summary>
    Comment,

    /// <summary>
    /// Special token that specifies to set the value to <see cref="int.MaxValue"/>. `MAX_INT`
    /// </summary>
    MaxInt,

    /// <summary>
    /// Special token that specifies to set the value to <see cref="int.MinValue"/>. `MIN_INT`
    /// </summary>
    MinInt,

    /// <summary>
    /// Special token that specifies to set the value to <see cref="decimal.MaxValue"/>. `MAX_DECIMAL`
    /// </summary>
    MaxDecimal,
    /// <summary>
    /// Special token that specifies to set the value to <see cref="decimal.MinValue"/>. `MIN_DECIMAL`
    /// </summary>
    MinDecimal,

    /// <summary>
    /// Special token represents a newline.
    /// </summary>
    Newline,

    /// <summary>
    /// Special token that represents the end of the contents.
    /// </summary>
    Eof,

    /// <summary>
    /// Special token that represents an invalid token.
    /// </summary>
    Invalid
}
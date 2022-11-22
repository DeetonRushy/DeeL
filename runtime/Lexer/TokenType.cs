namespace Runtime.Lexer;

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
    LeftBrace,

    /// <summary>
    /// The closing brace of a dictionary. `}`.
    /// </summary>
    RightBrace,

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
    /// A literal name, like a variable name. Tokens of this type can be used to represent 
    /// boolean values and the like.
    /// </summary>
    Identifier,

    /// <summary>
    /// The token that represents the start of a function call.
    /// </summary>
    LeftParen,

    /// <summary>
    /// The token that represents the end of a function call. 
    /// </summary>
    RightParen,

    /// <summary>
    /// Special token represents a newline.
    /// </summary>
    Newline,

    /// <summary>
    /// Special token that represents the end of a statement or declaration.
    /// </summary>
    LineBreak,

    /// <summary>
    /// Special token that represents the end of the contents.
    /// </summary>
    Eof,

    /// <summary>
    /// Whitespace, an empty character.
    /// </summary>
    Whitespace,

    /// <summary>
    /// Special token that represents an invalid token.
    /// </summary>
    Invalid,

    /// <summary>
    /// The module keyword. This sets the name of current translation unit
    /// </summary>
    Module,

    /// <summary>
    /// The end keyword. Signifies the end of a block of code.
    /// </summary>
    End,

    /// <summary>
    /// The if keyword. Conditional operations
    /// </summary>
    If,

    /// <summary>
    /// The else keyword. Fallback from conditional operations
    /// </summary>
    Else,

    /// <summary>
    /// The `fn` keyword. Used to define a function.
    /// </summary>
    Fn,

    /// <summary>
    /// The `return` keyword. Used to return a value from a function.
    /// </summary>
    Return,

    /// <summary>
    /// The `let` keyword. Used to define a variable.
    /// </summary>
    Let,

    /// <summary>
    /// The `__break` keyword. Used to programatically break and enter the debugger for the interpreter.
    /// </summary>
    ForcedBreakPoint,

    /// <summary>
    /// The `->` operator.
    /// </summary>
    Arrow,

    /// <summary>
    /// The `-` operator.
    /// </summary>
    Minus,

    /// <summary>
    /// The `+` operator.
    /// </summary>
    Plus,

    /// <summary>
    /// The `/` operator
    /// </summary>
    Divide,

    /// <summary>
    /// The `*` operator
    /// </summary>
    Star,

    /// <summary>
    /// The `%` operator
    /// </summary>
    Modulo,

    /// <summary>
    /// The `>` operator.
    /// </summary>
    Greater,

    /// <summary>
    /// The `>=` operator.
    /// </summary>
    GreaterEqual,

    /// <summary>
    /// The `&lt;` operator.
    /// </summary>
    Lesser,

    /// <summary>
    /// The '&lt;=' operator.
    /// </summary>
    LesserEqual,

    /// <summary>
    /// The '==' operator.
    /// </summary>
    EqualComparison,

    /// <summary>
    /// The '!=' operator.
    /// </summary>
    NotEqual,

    /// <summary>
    /// The '!' operator
    /// </summary>
    Not,

    /// <summary>
    /// The `struct` keyword.
    /// </summary>
    Struct,
    Access,
    While,
}
# DeeL (Work In Progress)

An esoteric language that barely works at the moment.

# DISCLAIMER
This isn't actually a language persay, there are no types that actually build the language
or make it extendable right now.

# Information
There are 6 implicit data types in DeeL. 

 - Strings: just a string. They can be optionally delimited by a ' or "
 - Numbers: a number, represented by a 64 bit integer
 - Decimals: a floating point value
 - Lists: an array of any of the above data types
 - Booleans: true or false
 - Dictionarys: an array of key-value pairs

## Goals

 1. Provide fast lexical analysis and parsing.
 3. Have a cool project!

## Examples

### Module identifiers
```
mod 'module_name'; # <-- can be anything in a string

fn someCodeInMyModule() {}
```

Support for import 'module_name' will be supported at some point.
### Declare variables
```
let variable: string = 'value';
let result = getResult(variable);
```

### User-defined functions
```
fn myFunction(arg) {
    writeln(arg);
    let inner: string = 'not available globally';
    return inner;
}
writeln(inner); # <-- error: inner is not available in this scope

myFunction('Hello, World!');
```

### Objects
Define a class/struct
```
object Person {
    fn construct(self, name: string) -> void {
        self.name = name;
    }
}
```
Assign to `self` within `self`. Other entities will not be able to assign to `self` (that is the plan
, but may still work in some cases)

### Module System
Import a module 
```
from 'std.io' import { File, Path };
# the `*` wildcard is also supported, this will import everything
from 'some.lib' import {*}; # Nice!
```

At some point there will be a CLI that you can register paths from.
But, currently you must use the `Configuration` class.

```csharp
Configuration.AppendToOption("module-paths", "C:/my/custom/path");
```
import will only load files that have the extension
`.dl` and **will** search within nested directorys.
### Conditional Logic
```
let num: number = get_some_number();
if (num == 69) {
    writeln("nice!");
}
```

### Dictionarys & Lists
Json set the standard, so they have to be included.
```
let people = {
    'deeton': {
	     'age': 19,
	     'country': 'England'
	   },
};

let countrys = [
    'England',
    'France',
    'Peru'
];
```

### Predefined variables
```
'working-directory' = $CurrentWorkingDirectory;
```

These are parse-time variables, sort of like a preprocessor variable. Once the interpreter
sees them, they are C# literals.
### Functions (builtin)

Small builtin functions can provide cross-platform, dynamic values. They can also prevent
doing extra work at runtime.

The `relative` function for example, will take a relative path and convert it to an absolute
path before your code even sees it.

Checkout builtin docs [here](https://github.com/DeetonRushy/DeeL/blob/master/functions/README.md)!

### Custom Interpreter Info

To implement your own interpreter, all you need to do is implement `ISyntaxTreeVisitor<T>`.

You will need to implement your own way of working with a `FunctionCall`

### Debugging

When working on the runtime, there is a builtin statement called '__break' that, when hit, will
cause a debugger to attach and break.

#### Example
```
__break; # <-- causes a break, then the next step will be your code.
my_new_feature_that_isnt_working()
```

## This isn't too serious
I'm making this to challenge myself. That being said, this isn't going to be the fastest, most reliable configuration language out there. However, it's been fun to work on up to now so it's worth it.

## Credits
[Jay Madden](https://github.com/Jay-Madden) with their project [SharpLox](https://github.com/Jay-Madden/SharpLox).
The entire design was inspired by this project.
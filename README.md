# DeeL (Work In Progress)

An esoteric language that barely works at the moment.

# DISCLAIMER
This isn't actually a language persay, there are no types that actually build the language
or make it extendable right now.

# Information
There are 6 implicit data types in DeeL. 

 - Strings: just a string. They can be optionally delimited by a ' or ".
 - Numbers: a number, represented by a 64 bit integer.
 - Decimals: a floating point value.
 - Lists: an array of any of the above data types.
 - Booleans: true or false.
 - Dictionarys: an array of key-value pairs. Keys must be a String, Number or Decimal. Values can be any of the above, including nested dictionary's. 

## Goals

 1. Provide fast lexical analysis and parsing.
 3. Have a cool project!

## Examples
```
 # optional use of `'` or `"`
'string' = "string";
 # lists
'values' = [
    'string value',
    69.6969, # decimal values
    9223372036854775806 # integer values
]; 
```

### Dictionarys
Json set the standard, so they have to be included.
```
'name-to-person' = {
    'deeton': {
	    'age': 19,
	    'country': 'England'
	},
};
```

### Predefined variables
```
'working-directory' = $CurrentWorkingDirectory;
```

These are defined in `DVariables.cs`, the identifier doesn't have to start with '$'.
The dictionary called `Variables` contains key-value pairs. If the key matches the identifier, the token
and value will replace the identifier token. This is all happens during parsing.

I plan to add a way to add these from the commandline.

### Functions (builtin)

Small builtin functions can provide cross-platform, dynamic values. They can also prevent
doing extra work at runtime.

The `relative` function for example, will take a relative path and convert it to an absolute
path before your code even sees it.

Checkout builtin docs [here](https://github.com/DeetonRushy/DeeL/blob/master/functions/README.md)!

### Custom Interpreter Info

To implement your own interpreter, all you need to do is implement `ISyntaxTreeVisitor<T>`.

You will need to implement your own way of working with a `FunctionCall`

## This isn't too serious
I'm making this to challenge myself. That being said, this isn't going to be the fastest, most reliable configuration language out there. However, it's been fun to work on up to now so it's worth it.

## Credits
[Jay Madden](https://github.com/Jay-Madden) with their project [SharpLox](https://github.com/Jay-Madden/SharpLox).
The entire design was inspired by this project.
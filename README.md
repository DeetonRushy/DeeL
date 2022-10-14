# DeeL (Work In Progress)

A lightweight configuration "language". Including json inspired syntax, custom interpreter support and optional string delimiters. 

There are 6 data types in DeeL. 

 - Strings: just a string. They can be optionally delimited by a ' or ".
 - Numbers: a number, represented by a 64 bit integer.
 - Decimals: a floating point value.
 - Lists: an array of any of the above data types.
 - Booleans: true or false.
 - Dictionarys: an array of key-value pairs. Keys must be a String, Number or Decimal. Values can be any of the above, including nested dictionary's. 

## Goals

 1. Provide fast lexical analysis and parsing.
 2. Provide an intuitive C# API for interfacing with the configuration data.
 3. Make a cool project!

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
The dictionary called `Variables` contains key-value pairs. If they key matches, the token
and value will replace the identifier token.

I plan to add a way to add these from the commandline.
## This isn't too serious
I'm making this to challenge myself. That being said, this isn't going to be the fastest, most reliable configuration language out there. However, it's been fun to work on up to now so it's worth it.

## Credits
[Jay Madden](https://github.com/Jay-Madden) with their project [SharpLox](https://github.com/Jay-Madden/SharpLox).
The entire design was inspired by this project.
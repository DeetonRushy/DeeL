# DeeL (Work In Progress)

A programming language that is getting there bit by bit.

# Features
see [here](https://github.com/DeetonRushy/DeeL/blob/master/runtime/README.md)

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
 2. Nice flowing syntax for fast development.
 3. Have a cool project!


### Custom Interpreter Info

To implement your own interpreter, all you need to do is implement `ISyntaxTreeVisitor<T>`.

You will need to implement your own way of working with a `FunctionCall`

## Looking to contribute?

Go for it. If you're new or a veteran, help is very much welcome.
Just fork the project and make your mark :^)

### List of things that would be really useful
 - README that shows each feature of the language (create an issue and I can make a list, or try the language and mess about!)
 - The projects main README is lackluster and could use some touchups!
 - The DeeL runtime could use some new ideas!

Never contributed before? Check out [this](https://www.dataschool.io/how-to-contribute-on-github/) content for a quick guide.

### Debugging

When working on the runtime, there is a builtin statement called '__break' that, when hit, will
cause a debugger to attach and break.

#### Example
```
__break; # <-- causes a break, then the next step will be your code.
my_new_feature_that_isnt_working()
```

## Credits
[Jay Madden](https://github.com/Jay-Madden) with their project [SharpLox](https://github.com/Jay-Madden/SharpLox).
The entire design was inspired by this project.
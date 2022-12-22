
# Core builtin objects

## Console (Console.cs)

### Attributes
 - Static
 - Limited functionality

### Methods
 - input: get user input from the console via a prompt
    - Arg0: The prompt to display to the user. [optional, default=">>"]
  - enable: enable the console
  - disable: disable the console
  - is_disabled: returns true if the console is disabled via disable(), otherwise false
  - clear: clears the console buffer using `System.Console.Clear()`

## Lang (Interop.cs)

### Attributes
 - Static
 - Purpose is to interop with C#.


### Methods
 - get_native_function(Namespace, MethodName): get a handle to a native function (C# method)
    - Arg0: The type (including namespace). Ex: `Lang::get_native_function('System.Console', 'WriteLine')`
    - Arg1: The method name
  - quit(code): exits the process immediately upon calling.
     - Arg0: the exit code for the process [optional, default=0]
  -  enable_option(opt): set an interpreter configuration option
     - Arg0: the option to enable (docs soon)
  - module_name(): the module name within the current context
  - time(code): get a time value
      - Code: 0 is millis, 1 is seconds, 2 is minutes, 3 is hours and 4 is days. These values correspond to a property of the current time.
  - get_native_types(): get all loaded C# types in the executing module.
  - 
## String (String.cs)
### Attributes
 - Static (for now, will be bootstrapped)


### Methods
 - format(fmt, ...): format a string
     - Arg0: The format string. (format is `my name is {}`, with the braces being replaced)
     - Arg1...: The variadic arguments
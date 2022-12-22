# The DeeL runtime. This is the lexer, parser and interpreter.

## Language Features as of (22/12/2022)

### Assignment
You can create variables using `let`.
When using let, you can specify type annotations that are enforced when present.

```
let me: string = 19 # ERROR! cannot assign type `string` to type `int`
let me: string = "deeton"

let anything = [1, 2, 3]
```

### Module Identifiers
You can identify your module by using the `mod` keyword.

```
mod 'my.module'
```

### Module system (based on file names)
You can import files that have been specified in the configuration.
The Configuration cannot yet be modified externally, but this currently points towards `project-root/STD`

Every file matching `*.dl` in specified directorys will be available.
To import a file that is available, you need to use either a `from` and/or `import` statement

The filename specified does not include the extension too.

#### From-Import syntax
```
from 'std.time' import {*}
from 'std.threading' import { Thread, ThreadState }
```

#### Import-Into-Namespace syntax
```
let threading = import 'std.threading'
```

### Conditional execution
This is currently kind of shit, but improvements will come.

#### If-Else
```
let result: bool = getSomeResult()
if (result == true) {
    ...
}
else {
    ...
}
```

#### While
```
let i: int = 0
while (i != 10) {
    i = i + 1
    print('Iteration #{}', i)
}
```

### Functions
Reusable subroutines that can accept arguments & return values.

Standalone functions with arguments **MUST** specify a type annotation on each parameter.
All functions must specify a return type.

Anything declared within a functions scope will drop out of scope once the functions execution ends.

```
fn makeUser(name: string, age: int) -> dict {
    return {'name': name, 'age': age}
}
```

### Objects / Structs
DeeL has objects. They cannot inherit from each other. However, they can have static methods, member functionsand propertys.

They are not fixed and can be dynamically assigned to. Although, any property or function that is present
in the declaration is fixed and will always be present.

Static & not-static functions are determined by the presence of a `self` parameter. If `self`
is present, the function is a member function. When called, `self` will be the instance that it has
been called on.

If `self` is not present, the method is static.
`self` does not require a type annotation and can be declared as `const`

#### Basic object declaration
```
object MyObject {

}
```

#### Define a constructor
```
object MyObject {
    fn construct(self) -> void {
        self.a = 2
    }
}
```

#### Define a couple of propertys
```
object MyObject {
    property count: int
    property *instances: int

    fn construct(self) -> void {
        self.count = 0
        # sadly this is how it has to be done right now lol
        MyObject::instances = MyObject::instances + 1
    }
}
```

Property syntax is as follows:
`property [*] <ident>: <annotation> [= initializer]`
The `*` signifies a static property.

#### Define a method to interact with the propertys
```
object MyObject {
    property count: int
    property *instances: int

    fn construct(self) -> void {
        self.count = 0
        # sadly this is how it has to be done right now lol
        MyObject::instances = MyObject::instances + 1
    }

    fn increment(self) -> void {
        self.count = self.count + 1
    }
}
```

That's a pretty useless object.

### Const-ness

#### Let-Const
You can declare something as const. This means that the value is not modifiable.
```
let const age: int = 19
```

This works with object instances too, it will basically make each property read-only for that
reference of the class. (unless it was declared const, then its totally const and cannot be modified ever)

#### Const-params
When parameters are declared as const, it's a way of enforcing that the function does not
edit the parameter. (because it cannot when declared const)

When a const parameter is initialized, that singular reference is const for the time that
it exists within that scope.

```
fn takeObject(const obj: MyObject) -> void {
    obj.count = 0 # ERROR! cannot modify constant instance
    print('{}', obj.count) # FINE! can read the objects propertys.
}
```

### Argument semantics
By default arguments are mutable references.
When declared as a const argument, they are a readonly reference.

### Debugging
The builtin statement `__break` will cause a debugger to attach and break.
Then you can follow the interpreter with the debugger.
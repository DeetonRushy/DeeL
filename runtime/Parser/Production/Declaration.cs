﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runtime.Parser.Production;

public record Declaration : Statement
{
    public override string Debug()
    {
        throw new NotImplementedException();
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        throw new NotImplementedException();
    }
}

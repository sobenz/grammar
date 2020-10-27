using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace TargetingTestApp.Grammar
{
    public interface IExpressionParser
    {
        Expression GenerateExpression(string text);
    }
}

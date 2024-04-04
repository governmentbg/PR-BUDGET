using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Shared.ExpressionEngine
{
    public class NodeVariable : Node
    {
        public NodeVariable(string variableName)
        {
            _variableName = variableName;
        }

        string _variableName;

        public override double Eval(IContext ctx)
        {
            return ctx.ResolveVariable(_variableName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Shared.ExpressionEngine
{
    class NodeNumber : Node
    {
        public NodeNumber(double number)
        {
            _number = number;
        }

        double _number;             // The number

        public override double Eval(IContext ctx)
        {
            // Just return it.  Too easy.
            return _number;
        }
    }
}

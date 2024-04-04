using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Shared.ExpressionEngine
{
    class NodeUnary : Node
    {
        // Constructor accepts the two nodes to be operated on and function
        // that performs the actual operation
        public NodeUnary(Node rhs, Func<double, double> op)
        {
            _rhs = rhs;
            _op = op;
        }

        Node _rhs;                              // Right hand side of the operation
        Func<double, double> _op;               // The callback operator

        public override double Eval(IContext ctx)
        {
            // Evaluate RHS
            var rhsVal = _rhs.Eval(ctx);

            // Evaluate and return
            var result = _op(rhsVal);
            return result;
        }
    }
}

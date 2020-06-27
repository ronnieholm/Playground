using System;
using System.Collections.Generic;

namespace ExpressionTrees
{
    abstract class Expr
    {
        public abstract int Eval(Dictionary<string, int> env);
        public abstract Expr Deriv(string var);
    }

    class Mult : Expr
    {
        Expr _l;
        Expr _r;

        public Mult(Expr l, Expr r)
        {
            _l = l;
            _r = r;
        }

        public override Expr Deriv(string var)
        {
            return new Plus(
                new Mult(_l.Deriv(var), _r),
                new Mult(_l, _r.Deriv(var)));
        }

        public override int Eval(Dictionary<string, int> env) => _l.Eval(env) * _r.Eval(env);
        public override string ToString() => $"({_l} * {_r})";
    }

    class Plus : Expr
    {
        Expr _l;
        Expr _r;

        public Plus(Expr l, Expr r)
        {
            _l = l;
            _r = r;
        }

        public override Expr Deriv(string var) => new Plus(_l.Deriv(var), _r.Deriv(var));
        public override int Eval(Dictionary<string, int> env) => _l.Eval(env) + _r.Eval(env);
        public override string ToString() => $"({_l} + {_r})";
    }

    class Const : Expr
    {
        int _val;

        public Const(int val) => _val = val;
        public override Expr Deriv(string var) => new Const(0);
        public override int Eval(Dictionary<string, int> env) => _val;
        public override string ToString() => _val.ToString();
    }

    class Var : Expr
    {
        string _name;

        public Var(string name) =>_name = name;
        public override Expr Deriv(string var) => _name == var ? new Const(1) : new Const(0);
        public override int Eval(Dictionary<string, int> env) => env[_name];
        public override string ToString() => _name;
    }

    class Program
    {
        static void Main(string[] args)
        {
            // 3 * (y + x)
            var e1 = 
                new Mult(
                    new Const(3),
                    new Plus(
                        new Var("y"),
                        new Var("x")
                    )
                );

            // 3 * y + x
            var e2 =
                new Plus(
                    new Mult(
                        new Const(3),
                        new Var("y")
                    ),
                    new Var("x")
                );

            Console.WriteLine(e1);
            Console.WriteLine(e2);

            var env = new Dictionary<string, int>
            {
                { "x", 2 },
                { "y", 4 }
            };

            Console.WriteLine(e1.Eval(env));
            Console.WriteLine(e2.Eval(env));

            // x^2 + xy + y^3
            var e3 =
                new Plus(
                    new Mult(
                        new Var("x"),
                        new Var("x")
                    ),
                    new Plus(
                        new Mult(
                            new Var("x"),
                            new Var("y")
                        ),
                        new Mult(
                            new Var("y"),
                            new Mult(
                                new Var("y"),
                                new Var("y")
                            )
                        )
                    )
                );

            Console.WriteLine(e3);
            var e3x = e3.Deriv("x");
            var e3y = e3.Deriv("y");
            Console.WriteLine(e3x);
            Console.WriteLine(e3y);

            // Compute slope at point (1, 1)
            env = new Dictionary<string, int>
            {
                { "x", 1 },
                { "y", 1 }
            };     

            Console.WriteLine(e3x.Eval(env)); // 3
            Console.WriteLine(e3y.Eval(env)); // 4

            // For recursive descent, because slope in y direction is steeper,
            // we'd move in that direction. Then redo the calculation and move
            // accordingly until slope becomes zero and we've reached the flat
            // part.
        }
    }
}

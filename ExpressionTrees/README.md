# ExpressionTrees

Based on [Coding Trees in Python - Computerphile](https://www.youtube.com/watch?v=7tCNu4CnjVc)
and [Slopes of Machine Learning - Computerphile](https://www.youtube.com/watch?v=Jd55bul1VHo).

The interesting thing with trees is writing tree processors. We'd feed a tree
into a processor and get a tree out. This example is with evaluating expression
trees by computing partial derivatives.

For this object oriented version, ```eval``` and ```deriv``` are overriden
methods on each type of node. With structured programming, each type would hold
only the data and ```eval``` and ```deriv``` would be free functions, switching
on the type of three node, calling itself recursively.
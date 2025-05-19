# Monte Carlo simulation of a project schedule

Given a set of tasks on the [critical path][critical-path-method], a single
number estimate (in whatever unit of measure) is meaningless on its
own. Estimates are better expressed in terms of confidence intervals. The [Monte
Carlo method][monte-carlo-method] derives such confidence intervals.

## How the simulation works

1. For each task
   a. Transform the triangular distribution defined by optimistic, most likely,
      and pessimistic estimates into a normal distribution.
   b. For each simulation run, randomly sample estimates from the normal
      distributions.
2. Sum estimates across tasks by simulation run.
3. Bucket sum estimates by occurrence count.
4. Normalize occurrence count to the interval ]0;1] to form a probability
   distribution.

As each task's estimate is represented by a normal distribution, given a large
number of simulation runs, the combined estimate also forms a normal
distribution.

[critical-path-method]: https://en.wikipedia.org/wiki/Critical_path_method
[monte-carlo-method]: https://web.williams.edu/Mathematics/sjmiller/public_html/105Sp10/handouts/MetropolisUlam_TheMonteCarloMethod.pdf

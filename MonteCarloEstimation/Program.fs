open System
open type System.Math
open Plotly.NET

let simulationRuns = 100_000

type Task_ =
    { Name: string
      Optimistic: int
      MostLikely: int
      Pessimistic: int
      SimulationRuns: float[] }

let createTask name optimistic mostLikely pessimistic =
    { Name = name
      Optimistic = optimistic
      MostLikely = mostLikely
      Pessimistic = pessimistic
      SimulationRuns = Array.zeroCreate simulationRuns }    

// Tasks on the critical path.
let tasks =
    [ createTask "Task01" 10 15 20
      createTask "Task02" 9 15 20
      createTask "Task03" 5 7 10
      createTask "Task04" 5 8 11
      createTask "Task05" 5 8 11
      createTask "Task06" 5 8 11
      createTask "Task07" 3 4 5
      createTask "Task08" 4 7 11
      createTask "Task09" 4 9 12
      createTask "Task10" 10 15 20
      createTask "Task11" 5 10 15
      createTask "Task12" 15
      createTask "Task13" 14 18 22    
      createTask "Task14" 5 7 11 ]

// let normalDistribution mean stddev  x=
//     (1. / (stddev * Sqrt(2. * PI))) * Exp(-0.5 * Pow((x - mean) / stddev, 2.))

// Use triangular distribution to derive the mean and standard deviation for uniform distribution.
let triangularDistribution optimistic mostLikely pessimistic =
    let mean = (optimistic + (4. * mostLikely) + pessimistic) / 6.
    let stddev = (pessimistic - optimistic) / 6.
    mean, stddev

let rng = Random()

let boxMullerTransform () =
    let u1 = rng.NextDouble()
    let u2 = rng.NextDouble()
    Sqrt(-2. * Log(u1)) * Cos(2. * PI * u2)

let sample mean stddev z =
    mean + stddev * z    

for task in tasks do
    let mean, stddev = triangularDistribution task.Optimistic task.MostLikely task.Pessimistic
    for s in 0 .. simulationRuns - 1 do
        task.SimulationRuns[s] <- boxMullerTransform () |> sample mean stddev
   
let estimates =
    [| for s in 0 .. simulationRuns - 1 do
           tasks
           |> List.sumBy _.SimulationRuns[s] |]

let estimatesProbabilities =
    estimates
    |> Array.map (Math.Round >> int)
    |> Array.groupBy id
    |> Array.map (fun (estimate, occurrence) -> estimate, float (Array.length occurrence) / (float simulationRuns))
    |> Array.sortBy fst

let mean = float (estimates |> Array.sum) / float (Array.length estimates)
let stddev =
    let variance = 
        estimates 
        |> Array.map (fun x -> (float x - mean) ** 2.0)
        |> Array.sum
        |> fun sum -> sum / float (Array.length estimates)
    Sqrt(variance)

let cumulativeDistribution =
    let mutable cumulative = 0.
    estimatesProbabilities
    |> Array.map (fun (estimate, probability) ->
        cumulative <- cumulative + probability
        estimate, cumulative)

let normal =
    Chart.Scatter(
        estimatesProbabilities |> Array.map fst,
        estimatesProbabilities |> Array.map snd,
        StyleParam.Mode.Lines_Markers)
    |> Chart.withTitle("Monte Carlo simulation of schedule (normal)")
    |> Chart.withXAxisStyle($"Development hours (mean: {mean:F1}, stddev: {stddev:F1})")
    |> Chart.withYAxisStyle("Probability")
    |> Chart.saveHtml("normal-distribution.html")

let cumulative =
    Chart.Scatter(
        cumulativeDistribution,
        StyleParam.Mode.Lines_Markers)
    |> Chart.withTitle("Monte Carlo simulation of schedule (cumulative)")
    |> Chart.withXAxisStyle($"Development hours (mean: {mean:F1}, stddev: {stddev:F1})")
    |> Chart.withYAxisStyle("Probability")
    |> Chart.saveHtml("cumulative-distribution.html")

printfn "Open normal-distribution.html and cumulative-distribution.html in browser."

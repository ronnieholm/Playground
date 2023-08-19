# CSharpVsGoHashmapPerformance

Simple comparison of C# vs Go hashmap performance.

## Results

With C#, populating and querying the hashmap is faster, and overall the program
uses less memory. User time and percent CPU of .NET app reflects CLR startup
time.

## Execution

```bash
$ cd csharp
$ /bin/time --verbose dotnet run --configuration release
446 1496 999066
        Command being timed: "dotnet run --configuration release"
        User time (seconds): 6.45
        System time (seconds): 0.52
        Percent of CPU this job got: 164%
        Elapsed (wall clock) time (h:mm:ss or m:ss): 0:04.24
        Average shared text size (kbytes): 0
        Average unshared data size (kbytes): 0
        Average stack size (kbytes): 0
        Average total size (kbytes): 0
        Maximum resident set size (kbytes): 409668
        Average resident set size (kbytes): 0
        Major (requiring I/O) page faults: 6503
        Minor (reclaiming a frame) page faults: 155574
        Voluntary context switches: 4254
        Involuntary context switches: 2967
        Swaps: 0
        File system inputs: 0
        File system outputs: 48
        Socket messages sent: 0
        Socket messages received: 0
        Signals delivered: 0
        Page size (bytes): 4096
        Exit status: 0
```

```bash
$ cd go
$ /bin/time --verbose ./main
3777 1586 999965
        Command being timed: "./main"
        User time (seconds): 3.92
        System time (seconds): 1.73
        Percent of CPU this job got: 104%
        Elapsed (wall clock) time (h:mm:ss or m:ss): 0:05.42
        Average shared text size (kbytes): 0
        Average unshared data size (kbytes): 0
        Average stack size (kbytes): 0
        Average total size (kbytes): 0
        Maximum resident set size (kbytes): 639160
        Average resident set size (kbytes): 0
        Major (requiring I/O) page faults: 0
        Minor (reclaiming a frame) page faults: 309463
        Voluntary context switches: 2783
        Involuntary context switches: 472
        Swaps: 0
        File system inputs: 0
        File system outputs: 0
        Socket messages sent: 0
        Socket messages received: 0
        Signals delivered: 0
        Page size (bytes): 4096
        Exit status: 0
```

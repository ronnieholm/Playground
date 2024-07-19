// dotnet run --configuration Release
//
// Array of structs
// 00:00:00.3809360
// Struct of arrays
// 00:00:00.4345043
//
// Opposite result of Go program.

using System;
using System.Diagnostics;

static void RunArrayOfStructs(int count)
{
    var particles = new Particle[count];
    for (var i = 0; i < count; i++)
    {
        particles[i].x = 1;
        particles[i].y = 2;
        particles[i].z = 3;
        particles[i].vx = 4;
        particles[i].vy = 5;
        particles[i].vz = 6;
    }

    var sw = new Stopwatch();
    sw.Start();

    for (var i = 0; i < count; i++)
    {
        particles[i].x += particles[i].vx;
        particles[i].y += particles[i].vy;
        particles[i].z += particles[i].vz;
    }

    var elapsed = sw.Elapsed;
    Console.WriteLine(elapsed);
}

static void RunStructOfArrays(int count)
{
    var p = new ParticleSoa
    {
        x = new float[count],
        y = new float[count],
        z = new float[count],
        vx = new float[count],
        vy = new float[count],
        vz = new float[count]
    };

    for (var i = 0; i < count; i++)
    {
        p.x[i]= 1;
        p.y[i]= 2;
        p.z[i]= 3;
        p.vx[i] = 4;
        p.vy[i] = 5;
        p.vz[i] = 6;
    }

    var sw = new Stopwatch();
    sw.Start();

    for (var i = 0; i < count; i++)
    {
        p.x[i] += p.vx[i];
        p.y[i] += p.vy[i];
        p.z[i] += p.vz[i];
    }

    var elapsed = sw.Elapsed;
    Console.WriteLine(elapsed);
}

System.Console.WriteLine("Array of structs");
RunArrayOfStructs(100_000_000);
System.Console.WriteLine("Struct of arrays");
RunStructOfArrays(100_000_000);

struct Particle {
    public float x, y, z;
    public float vx, vy, vz;
}

struct ParticleSoa {
    public float[] x, y, z;
    public float[] vx, vy, vz;
}
// Copyright (c) 2017 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

namespace FsRandom

open System

/// A `System.Random` descendant that uses PCG-32.
/// It's not recommended to be used from F# projects.
type Pcg32Random(state) = 
    inherit Random()
    let mutable state = state

    /// Constructs a `Pcg32Random` object with the given seed.
    /// The state is constructed using `Pcg32.createOneSeq`.
    new (seed) = Pcg32Random(seed |> Pcg32.createOneSeq)

    /// Constructs a `Pcg32Random` object with the given seed and stream index
    new (seed, initSeq) = 
        let newSeed = (seed, initSeq) ||> Pcg32.create
        Pcg32Random (newSeed)

    member private x.InternalSample() =
        let result, newState = Pcg32.get state
        state <- newState
        result
    
    override x.Next() = x.InternalSample() |> int

    override x.Sample() = (x.InternalSample() |> float) * (1.0 / 2147483647.0)

    override x.Next(min, max) = ((min-max) |> abs |> x.Next) + Math.Min(max,min);

    override x.NextBytes(buffer) = 
        for i = 0 to buffer.Length - 1 do 
            buffer.[i] <- (x.Next(Byte.MaxValue |> int) |> byte)
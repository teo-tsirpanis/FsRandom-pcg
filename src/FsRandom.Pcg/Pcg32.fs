// Copyright (c) 2017 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

namespace FsRandom

open System

[<NoComparison>]
/// A PCG-32 state. It consists of two unsigned 64-bit numbers.
type Pcg32State = private Pcg of state:uint64 * inc:uint64

/// Functions to deal with `Pcg32State`s.
module Pcg32 =

    [<Literal>]
    let private DefaultMultiplier = 6364136223846793005UL
    [<Literal>]
    let private DefaultIncrement = 1442695040888963407UL

    let private stepState (Pcg(state, inc)) = state * DefaultMultiplier + inc

    let private setState (Pcg(_, inc)) newState = (newState, inc) |> Pcg

    let private outputPermutation (state: uint64) =
        let rotr (value: uint32) (rot: uint32) =
            let rot = rot |> int
            (value >>> rot) ||| (value <<< ((-rot) &&& 31))
        let newState = ((state >>> 18) ^^^ state) >>> 27 |> uint32
        let rotValue = state >>> 59 |> uint32
        rotr newState rotValue

    /// Gets the stream index of a state.
    let getInc (Pcg (_, x)) = x

    /// Generates a random number from theprovided state.
    /// Returns a tuple consisting of the new number and the new state.
    let get state =
        let newState = state |> stepState |> (setState state)
        let (Pcg(oldState, _)) = state
        oldState |> outputPermutation, newState

    /// Advances a PCG-32 state forward by `delta` steps, but does so in logarithmic time.
    let advance (Pcg(state, inc)) delta =
        (LcgAdvance.advance32 state delta DefaultMultiplier inc, inc)
        |> Pcg
    
    /// Moves the PCG-32 backwards by `delta` steps, but does so in logarithmic time.
    let backstep state delta = advance state (UInt64.MaxValue - delta)

    /// Creates a PCG-32 state from the given seed and stream index.
    let create seed initSeq =
        let inc = (initSeq <<< 1) ||| 1UL
        let makeState seed = (seed, inc) |> Pcg
        makeState 0UL |> stepState |> ((+) seed)
                      |> makeState |> stepState |> makeState

    /// Creates a PCG-32 state from the given seed.
    /// If you don't care about the stream index,
    /// it's better to call this, instead of `create seed 0`.
    let createOneSeq seed = create seed DefaultIncrement

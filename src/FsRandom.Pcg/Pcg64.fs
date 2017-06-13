// Copyright (c) 2017 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

namespace FsRandom

#nowarn "10001"

open SoftWx.Numerics
open System
open System.Numerics
open System.Security.Cryptography

/// Functions to deal with `UInt128`s.
module UInt128 =

    /// Returns the low part of a `UInt128`.
    let low (x: UInt128) = x.Low

    /// Returns the hogh part of a `UInt128`.
    let high (x:UInt128) = x.High

    /// Returns a cryptographically random `UInt128`.
    /// Useful for seeding PCG-64.
    let random () =
        let random64 () =
            let rng = RandomNumberGenerator.Create()
            let bytes = Array.zeroCreate 8
            rng.GetBytes bytes
            BitConverter.ToUInt64 (bytes, 0)
        UInt128 (random64(), random64())

[<NoComparison>]
/// A PCG-64 state. It consists of two `UInt128`s.
type Pcg64State = private Pcg of state:UInt128 * inc:UInt128

module Pcg64 =

    let private defaultMultiplier = UInt128(2549297995355413924UL, 4865540595714422341UL)

    let private defaultIncrement = UInt128(6364136223846793005UL, 1442695040888963407UL)

    let private setState (Pcg(_, inc)) newState = (newState, inc) |> Pcg

    let private stepState (Pcg(inc, state)) = state * defaultMultiplier + inc

    let private outputPermutation (state: UInt128) =
        let rotr (value: uint64) (rot: uint32) =
            let rot = rot |> int
            (value >>> rot) ||| (value <<< ((-rot) &&& 63))
        let state' = state.High ^^^ state.Low |> uint64
        let rotValue = state >>> 122 |> uint64 |> uint32
        rotr state' rotValue
    
    /// Gets the stream index of a state.
    [<CompiledName("GetInc")>]
    let getInc (Pcg(_, inc)) = inc

    /// Generates a random number from theprovided state.
    /// Returns a tuple consisting of the new number and the new state.
    [<CompiledName("Get")>]
    let get state =
        let state = state |> stepState |> setState state
        let (Pcg(state', _)) = state
        state' |> outputPermutation, state

    /// Advances a PCG-64 state forward by `delta` steps, but does so in logarithmic time.
    [<CompiledName("Advance"); CompilerMessage("This method is known for not working.", 10001, IsHidden=false, IsError=false)>]
    let advance delta (Pcg(state, inc)) =
        (LcgAdvance.advance128 state delta defaultMultiplier inc, inc)
        |> Pcg
    
    /// Moves the PCG-64 state backwards by `delta` steps, but does so in logarithmic time.
    [<CompiledName("Backstep"); CompilerMessage("This method is known for not working.", 10001, IsHidden=false, IsError=false)>]
    let backstep delta state = advance (UInt128.op_UnaryNegation delta) state

    /// Creates a PCG-64 state from the given seed and stream index.
    [<CompiledName("Create")>]
    let create seed initSeq =
        let inc = (initSeq <<< 1) ||| UInt128.One
        let makeState seed = (seed, inc) |> Pcg
        makeState UInt128.Zero |> stepState |> ((+) seed)
                       |> makeState |> stepState |> makeState

    /// Creates a PCG-64 state from the given seed.
    /// If you don't care about the stream index,
    /// it's better to call this, instead of `create seed 0`.
    [<CompiledName("CreateOneSeq")>]
    let createOneSeq seed = create seed defaultIncrement

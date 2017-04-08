// Copyright (c) 2017 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

namespace FsRandom

open System

/// An unsigned 128-bit number.
/// It must be converted to a bigint in to access its value.
type UInt128 = private UInt128 of bigint
with
    override x.ToString() = match x with | UInt128 x -> x.ToString()

[<AutoOpen>]
/// Functions to deal with `UInt128`s.
module UInt128 =

    /// Number two raised in the 128th power, _as a `bigint`_.
    let ``2^128`` = 340282366920938463463374607431768211456I

    /// Converts a `bigint` to a `UInt128`.
    /// The absolute value of the bigint mod 2^128
    /// will be the value of the new `UInt128`.
    let uint128 (x: bigint) = x % ``2^128`` |> abs |> UInt128

    /// Converts a `UInt128` to a `bigint`.
    let bigint (UInt128 x) = x

    /// Applies a function to the value of a `UInt128`.
    /// A shortcut of calling `x |> bigint |> f`.
    let apply f (UInt128 x) = f x

    /// Applies a function that processes and returns a bigint
    /// to the value of a `UInt128`, and then converts it to a new `UInt128`.
    /// Useful for making operations with `UInt128`s.
    /// A shortcut of calling 'x |> apply f |> uint128'
    let map f x = (f, x) ||> apply |> uint128

    /// Number zero as a `UInt128`.
    let zero = 0I |> UInt128

[<NoComparison>]
/// A PCG-64 state. It consists of two `UInt128`s.
type Pcg64State = private Pcg of state:UInt128 * inc:UInt128

module Pcg64 =

    let private defaultMultiplier = 47026247687942121848144207491837523525I

    let private defaultIncrement = 117397592171526113268558934119004209487I

    let private setState (Pcg(_, inc)) newState = (newState, inc) |> Pcg

    let private stepState (Pcg(UInt128(inc), UInt128(state))) = state * defaultMultiplier + inc |> uint128

    let private outputPermutation (UInt128 state) =
        let rotr (value: uint64) (rot: uint32) =
            let rot = rot |> int
            (value >>> rot) ||| (value <<< ((-rot) &&& 63))
        let state' = (state >>> 64) ^^^ state |> uint64
        let rotValue = state >>> 122 |> uint32
        rotr state' rotValue

    /// Gets the stream index of a state.
    let getInc (Pcg(_, inc)) = inc

    /// Generates a random number from theprovided state.
    /// Returns a tuple consisting of the new number and the new state.
    let get state =
        let state = state |> stepState |> (setState state)
        let (Pcg(state', _)) = state
        state' |> outputPermutation, state

    /// Creates a PCG-64 state from the given seed and stream index.
    let create seed initSeq =
        let inc = (initSeq <<< 1) ||| 1I |> uint128
        let makeState seed = (seed, inc) |> Pcg
        makeState zero |> stepState |> (map ((+) seed))
                       |> makeState |> stepState |> makeState

    /// Creates a PCG-64 state from the given seed.
    /// If you don't care about the stream index,
    /// it's better to call this, instead of `create seed 0`.
    let createOneSeq seed = create seed defaultIncrement

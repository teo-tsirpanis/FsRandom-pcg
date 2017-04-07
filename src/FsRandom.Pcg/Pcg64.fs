// Copyright (c) 2017 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

namespace FsRandom

open System

type UInt128 = private UInt128 of bigint
with
    override x.ToString() = match x with | UInt128 x -> x.ToString()

[<AutoOpen>]
module UInt128 =

    let ``2^128`` = 340282366920938463463374607431768211456I

    let uint128 (x: bigint) = x % ``2^128`` |> abs |> UInt128

    let bigint (UInt128 x) = x

    let apply f (UInt128 x) = f x

    let map f x = (f, x) ||> apply |> uint128

    let zero = 0I |> UInt128

[<NoComparison>]
type Pcg64State = private Pcg of state:UInt128 * inc:UInt128

module Pcg64 =

    let defaultMultiplier = 47026247687942121848144207491837523525I

    let defaultIncrement = 117397592171526113268558934119004209487I

    let getInc (Pcg(_, inc)) = inc

    let private setState (Pcg(_, inc)) newState = (newState, inc) |> Pcg

    let stepState (Pcg(UInt128(inc), UInt128(state))) = state * defaultMultiplier + inc |> uint128

    let outputPermutation (UInt128 state) = 
        let rotr (value: uint64) (rot: uint32) = 
            let rot = rot |> int
            (value >>> rot) ||| (value <<< ((-rot) &&& 63))
        let state' = (state >>> 64) ^^^ state |> uint64
        let rotValue = state >>> 122 |> uint32
        rotr state' rotValue
    
    let get state =
        let state = state |> stepState |> (setState state)
        let (Pcg(state', _)) = state
        state' |> outputPermutation, state

    let create seed initSeq =
        let inc = (initSeq <<< 1) ||| 1I |> uint128
        let makeState seed = (seed, inc) |> Pcg
        makeState zero |> stepState |> (map ((+) seed)) 
                       |> makeState |> stepState |> makeState

    let createOneSeq seed = create seed defaultIncrement

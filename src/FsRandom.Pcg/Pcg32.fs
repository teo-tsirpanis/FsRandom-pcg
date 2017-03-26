// Copyright (c) 2017 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

namespace FsRandom

open System


[<NoComparison>]
type Pcg32State = Pcg of uint64 * uint64

module Pcg32 =

    [<Literal>]
    let DefaultMultiplier = 6364136223846793005UL
    [<Literal>]
    let DefaultIncrement = 1442695040888963407UL

    let stepState inc state = state * DefaultMultiplier + inc

    let outputPermutation (state: uint64) = 
        let rotr (value: uint32) (rot: uint32) = 
            let rot = rot |> int
            (value >>> rot) ||| (value <<< ((-rot) &&& 31))
        let state' = ((state >>> 18) ^^^ state) >>> 27
        let rotValue = state >>> 59 |> uint32
        rotr (uint32 state) rotValue

    let get x = 
        let pcg' (Pcg (state, inc)) =
            let step = stepState inc
            let getImpl state = 
                let newState = state |> step
                let out = state |> outputPermutation
                out, newState
            let out1, state1 = getImpl state
            let out2, state2 = getImpl state1
            let makeUInt64 (u1: uint32) u2 =
                let temp = [|u1; u2|] |> Array.collect BitConverter.GetBytes
                BitConverter.ToUInt64 (temp, 0)
            (out1, out2) ||> makeUInt64, ((state2, inc) |> Pcg)
        pcg' x
        
    let getInc (Pcg (_, x)) = x

    let setInc newInc (Pcg (state, _)) = (state, newInc) |> Pcg

    let mapInc f (Pcg (state, inc)) = (state, f inc) |> Pcg

    let create seed initSeq = 
        let mutable state = (0UL, initSeq <<< 1 ||| 1UL) |> Pcg
        let (Pcg (s, _)) = state |> get |> snd
        state <- ((s + seed), state |> getInc) |> Pcg
        state <- state |> get |> snd
        state

    let createOneSeq seed = create seed DefaultIncrement

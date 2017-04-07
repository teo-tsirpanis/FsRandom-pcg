// Copyright (c) 2017 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

namespace FsRandom

open System


[<NoComparison>]
type Pcg32State = private Pcg of state:uint64 * inc:uint64

module Pcg32 =

    [<Literal>]
    let DefaultMultiplier = 6364136223846793005UL
    [<Literal>]
    let DefaultIncrement = 1442695040888963407UL
    
    let getInc (Pcg (_, x)) = x

    let stepState (Pcg(state, inc)) = state * DefaultMultiplier + inc
        
    let private setState (Pcg(_, inc)) newState = (newState, inc) |> Pcg

    let outputPermutation (state: uint64) = 
        let rotr (value: uint32) (rot: uint32) = 
            let rot = rot |> int
            (value >>> rot) ||| (value <<< ((-rot) &&& 31))
        let newState = ((state >>> 18) ^^^ state) >>> 27 |> uint32
        let rotValue = state >>> 59 |> uint32
        rotr newState rotValue

    let get state =
        let getImpl state = 
            let newState = state |> stepState |> (setState state)
            let (Pcg(oldState, _)) = state
            oldState |> outputPermutation, newState
        let out1, state1 = getImpl state
        let out2, state2 = getImpl state1
        let out = (uint64 out1 <<< 64) + uint64 out2
        out, state2        

    let create seed initSeq = 
        let inc = (initSeq <<< 1) ||| 1UL
        let makeState seed = (seed, inc) |> Pcg
        makeState 0UL |> stepState |> ((+) seed)
                      |> makeState |> stepState |> makeState

    let createOneSeq seed = create seed DefaultIncrement

// Copyright (c) 2017 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

module TestLcgAdvance

open System
open Expecto
open FsCheck
open SoftWx.Numerics
open FsRandom

let testProperty = TestUtils.testProperty

let ftestProperty = TestUtils.ftestProperty

[<Tests>]
let properties =
    testList "LCG advancing tests" [
        testProperty "modExp64 works properly" <| fun a exp ->
            (exp >= 0) ==>
                lazy
                    (let exp = uint64 exp
                    LcgAdvance.modExp64 a exp = (Seq.replicate (int exp) a |> Seq.fold (*) 1UL))

        testProperty "modExp128 works properly" <| fun a exp ->
            (exp >= 0) ==>
                lazy
                (
                    let expected = exp |> uint64 |> UInt128.op_Implicit |> LcgAdvance.modExp128 a
                    let actual = Seq.replicate exp a |> Seq.fold (*) UInt128.One
                    expected = actual)
        
        testProperty "A 64-bit LCG advance of one step is essentially an LCG" <| fun state a b ->
            let expected = a * state + b
            let actual = LcgAdvance.advance64 state 1UL a b
            expected = actual
                    
        testProperty "A 128-bit LCG advance of one step is essentially an LCG" <| fun state a b ->
            let expected = a * state + b
            let actual = LcgAdvance.advance128 state UInt128.One a b
            expected = actual]
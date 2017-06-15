// Copyright (c) 2017 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

module TestPcg32

#nowarn "10001"

open Expecto
open FsCheck
open FsRandom
open FsRandom.LcgAdvance
open System

let testProperty = TestUtils.testProperty

[<Tests>]
let properties =
    testList "PCG-32 tests" [
        TestUtils.testProperty "advance is an inverse of backstep" <| fun x delta ->
            x |> Pcg32.advance delta |> Pcg32.backstep delta |> ((=) x)
            
        testProperty "advance 1 is the same thing with get" <| fun x ->
            let expected = x |> Pcg32.get |> snd
            let actual = x |> Pcg32.advance 1UL
            expected = actual
            
        testProperty "advance 0 does not change the state" <| fun x ->
            Pcg32.advance 0UL x = x
            
        testProperty "advance is distributive" <| fun x d1 d2 ->
            x |> Pcg32.advance d1 |> Pcg32.advance d2 = Pcg32.advance (d1 + d2) x]
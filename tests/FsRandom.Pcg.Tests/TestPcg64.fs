// Copyright (c) 2017 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

module TestPcg64

#nowarn "10001"

open System
open Expecto
open FsCheck
open SoftWx.Numerics
open FsRandom

let testProperty = TestUtils.testProperty

let ftestProperty = TestUtils.ftestProperty

[<Tests>]
let properties =
    testList "PCG-64 tests" [
        testProperty "advance is an inverse of backstep" <| fun x delta ->
            x |> Pcg64.advance delta |> Pcg64.backstep delta |> ((=) x)
            
        testProperty "advance 1 is the same thing with get" <| fun x ->
            let expected = x |> Pcg64.get |> snd
            let actual = x |> Pcg64.advance UInt128.One
            expected = actual
            
        testProperty "advance 0 does not change the state" <| fun x ->
            Pcg64.advance UInt128.Zero x = x
            
        testProperty "advance is distributive" <| fun x d1 d2 ->
            x |> Pcg64.advance d1 |> Pcg64.advance d2 = Pcg64.advance (d1 + d2) x]
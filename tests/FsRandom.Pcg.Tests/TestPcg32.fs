// Copyright (c) 2017 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

module Tests

#nowarn "10001"

open System
open Xunit
open FsRandom
open FsRandom.LcgAdvance
open FsCheck.Xunit
open FsCheck

let genPcg32State = Arb.generate<uint64> |> Gen.two |> Gen.map ((<||) Pcg32.create)

type Generators =
    static member Pcg32State() = Arb.fromGen genPcg32State

[<Property>]
let ``modExp64 should work properly`` a exp =
    (a >= 0 && exp >= 0) ==>
        lazy
            (let a, exp = uint64 a, uint64 exp
            modExp64 a exp = (Seq.replicate (int exp) a |> Seq.fold (*) 1UL))

[<Property(Arbitrary = [|typeof<Generators>|])>]
let ``Pcg32.advance should be an inverse of backstep`` x delta =
    x |> Pcg32.advance delta |> Pcg32.backstep delta |> ((=) x)

[<Property(Arbitrary = [|typeof<Generators>|])>]
let ``Pcg32.advance 1 should be the same thing with Pcg32.get`` x =
    let expected = x |> Pcg32.get |> snd
    let actual = x |> Pcg32.advance 1UL
    expected = actual

[<Property(Arbitrary = [|typeof<Generators>|])>]
let ``Pcg32.advance 0 should not change the state`` x =
    Pcg32.advance 0UL x = x
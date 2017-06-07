// Copyright (c) 2017 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

module TestPcg64

open System
open FsCheck
open FsCheck.Xunit
open SoftWx.Numerics
open FsRandom
open FsRandom.LcgAdvance
open FsRandom.Pcg64

let genUInt128 = Arb.generate<uint64> |> Gen.two |> Gen.map UInt128

let rec shrinkUInt128 x = seq {
    if x <> UInt128.Zero then
        let x = x - UInt128.One
        yield x
        yield! shrinkUInt128 x
}

let genPcg64State = Arb.generate<UInt128> |> Gen.two |> Gen.map ((<||) create)

type Generators =
    static member UInt128() = Arb.fromGenShrink (genUInt128, shrinkUInt128)
    static member Pcg64State() = Arb.fromGen genPcg64State

[<Property(Arbitrary = [|typeof<Generators>|])>]
let ``modExp128 should work properly`` a exp = 
    (exp >= 0) ==>
        lazy
            (
                let expected = exp |> uint64 |> UInt128.op_Implicit |> modExp128 a
                let actual = Seq.replicate exp a |> Seq.fold (*) UInt128.One
                expected = actual
            )
        
[<Property(Arbitrary = [|typeof<Generators>|])>]
let ``Pcg64.advance should be an inverse of backstep`` x delta =
    x |> advance delta |> backstep delta |> ((=) x)

[<Property(Arbitrary = [|typeof<Generators>|])>]
let ``Pcg64.advance 1 should be the same thing with Pcg64.get`` x =
    let expected = x |> Pcg64.get |> snd
    let actual = x |> Pcg64.advance UInt128.One
    expected = actual

[<Property(Arbitrary = [|typeof<Generators>|])>]
let ``Pcg64.advance 0 should not change the state`` x =
    Pcg64.advance UInt128.Zero x = x
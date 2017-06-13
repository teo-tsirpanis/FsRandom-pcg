// Copyright (c) 2017 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT
[<AutoOpen>]
module TestUtils

open FsCheck
open FsCheck.Xunit
open FsRandom
open SoftWx.Numerics

let genUInt128 = Arb.generate<uint64> |> Gen.two |> Gen.map UInt128

let rec shrinkUInt128 x = seq {
    if x <> UInt128.Zero then
        let x = x - UInt128.One
        yield x
        yield! shrinkUInt128 x
}

type Generators =
    static member UInt128() = Arb.fromGenShrink (genUInt128, shrinkUInt128)
    static member Pcg32State() =
        Arb.generate<uint64>
        |> Gen.two
        |> Gen.map ((<||) Pcg32.create)
        |> Arb.fromGen
    static member Pcg64State() =
        Arb.generate<UInt128>
        |> Gen.two
        |> Gen.map ((<||) Pcg64.create)
        |> Arb.fromGen

type FsRandomPropertyAttribute() =
    inherit PropertyAttribute(Arbitrary = [|typeof<Generators>|])
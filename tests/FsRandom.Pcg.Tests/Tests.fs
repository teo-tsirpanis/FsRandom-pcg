module Tests

open System
open Xunit
open FsRandom.UInt128
open FsCheck.Xunit
open FsCheck
open SoftWx.Numerics

let genUInt128 = Arb.generate<uint64> |> Gen.two |> Gen.map UInt128

let rec shrinkUInt128 x = seq {
    yield x
    printf "Yielded %A" x
    if x <> UInt128.Zero then
        yield! x - UInt128.One |> shrinkUInt128
}

type UInt128Generator =
    static member UInt128() =
        {new Arbitrary<UInt128>() with
            override x.Generator = genUInt128
            override x.Shrinker y = Seq.empty}

[<Property>]
let ``I will succeed`` x = x >= 0u
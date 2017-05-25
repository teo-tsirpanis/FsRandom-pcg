module Tests

open System
open Xunit
open FsRandom.UInt128
open FsCheck.Xunit
open FsCheck
open SoftWx.Numerics

let genUInt128 = Arb.generate<uint64> |> Gen.map (fun x -> UInt128(x, 0UL))

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

// [<Property>]
[<Property(Arbitrary=[|typeof<UInt128Generator>|])>]
let ``A UInt128 can be reliably converted to a bigint and the other way round`` x = x = (x |> bigint |> uint128)
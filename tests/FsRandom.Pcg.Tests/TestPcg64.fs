// Copyright (c) 2017 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

module TestPcg64

open System
open FsCheck
open FsCheck.Xunit
open SoftWx.Numerics
open FsRandom.LcgAdvance
open FsRandom.Pcg64

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
let ``modExp128 should work properly`` a exp = 
    (a > 0 && a <= Int32.MaxValue && exp <= Int32.MaxValue && exp >= 0) ==>
        lazy
            (let a, exp = uint64 a, uint64 exp
            modExp128 (UInt128.op_Implicit a) (UInt128.op_Implicit exp) = (Seq.replicate (int exp) a |> Seq.fold (*) UInt128.One))
        
[<Property>]
let ``Pcg64.advance should be an inverse of backstep`` x dlo dhi =
    let delta = UInt128 (dhi, dlo)
    x |> advance delta |> backstep delta |> ((=) x)

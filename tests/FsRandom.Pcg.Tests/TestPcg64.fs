// Copyright (c) 2017 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

module TestPcg64

#nowarn "10001"

open System
open FsCheck
open FsCheck.Xunit
open SoftWx.Numerics
open FsRandom
open FsRandom.LcgAdvance
open FsRandom.Pcg64

[<FsRandomProperty>]
let ``modExp128 should work properly`` a exp = 
    (exp >= 0) ==>
        lazy
            (
                let expected = exp |> uint64 |> UInt128.op_Implicit |> modExp128 a
                let actual = Seq.replicate exp a |> Seq.fold (*) UInt128.One
                expected = actual
            )
        
[<FsRandomProperty>]
let ``Pcg64.advance should be an inverse of backstep`` x delta =
    x |> advance delta |> backstep delta |> ((=) x)

[<FsRandomProperty>]
let ``Pcg64.advance 1 should be the same thing with Pcg64.get`` x =
    let expected = x |> Pcg64.get |> snd
    let actual = x |> Pcg64.advance UInt128.One
    expected = actual

[<FsRandomProperty>]
let ``Pcg64.advance 0 should not change the state`` x =
    Pcg64.advance UInt128.Zero x = x

[<FsRandomProperty>]
let ``Pcg64.advance should be distributive`` x d1 d2 =
    x |> Pcg64.advance d1 |> Pcg64.advance d2 = Pcg64.advance (d1 + d2) x

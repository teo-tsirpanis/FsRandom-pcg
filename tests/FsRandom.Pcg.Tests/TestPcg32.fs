module Tests

open System
open Xunit
open FsRandom
open FsRandom.LcgAdvance
open FsCheck.Xunit
open FsCheck

[<Property>]
let ``modExp64 should work properly`` a exp =
    (a >= 0 && exp >= 0) ==>
        lazy
            (let a, exp = uint64 a, uint64 exp
            modExp64 a exp = (Seq.replicate (int exp) a |> Seq.fold (*) 1UL))

[<Property>]
let ``Pcg32.advance should be an inverse of backstep`` x delta =
    x |> Pcg32.advance delta |> Pcg32.backstep delta |> ((=) x)

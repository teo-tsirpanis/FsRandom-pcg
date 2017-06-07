// Copyright (c) 2017 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

namespace FsRandom

/// Contains functions that effeciently advance LCGs.
module LcgAdvance =

    open SoftWx.Numerics

    /// Efficiently calculates `a ^ exp mod 2 ^ 64`.
    [<CompiledName("ModExp64")>]
    let rec modExp64 a exp =
        if exp = 0UL then
            1UL
        elif exp &&& 1UL = 0UL then
            let x = exp >>> 1 |> modExp64 a
            x * x
        else
            exp - 1UL |> modExp64 a |> ((*) a)

    /// Efficiently calculates `a ^ exp mod 2 ^ 128`.
    [<CompiledName("ModExp128")>]
    let rec modExp128 a exp =
        let two = UInt128.One + UInt128.One
        if exp = UInt128.Zero then
            UInt128.One
        elif exp.Low % 2UL = 0UL then
            let x = exp >>> 1 |> modExp128 a
            x * x
        else
            exp - UInt128.One |> modExp128 a |> ((*) a)

    /// Efficiently advances a 64-bit LCG state with multiplier `a`, incrementer `b`, by `delta` steps.
    [<CompiledName("Advance64")>]
    let advance64 state delta a b =
        // let an = modExp64 a delta
        // an * state + (an - 1UL) * b / (a - 1UL)
        let mutable delta = delta
        let mutable curmult = a
        let mutable curplus = b
        let mutable accmult = 1UL
        let mutable accplus = 0UL
        while delta > 0UL do
            if delta &&& 1UL <> 0UL then
                accmult <- accmult * curmult
                accplus <- accplus * curmult + curplus
            curplus <- (curmult + 1UL) * curplus
            curmult <- curmult * curmult
            delta <- delta >>> 1
        accmult * state + accplus

    /// Efficiently advances a 128-bit LCG state with multiplier `a`, incrementer `b`, by `delta` steps.
    [<CompiledName("Advance128")>]
    let advance128 state delta a b =
        // let an = modExp128 a delta
        // an * state + ((an - UInt128.One) / (a - UInt128.One)) * b
        let mutable delta = delta
        let mutable curmult = a
        let mutable curplus = b
        let mutable accmult = UInt128.One
        let mutable accplus = UInt128.Zero
        while delta > UInt128.Zero do
            if delta &&& UInt128.One <> UInt128.Zero then
                accmult <- accmult * curmult
                accplus <- accplus * curmult + curplus
            curplus <- (curmult + UInt128.One) * curplus
            curmult <- curmult * curmult
            delta <- delta >>> 1
        accmult * state + accplus
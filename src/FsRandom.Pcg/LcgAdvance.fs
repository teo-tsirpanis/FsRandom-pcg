// Copyright (c) 2017 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

namespace FsRandom

module internal LcgAdvance =

    let advance32 state delta cmult cplus =
        let rec loopIt delta amult aplus cmult cplus =
            let amult, aplus =
                if delta % 2UL = 0UL then
                    amult * cmult, aplus * cmult + cplus
                else
                    amult, aplus
            let cplus = (cmult + 1UL) * cplus
            let cmult = cmult * cmult
            if delta <> 0UL then
                loopIt (delta / 2UL) amult aplus cmult cplus
            else
                amult, aplus
        let amult, aplus = loopIt delta 1UL 0UL cmult cplus
        amult * state + aplus
# The PCG Pseudo-Random Number Generator for F# #

[![NuGet](https://img.shields.io/nuget/v/FsRandom.Pcg.svg)](https://www.nuget.org/packages/FsRandom.Pcg)
[![AppVeyor](https://img.shields.io/appveyor/ci/teo-tsirpanis/FsRandom-Pcg.svg?style=flat-square)](https://ci.appveyor.com/project/teo-tsirpanis/fsrandom-pcg)

## About PCG

PCG is a family of simple fast space-efficient statistically good algorithms for random number generation. Unlike many general-purpose RNGs, they are also hard to predict. You can read more about PCG [here][pcghome].

## `System.Random` is not so good

`System.Random` has some problems that might be unsuitable for some users.
Specifically:

* It uses an obscure algorithm to generate random numbers.

    [According to Microsoft][msdn]:
    > The current implementation of the Random class is based on Donald E. Knuth's subtractive random number generator algorithm. For more information, see D. E. Knuth. "The Art of Computer Programming, volume 2: Seminumerical Algorithms". Addison-Wesley, Reading, MA, second edition, 1981.

    Try to google it. And then, try to google "Mersenne Twister" or "linear congruential generator". Did you see the diference? The obscurity of this algorithm can be important. While it is designed by a well-respected computer scientist, it's very possible that its output is not random enough, or even worse, it can be easily predicted. Imagine that you are playing a game, and your opponent can predict the roll of a dice or the order of a shuffled deck. [Predictability][predictability] directly contradicts the very definition of randomness. While no software can generate completely random material, it can give a decent try; and it's doing well. Furthermore, [`System.Random`'s implementation has a serious defect][randombug].

* This is more specific to F# users, but, regardless of the underlying algorithm, the `System.Random` class has mutable state, an antipattern of functional programming. For example, with immutable values, generating the same random stream again might be a little hard in a stateful, imperative language like C#. Uses of `System.Random` have an inherent cost of adding side effects, something functional programmers are scared of. But don't worry. F# has [a good library][fsrandom] for functional random number generation.

## PCG is better

Taken from [here][pcghome] (modified):

* It's really easy to use, and yet its very flexible and offers [powerful features][pcgfeatures] (not all of them are yet available on this library) (including some that allow you to perform [silly party tricks][partytricks]).
* It's very fast, and can occupy very little space (only 32 bytes of state, in comparison with `System.Random`'s 232 bytes of state, and with Mersenne Twister's 2.5 KiB of internal state).
* It has small code size.
* It's performance in [statistical tests][statisticaltests] is excellent (see the [PCG paper][pcgpaper] for full details).
* It's much less [predictable][predictability] and thus more secure than most generators, _although I would not advise it for cryptographic usage_.
* The reference implementation is open source software, with a permissive license (the Apache license).

## Documentation

The documentation of the library is in the [wiki][wiki]. You can also check the source code comments.

[pcghome]:http://www.pcg-random.org/
[randombug]: https://connect.microsoft.com/VisualStudio/feedback/details/634761/system-random-serious-bug
[predictability]:http://www.pcg-random.org/predictability.html
[msdn]:https://msdn.microsoft.com/en-us/library/system.random(v=vs.110).aspx
[fsrandom]:http://kos59125.github.io/FsRandom/
[pcgfeatures]:http://www.pcg-random.org/useful-features.html
[partytricks]:http://www.pcg-random.org/party-tricks.html
[statisticaltests]:http://www.pcg-random.org/statistical-tests.html
[pcgpaper]:http://www.pcg-random.org/paper.html
[wiki]:https://github.com/teo-tsirpanis/FsRandom-pcg/wiki

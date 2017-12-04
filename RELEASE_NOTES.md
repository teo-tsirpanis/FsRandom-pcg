### New in 1.4.0 (Released 2017/12/04)
* Integrated the source code of FsRandom into the project.
* FsRandom got some improvements (`ref` became `mutable` and so on).
* The library now targets only .NET Standard 2.0.
* Today used to be a wonderful day. Now it shouldn't be anything of importance.

### New in 1.3.0 (Released 2017/06/16)
* Breaking Change: Removed support for conversions between UInt128 and bigint.
* The LcgAdvance module is now public.
* Added methods to efficiently advance PCG-64 states.
* Added prettier C# names for functions (for example get becomes Get etc).

### New in 1.2.0 (Released 2017/04/18)
* Added XML documentation in the library. It appears that Visual Studio Code renders Markdown in code documentation, so some markdown code annotations are expected to be seen.
* The library uses an optimized UInt128 implementation for PCG-64. This math library was originally written by Steve Hatchett, and was ported to .NET Standard by me.
* The library now targets the .NET Framework 4.5 in addition to .NET Standard 1.6. It might target .NET 4.0 in the future.
* Added a System.Random descendant based on PCG-32. It has a handful of seeding options for easier use.
* Added a function that efficiently advances a PCG-32 state by a specified number of steps. This feature is experimental, untested, and may change in future releases. In the future, it will be available for PCG-64 too.

### New in 1.1.1 (Released 2017/04/07)
* Fixed a flaw in the seeding process.
* Fixed AssemblyInfo.fs.

### New in 1.1.0
* Breaking Change: Removed methods Pcg32.setInc and Pcg32.mapInc.
* Added PCG-64 implementaion. It gets an 128-bit seed and stream index, and returns a 64-bit integer. It's better for FsRandom, since it only supports generators that return 64-bit integers.
* Refactored the code.

### New in 1.0.1 (Released 2017/04/02)
* Fixed a mistake in the implementation of PCG-32.

### New in 1.0.0 (Released 2017/03/24)
* Initial release. 🎉
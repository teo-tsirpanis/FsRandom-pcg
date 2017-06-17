[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module FsRandom.Seq

[<CompiledName("OfRandom")>]
let ofRandom generator =
   let f = Random.next generator
   fun s0 -> seq {
      let mutable s = s0
      while true do
         let r, s' = f s
         yield r
         s <- s'
   }

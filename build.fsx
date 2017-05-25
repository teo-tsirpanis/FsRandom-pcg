// Copyright © 2017 Theodore Tsirpanis
// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.AppVeyor
open Fake.AssemblyInfoFile
open Fake.Git
open System
open System.IO

// Directories
[<Literal>]
let AppName = "FsRandom.Pcg"

[<Literal>]
let AppVersionMessage = 
    "Teo Version {0}. \nGit commit hash: {1}. \nBuilt on {2} (UTC)." 
    + "\nCopyright © 2016 Theodore Tsirpanis."

// version info
[<Literal>]
let BuildVersion = "1.0"

let version = 
    match buildServer with
    | AppVeyor -> AppVeyorEnvironment.BuildVersion
    | _ -> BuildVersion // or retrieve from CI server

[<Literal>]
let BuildDir = "./build/"

// Filesets
let projectFiles = !!"/**/*.fsproj"
let testProjects, codeProjects = projectFiles |> List.ofSeq |> List.partition (fun x -> x.Contains("test"))
let sourceFiles = !!"src/**/*.fs" ++ "src/**/*.fsx" ++ "build.fsx"
let resourceFiles = !!"src/resources/**.*"

let makeResource file =
    let content = File.ReadAllText file
    let file = file |> Path.GetFileNameWithoutExtension
    sprintf "let %s = \"\"\"\n%s\n\"\"\"" file content

let attributes = 
    let gitHash = Information.getCurrentHash()
    let buildDate = DateTime.UtcNow.ToString()
    [ Attribute.Title AppName
      
      Attribute.Copyright 
          "Copyright © 2017 Theodore Tsirpanis."
      Attribute.Metadata("Git Hash", gitHash)
      Attribute.Metadata("Build Date", buildDate)
      
      Attribute.Metadata
          ("Version Message", 
           String.Format(AppVersionMessage, version, gitHash, buildDate))
      Attribute.Version version ]

// Targets
Target "Clean" (fun _ -> DotNetCli.RunCommand id "clean")

Target "CleanBuildOutput" (fun _ -> DeleteDir BuildDir)

Target "MakeResources" (fun _ -> 
                            let content = resourceFiles |> Seq.map makeResource |> String.concat "\n" |> sprintf "module Teo.Resources\n%s"
                            File.WriteAllText("./src/Resources.fs", content))

Target "AssemblyInfo" (fun _ -> CreateFSharpAssemblyInfo "./src/AssemblyInfo.fs" attributes)

Target "Restore" (fun _ -> DotNetCli.Restore id)

Target "Build" ignore /// Just a placeholder to define the "Publish" and "Test" dependencies

Target "Publish" (fun _ -> codeProjects
                           |> List.iter 
                            (fun x -> DotNetCli.Publish (fun p -> {p with Project = x;
                                                                          Configuration = "Release";
                                                                          Output = sprintf "./../../%s" BuildDir;})))

Target "Test" (fun _ -> testProjects
                           |> List.iter 
                            (fun x -> DotNetCli.Test (fun p -> {p with Project = x;
                                                                       Configuration = "Debug"})))

// Build order
"CleanBuildOutput" ==> "Clean"
"CleanBuildOutput"
    ==> "Restore"
    ==> "AssemblyInfo"
    ==> "MakeResources"
    ==> "Build"
    ==> "Publish"
"Build" ==> "Test"
// start build
RunTargetOrDefault "Publish"

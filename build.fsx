// Copyright © 2017 Theodore Tsirpanis
// include Fake libs
#r "./packages/build/FAKE/tools/FakeLib.dll"

open Fake
open Fake.AppVeyor
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


// Targets
Target "Clean" (fun _ -> DotNetCli.RunCommand id "clean")

Target "CleanBuildOutput" (fun _ -> DeleteDir BuildDir)

Target "MakeResources" (fun _ -> 
                            let content = resourceFiles |> Seq.map makeResource |> String.concat "\n" |> sprintf "module Teo.Resources\n%s"
                            File.WriteAllText("./src/Resources.fs", content))

Target "Restore" (fun _ -> DotNetCli.Restore id)

Target "Build" (fun _ -> DotNetCli.Build (fun p -> {p with Configuration = "Release"}))

Target "Pack" (fun _ -> codeProjects |> List.iter (fun x -> DotNetCli.Pack (fun p -> {p with Project = x; AdditionalArgs = ["--no-build"]})))

Target "BuildTestDebug " (fun _ -> testProjects |> List.iter (fun x -> DotNetCli.Build (fun p -> {p with Project = x; Configuration = "Debug"})))

Target "Test" (fun _ -> testProjects |> List.iter (fun x -> DotNetCli.Test (fun p -> {p with Project = x; AdditionalArgs = ["--no-build"]})))

// Build order
"CleanBuildOutput"
    ==> "Clean"
    ==> "Restore"
    ==> "MakeResources"
    ==> "Build"
    =?> ("BuildTestDebug", hasBuildParam "Test" && not testProjects.IsEmpty)
    =?> ("Test" , not testProjects.IsEmpty)
    ==> "Pack"
// start build
RunTargetOrDefault "Pack"

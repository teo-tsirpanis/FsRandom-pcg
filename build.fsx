// Copyright (c) 2017 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

#r "./packages/build/FAKE/tools/FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open Fake.Git
open Fake.Testing
open System
open System.IO
open System.Diagnostics

[<Literal>]
let AppName = "FsRandom.Pcg"

let relNotes = ReleaseNotesHelper.LoadReleaseNotes "RELEASE_NOTES.md"

[<Literal>]
let BuildDir = "build/"

[<Literal>]
let DotNetVersion = "2.0.0-preview1-005977"

let mutable dotNetTools = "dotnet"

let isDotNetInstalled = DotNetCli.getVersion() = DotNetVersion

// Filesets
let codeProjects = !!"./src/**/*.fsproj"
let testProjects = !!"./tests/**/*.fsproj"
let expectoTests = !!"./tests/**/bin/Release/net462/*.exe"
let nUnitTests = !!"./tests/**/bin/Release/net462/FsRandom.Tests.dll"
let packages = !!"./build/**/*.nupkg"

let attributes =
    let buildDate = DateTime.UtcNow.ToString()
    [ Attribute.Title AppName
      Attribute.Description "The PCG RNG for F#"
      Attribute.Company "Theodore Tsirpanis"
      Attribute.Copyright
          "(c) 2016 Theodore Tsirpanis. Licensed under the MIT License."
      Attribute.Metadata("Build Date", buildDate)
      Attribute.InternalsVisibleTo "FsRandom.Tests"

      Attribute.Version relNotes.AssemblyVersion
      Attribute.FileVersion relNotes.AssemblyVersion
      Attribute.InformationalVersion relNotes.AssemblyVersion]

let isAppVeyorBuild = buildServer = AppVeyor

let commitMessage = environVarOrDefault "APPVEYOR_REPO_COMMIT_MESSAGE" ""

let hasNuGetKey =
    environVarOrNone "nuget_key"
    |> Option.isSome

let shouldPushToGithub =
    // AppVeyor will push a tag first, and then the build for the tag will publish to NuGet.org
    let bumpsVersion =
        commitMessage
        |> toLower
        |> startsWith "bumped version"
    tracefn "Last commit message: %s" commitMessage
    tracefn "Does the last commit message bump the version? %b" bumpsVersion
    match buildServer with
    | AppVeyor -> bumpsVersion && hasNuGetKey
    | _ -> false

let packFunc proj (x: DotNetCli.PackParams) =
    {x with
        Project = proj
        Configuration = "Release"
        OutputPath = ".." </> Directory.GetCurrentDirectory() @@ BuildDir
        ToolPath = dotNetTools
        AdditionalArgs =
            [
                "--no-build"
                sprintf "/p:Version=%s" relNotes.NugetVersion
                relNotes.Notes |> String.concat "\n" |> sprintf "/p:PackageReleaseNotes=\"%s\""
            ]}

let pushFunc url apiEnv (x: Paket.PaketPushParams) =
    {x with
        ApiKey = environVarOrFail apiEnv
        PublishUrl = url
        WorkingDir = BuildDir}

let makeAppVeyorStartInfo pkg =
    { defaultParams with
        Program = "appveyor"
        CommandLine = sprintf "PushArtifact %s" pkg
    }

// Targets
Target "InstallNetCore"
    (fun _ ->
        dotNetTools <- DotNetCli.InstallDotNetSDK DotNetVersion)

Target "Clean" (fun _ -> DotNetCli.RunCommand (fun p -> {p with ToolPath = dotNetTools}) "clean")

Target "CleanBuildOutput" (fun _ -> DeleteDir BuildDir)

Target "AssemblyInfo" (fun _ -> CreateFSharpAssemblyInfo "AssemblyVersionInfo.fs" attributes)

Target "Restore" (fun _ -> DotNetCli.Restore (fun p -> {p with ToolPath = dotNetTools}))

Target "Build" (fun _ -> DotNetCli.Build (fun p -> {p with ToolPath = dotNetTools; Configuration = "Release"}))

Target "Pack" (fun _ -> codeProjects |> Seq.iter (packFunc >> DotNetCli.Pack))

Target "Test"
    (fun _ ->
        expectoTests |> Expecto.Expecto (fun p -> {p with FailOnFocusedTests = isAppVeyorBuild})
        nUnitTests
            |> NUnit3
                (fun p ->
                    {p with
                        ToolPath = "./packages/test/NUnit.ConsoleRunner/tools/nunit3-console.exe"})
    )

Target "PushToNuGet" (fun _ -> Paket.Push (pushFunc "https://api.nuget.org/v3/index.json" "nuget_key"))

Target "GitTag"
    (fun _ ->
        let tagName = sprintf "v%s" relNotes.NugetVersion
        Branches.tag currentDirectory tagName
        Branches.pushTag currentDirectory "origin" tagName)

Target "AppVeyorPush"
    (fun _ ->
        packages
        |> Seq.map (makeAppVeyorStartInfo >> asyncShellExec)
        |> Async.Parallel
        |> Async.RunSynchronously
        |> Array.filter ((<>) 0)
        |> Array.iter (failwithf "An AppVeyor package push failed with error code %d."))

Target "Release" DoNothing

Target "PrintStatus"
    (fun _ ->
        tracefn "Current directory: %s." currentDirectory
        tracefn "Git branch: %s." <| Information.getBranchName currentDirectory
        tracefn "Author of the last commit: %s." <| environVar "APPVEYOR_REPO_COMMIT_AUTHOR"
        tracefn "Is a NuGet key defined? %b." hasNuGetKey
        tracefn "Will the packages be pushed to AppVeyor? %b." isAppVeyorBuild
        tracefn "Will the packages be pushed to GitHub/NuGet? %b." shouldPushToGithub)

// Build order
"PrintStatus"
    =?> ("InstallNetCore", not isDotNetInstalled)
    ==> "CleanBuildOutput"
    ==> "Clean"
    ==> "AssemblyInfo"
    ==> "Restore"
    ==> "Build"
    ==> "Pack"
    ==> "Test"
    =?> ("AppVeyorPush", isAppVeyorBuild)
    // =?> ("PushToNuGet", shouldPushToGithub)
    // =?> ("GitTag", shouldPushToGithub)
    ==> "Release"
"Build" ==> "Test"
// start build
RunTargetOrDefault "Release"
#addin nuget:?package=Cake.Git
#addin "MagicChunks"
#addin "Cake.FileHelpers"

var configuration = Argument("configuration", "Release");
var target = Argument("target", "Default");

var mainProject = File("./src.net/Marketo-Rest/Marketo-Rest.NetCore.csproj");
var testProject = File("./src.net/Marketo-Rest.Tests/Marketo-Rest.Tests.NetCore.csproj");
var projects = new[] { mainProject, testProject };
var artifactsDirectory = Directory("./_artifacts");
var revision = AppVeyor.IsRunningOnAppVeyor ? AppVeyor.Environment.Build.Number : 0;
var version = AppVeyor.IsRunningOnAppVeyor ? new Version(AppVeyor.Environment.Build.Version.Split('-')[0]).ToString(3) : "0.0.2";
var globalAssemblyInfo = File("./src.net/GlobalAssemblyVersion.cs");

var generatedVersion = "";
var generatedSuffix = "";


Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDirectory);
});


Task("Restore-Packages")
    .Does(() =>
{
    foreach(var project in projects)
    {
        DotNetCoreRestore(project);
    }
});


Task("Generate-Versionning")
    .Does(() =>
{
    generatedVersion = version + "." + revision;
    Information("Generated version '{0}'", generatedVersion);

    var branch = (AppVeyor.IsRunningOnAppVeyor ? AppVeyor.Environment.Repository.Branch : GitBranchCurrent(".").FriendlyName).Replace('/', '-');
    generatedSuffix = (branch == "master" && revision >= 0) ? "" : branch.Substring(0, Math.Min(10, branch.Length)) + "-" + revision;
    Information("Generated suffix '{0}'", generatedSuffix);
});


Task("Patch-GlobalAssemblyVersions")
    .IsDependentOn("Generate-Versionning")
    .Does(() =>
{
    CreateAssemblyInfo(globalAssemblyInfo, new AssemblyInfoSettings {
        FileVersion = generatedVersion,
        InformationalVersion = version + "-" + generatedSuffix,
        Version = generatedVersion
        }
    );
});


Task("Patch-ProjectJson")
    .IsDependentOn("Generate-Versionning")
    .Does(() =>
{
    TransformConfig(
        mainProject,
        mainProject,
        new TransformationCollection
        {
            { "Project/PropertyGroup/VersionPrefix", version },
            { "Project/PropertyGroup/VersionSuffix", generatedSuffix }
        }
    );
});


Task("Patch")
    .IsDependentOn("Patch-GlobalAssemblyVersions")
    .IsDependentOn("Patch-ProjectJson");


Task("Build")
    .IsDependentOn("Restore-Packages")
    .IsDependentOn("Patch")
    .Does(() =>
{
    foreach(var project in projects)
    {
        DotNetCoreBuild(
            project,
            new DotNetCoreBuildSettings
            {
                Configuration = configuration
            }
        );
    }
});


Task("Test")
    .IsDependentOn("Restore-Packages")
    .IsDependentOn("Patch")
    .Does(() =>
{
    // AppVeyor is unable to differentiate tests from multiple frameworks
    // To push all test results on AppVeyor:
    // - disable builtin AppVeyor push from XUnit
    // - generate MSTest report
    // - replace assembly name in test report
    // - manualy push test result
    foreach (var framework in new[] { "net452", "netcoreapp2.0"})
    {
        DotNetCoreTest(
            testProject,
            new DotNetCoreTestSettings
            {
                Configuration = configuration,
                Framework = framework,
                ArgumentCustomization = args => args.Append("--logger \"trx;LogFileName=result_" + framework + ".trx\""),
                EnvironmentVariables = new Dictionary<string, string>{
                    { "APPVEYOR_API_URL", null }
                }
            }
        );

        if (AppVeyor.IsRunningOnAppVeyor)
        {
            var testResult = File("./src.net/Marketo-Rest.Tests/TestResults/result_" + framework + ".trx");

            ReplaceRegexInFiles(
                testResult,
                @"Maketo\.tests\.dll",
                "Marketo-Rest.Tests." + framework + ".dll",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

           AppVeyor.UploadTestResults(testResult, AppVeyorTestResultsType.MSTest);
        }
    }
});


Task("Pack")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    //.IsDependentOn("Test")
    .Does(() =>
{
    DotNetCorePack(
        mainProject,
        new DotNetCorePackSettings
        {
            Configuration = configuration,
            OutputDirectory = artifactsDirectory,
            VersionSuffix = generatedSuffix,
            ArgumentCustomization = args => args.Append("--include-symbols")
        }
    );
});


Task("Push")
    .Does(() =>
{
    DotNetCoreNuGetPush("./_artifacts/*.nupkg"); //  -source "https://www.nuget.org"
});


Task("Default")
    .IsDependentOn("Pack");


RunTarget(target);
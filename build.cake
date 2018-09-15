#addin "Cake.Http"
#addin "Cake.Json"
#addin "Cake.ExtendedNuGet"
#addin nuget:?package=Cake.Twitter&version=0.6.0
#addin nuget:?package=Newtonsoft.Json&version=9.0.1
#addin nuget:?package=NuGet.Core&version=2.14.0
#addin nuget:?package=NuGet.Versioning&version=4.6.2
#addin "nuget:?package=Cake.Wyam"
#addin nuget:?package=Cake.Git
#addin nuget:?package=TIKSN-Cake&loaddependencies=true
#tool "nuget:?package=Mono.TextTransform"
#tool "nuget:?package=xunit.runner.console"
#tool "nuget:?package=Wyam"

var target = Argument("target", "Tweet");
var configuration = Argument("configuration", "Debug");
var solution = "PowerPython.sln";
var mainProject = "PowerPython/PowerPython.csproj";
var nextVersionString = "";

using System;
using System.Linq;
using NuGet.Versioning;
using Cake.Core.Diagnostics;

Setup(context =>
{
    SetTrashParentDirectory(GitFindRootFromPath("."));
});

Teardown(context =>
{
    // Executed AFTER the last task.
});

Task("Tweet")
  .IsDependentOn("Publish")
  .Does(() =>
{
  var oAuthConsumerKey = EnvironmentVariable("PowerPython-ConsumerKey");
  var oAuthConsumerSecret = EnvironmentVariable("PowerPython-ConsumerSecret");
  var accessToken = EnvironmentVariable("PowerPython-AccessToken");
  var accessTokenSecret = EnvironmentVariable("PowerPython-AccessTokenSecret");

  TwitterSendTweet(oAuthConsumerKey, oAuthConsumerSecret, accessToken, accessTokenSecret, $"PowerPython {nextVersionString} is published https://www.powershellgallery.com/packages/PowerPython/{nextVersionString}");
});

Task("BuildDocs")
  .IsDependentOn("Build")
  .Does(() =>
{
    Wyam(new WyamSettings {
      OutputPath = Directory("./docs/")
    });
});
    
Task("PreviewDocs")
  .IsDependentOn("BuildDocs")
  .Does(() =>
{
    Wyam(new WyamSettings
    {
        Preview = true,
        Watch = true
    });        
});

Task("Publish")
  .Description("Publish NuGet package.")
  .IsDependentOn("Pack")
  .Does(() =>
{
 var package = string.Format("tools/TIKSN-Framework.{0}.nupkg", nextVersionString);

 NuGetPush(package, new NuGetPushSettings {
     Source = "nuget.org",
     ApiKey = EnvironmentVariable("TIKSN-Framework-ApiKey")
 });
});

Task("Pack")
  .Description("Pack NuGet package.")
  .IsDependentOn("Build")
  .IsDependentOn("EstimateNextVersion")
  .IsDependentOn("BuildDocs")
  //.IsDependentOn("Test")
  .Does(() =>
{
  var nuGetPackSettings = new NuGetPackSettings {
    Version = nextVersionString,
    OutputDirectory = "tools"
    };

  // NuGetPack(nuspec, nuGetPackSettings);
});

Task("Build")
  .IsDependentOn("Clean")
  .IsDependentOn("Restore")
  .Does(() =>
{
  var configuration = Argument<string>("Configuration", "Debug");

  MSBuild(solution, configurator =>
    configurator.SetConfiguration(configuration)
      .SetVerbosity(Verbosity.Minimal)
      .UseToolVersion(MSBuildToolVersion.VS2017)
      .SetMSBuildPlatform(MSBuildPlatform.x64)
      .SetPlatformTarget(PlatformTarget.MSIL)
      //.WithTarget("Rebuild")
      );

  var dotNetCorePublishSettings = new DotNetCorePublishSettings
     {
         Configuration = configuration,
         SelfContained = false
     };
  DotNetCorePublish(mainProject, dotNetCorePublishSettings);
});

Task("EstimateNextVersion")
  .Description("Estimate next version.")
  .Does(() =>
{
  var packageList = NuGetList("TIKSN-Framework", new NuGetListSettings {
      AllVersions = false,
      Prerelease = true
      });
  var latestPackage = packageList.Single();
  var latestPackageNuGetVersion = new NuGetVersion(latestPackage.Version);

  if(!latestPackageNuGetVersion.IsPrerelease)
    throw new FormatException("Latest package version is not pre-release version.");

  if(latestPackageNuGetVersion.ReleaseLabels.Count() != 2)
    throw new FormatException("Latest package version should have exactly 2 pre-release labels.");

  var prereleaseNumber = int.Parse(latestPackageNuGetVersion.ReleaseLabels.ElementAt(1));
  var nextPrereleaseNumber = prereleaseNumber + 1;

  var nextReleaseLabels = latestPackageNuGetVersion.ReleaseLabels.ToArray();
  nextReleaseLabels[1] = nextPrereleaseNumber.ToString();
  var nextVersion = new NuGetVersion(latestPackageNuGetVersion.Version, nextReleaseLabels, null, null);
  nextVersionString = nextVersion.ToString();
  Information("Next version estimated to be " + nextVersionString);
});

Task("Restore")
  .Description("Restores packages.")
  .Does(() =>
{
  NuGetRestore(solution);
});

Task("Clean")
  .Description("Cleans all directories that are used during the build process.")
  .Does(() =>
{
  CleanDirectories("**/bin/**");
  CleanDirectories("**/obj/**");
});

RunTarget(target);
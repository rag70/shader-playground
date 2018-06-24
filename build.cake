#addin nuget:?package=SharpZipLib
#addin nuget:?package=Cake.Compression
#addin nuget:?package=Cake.Git

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Task("Prepare-Build-Directory")
  .Does(() => {
    EnsureDirectoryExists("./build");

    EnsureDirectoryExists("./src/ShaderPlayground.Core/Binaries");
    CleanDirectory("./src/ShaderPlayground.Core/Binaries");
  });

string DownloadCompiler(string url, string binariesFolderName, string version, bool cache)
{
  var tempFileName = $"./build/{binariesFolderName}-{version}.zip";

  if (!cache || !FileExists(tempFileName)) {
    DownloadFile(url, tempFileName);
  }

  return tempFileName;
}

string DownloadAndUnzipCompiler(string url, string binariesFolderName, string version, bool cache)
{
  var tempFileName = DownloadCompiler(url, binariesFolderName, version, cache);
  var unzippedFolder = $"./build/{binariesFolderName}/{version}";

  EnsureDirectoryExists(unzippedFolder);
  CleanDirectory(unzippedFolder);

  ZipUncompress(tempFileName, unzippedFolder);

  return unzippedFolder;
}

string DownloadAndUnzipCompiler(string url, string binariesFolderName, string version, bool cache, string filesToCopy)
{
  var unzippedFolder = DownloadAndUnzipCompiler(url, binariesFolderName, version, cache);

  var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/{binariesFolderName}/{version}";
  EnsureDirectoryExists(binariesFolder);
  CleanDirectory(binariesFolder);

  CopyFiles(
    $"{unzippedFolder}/{filesToCopy}",
    binariesFolder,
    true);

  return binariesFolder;
}

Task("Download-Dxc")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://ci.appveyor.com/api/projects/antiagainst/directxshadercompiler/artifacts/build%2FRelease%2Fdxc-artifacts.zip?branch=master&pr=false",
      "dxc",
      "trunk",
      false,
      "bin/*.*");
  });

Task("Download-Glslang")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://github.com/KhronosGroup/glslang/releases/download/master-tot/glslang-master-windows-x64-Release.zip",
      "glslang",
      "trunk",
      false,
      "bin/*.*");
  });

Task("Download-Mali-Offline-Compiler")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://armkeil.blob.core.windows.net/developer/Files/downloads/opengl-es-open-cl-offline-compiler/6.2/Mali_Offline_Compiler_v6.2.0.7d271f_Windows_x64.zip",
      "mali",
      "6.2.0",
      true,
      "Mali_Offline_Compiler_v6.2.0/**/*.*");
  });

Task("Download-SPIRV-Cross")
  .Does(() => {
    var unzippedFolder = DownloadAndUnzipCompiler(
      "https://github.com/KhronosGroup/SPIRV-Cross/archive/master.zip",
      "spirv-cross",
      "trunk",
      false);

    MSBuild(unzippedFolder + "/SPIRV-Cross-master/msvc/SPIRV-Cross.vcxproj", new MSBuildSettings()
      .SetConfiguration(configuration)
      .WithProperty("WindowsTargetPlatformVersion", "10.0.17134.0")
      .WithProperty("PlatformToolset", "v141"));

    var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/spirv-cross/trunk";
    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"{unzippedFolder}/SPIRV-Cross-master/msvc/{configuration}/SPIRV-Cross.exe",
      binariesFolder,
      true);
  });

Task("Download-SPIRV-Cross-ISPC")
  .Does(() => {
    var unzippedFolder = DownloadAndUnzipCompiler(
      "https://github.com/GameTechDev/SPIRV-Cross/archive/master-ispc.zip",
      "spirv-cross-ispc",
      "trunk",
      false);

    MSBuild(unzippedFolder + "/SPIRV-Cross-master-ispc/msvc/SPIRV-Cross.vcxproj", new MSBuildSettings()
      .SetConfiguration(configuration)
      .WithProperty("WindowsTargetPlatformVersion", "10.0.17134.0"));

    var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/spirv-cross-ispc/trunk";
    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"{unzippedFolder}/SPIRV-Cross-master-ispc/msvc/{configuration}/SPIRV-Cross.exe",
      binariesFolder,
      true);
  });

Task("Download-SPIRV-Tools")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://github.com/KhronosGroup/SPIRV-Tools/releases/download/master-tot/SPIRV-Tools-master-windows-x64-Release.zip",
      "spirv-tools",
      "trunk",
      false,
      "bin/*.*");
  });

Task("Download-XShaderCompiler")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://github.com/LukasBanana/XShaderCompiler/releases/download/v0.10-alpha/Xsc-v0.10-alpha.zip",
      "xshadercompiler",
      "v0.10-alpha",
      true,
      "Xsc-v0.10-alpha/bin/Win32/xsc.exe");
  });

Task("Download-Slang")
  .Does(() => {
    void DownloadSlang(string version)
    {
      DownloadAndUnzipCompiler(
        $"https://github.com/shader-slang/slang/releases/download/v{version}/slang-{version}-win64.zip",
        "slang",
        $"v{version}",
        true,
        "bin/windows-x64/release/*.*");
    }
    
    DownloadSlang("0.10.24");
    DownloadSlang("0.10.25");
    DownloadSlang("0.10.26");
  });

Task("Download-HLSLParser")
  .Does(() => {
    var unzippedFolder = DownloadAndUnzipCompiler(
      "https://github.com/Thekla/hlslparser/archive/master.zip",
      "hlslparser",
      "trunk",
      false);

    MSBuild(unzippedFolder + "/hlslparser-master/hlslparser.vcxproj", new MSBuildSettings()
      .SetConfiguration(configuration)
      .WithProperty("WindowsTargetPlatformVersion", "10.0.17134.0")
      .WithProperty("PlatformToolset", "v141"));

    var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/hlslparser/trunk";
    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"{unzippedFolder}/hlslparser-master/{configuration}/hlslparser.exe",
      binariesFolder,
      true);
  });

Task("Build-Fxc-Shim")
  .Does(() => {
    DotNetCorePublish("./shims/ShaderPlayground.Shims.Fxc/ShaderPlayground.Shims.Fxc.csproj", new DotNetCorePublishSettings
    {
      Configuration = configuration
    });

    var fxcVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo("./lib/x64/d3dcompiler_47.dll").ProductVersion;
    var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/fxc/{fxcVersion}";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.Fxc/bin/{configuration}/netcoreapp2.0/publish/**/*.*",
      binariesFolder,
      true);
  });

Task("Build-HLSLcc-Shim")
  .Does(() => {
    MSBuild("./shims/ShaderPlayground.Shims.HLSLcc/ShaderPlayground.Shims.HLSLcc.vcxproj", new MSBuildSettings
    {
      Configuration = configuration
    });

    var binariesFolder = "./src/ShaderPlayground.Core/Binaries/hlslcc/trunk";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.HLSLcc/{configuration}/ShaderPlayground.Shims.HLSLcc.exe",
      binariesFolder,
      true);
  });

Task("Build-GLSL-Optimizer-Shim")
  .Does(() => {
    MSBuild("./shims/ShaderPlayground.Shims.GlslOptimizer/Source/projects/vs2010/glsl_optimizer_lib.vcxproj", new MSBuildSettings()
      .WithProperty("PlatformToolset", "v141")
      .WithProperty("WindowsTargetPlatformVersion", "10.0.17134.0")
      .SetConfiguration(configuration)
      .SetPlatformTarget(PlatformTarget.Win32));

    MSBuild("./shims/ShaderPlayground.Shims.GlslOptimizer/ShaderPlayground.Shims.GlslOptimizer.vcxproj", new MSBuildSettings()
      .SetConfiguration(configuration));

    var binariesFolder = "./src/ShaderPlayground.Core/Binaries/glsl-optimizer/trunk";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.GlslOptimizer/{configuration}/ShaderPlayground.Shims.GlslOptimizer.exe",
      binariesFolder,
      true);
  });

Task("Build-HLSL2GLSL-Shim")
  .Does(() => {
    MSBuild("./shims/ShaderPlayground.Shims.Hlsl2Glsl/Source/hlslang.vcxproj", new MSBuildSettings()
      .WithProperty("PlatformToolset", "v141")
      .WithProperty("WindowsTargetPlatformVersion", "10.0.17134.0")
      .WithProperty("ForcedIncludeFiles", "<algorithm>")
      .SetConfiguration(configuration)
      .SetPlatformTarget(PlatformTarget.Win32));

    MSBuild("./shims/ShaderPlayground.Shims.Hlsl2Glsl/ShaderPlayground.Shims.Hlsl2Glsl.vcxproj", new MSBuildSettings()
      .SetConfiguration(configuration));

    var binariesFolder = "./src/ShaderPlayground.Core/Binaries/hlsl2glsl/trunk";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.Hlsl2Glsl/{configuration}/ShaderPlayground.Shims.Hlsl2Glsl.exe",
      binariesFolder,
      true);
  });

Task("Build-SMOL-V-Shim")
  .Does(() => {
    MSBuild("./shims/ShaderPlayground.Shims.Smolv/ShaderPlayground.Shims.Smolv.vcxproj", new MSBuildSettings
    {
      Configuration = configuration
    });

    var binariesFolder = "./src/ShaderPlayground.Core/Binaries/smol-v/trunk";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.Smolv/{configuration}/ShaderPlayground.Shims.Smolv.exe",
      binariesFolder,
      true);
  });

Task("Build-Shims")
  .IsDependentOn("Build-Fxc-Shim")
  .IsDependentOn("Build-HLSLcc-Shim")
  .IsDependentOn("Build-GLSL-Optimizer-Shim")
  .IsDependentOn("Build-HLSL2GLSL-Shim")
  .IsDependentOn("Build-SMOL-V-Shim");

Task("Build")
  .IsDependentOn("Build-Shims")
  .Does(() => {
    var outputFolder = "./build/site";
    EnsureDirectoryExists(outputFolder);
    CleanDirectory(outputFolder);

    DotNetCorePublish("./src/ShaderPlayground.Web/ShaderPlayground.Web.csproj", new DotNetCorePublishSettings
    {
      Configuration = configuration,
      OutputDirectory = outputFolder
    });

    ZipCompress(outputFolder, "./build/site.zip");
  });

Task("Test")
  .IsDependentOn("Build")
  .Does(() => {
    DotNetCoreTest("./src/ShaderPlayground.Core.Tests/ShaderPlayground.Core.Tests.csproj", new DotNetCoreTestSettings
    {
      Configuration = configuration
    });
  });

Task("Default")
  .IsDependentOn("Prepare-Build-Directory")
  .IsDependentOn("Download-Dxc")
  .IsDependentOn("Download-Glslang")
  .IsDependentOn("Download-Mali-Offline-Compiler")
  .IsDependentOn("Download-SPIRV-Cross")
  .IsDependentOn("Download-SPIRV-Tools")
  .IsDependentOn("Download-XShaderCompiler")
  .IsDependentOn("Download-SPIRV-Cross-ISPC")
  .IsDependentOn("Download-Slang")
  .IsDependentOn("Download-HLSLParser")
  .IsDependentOn("Build")
  .IsDependentOn("Test");

RunTarget(target);
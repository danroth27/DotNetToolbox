# .NET Core Toolbox

The .NET Core Toolbox gives you a simple way to install and run .NET Core tools.

## Installing

You first need to install the latest [.NET Core SDK](https://github.com/dotnet/cli#installers-and-binaries).

Now run one of the installation scripts below:

### Powershell
```powershell
&{$wc=New-Object System.Net.WebClient;$wc.Proxy=[System.Net.WebRequest]::DefaultWebProxy;$wc.Proxy.Credentials=[System.Net.CredentialCache]::DefaultNetworkCredentials;Invoke-Expression($wc.DownloadString('https://raw.githubusercontent.com/danroth27/dotnettoolbox/master/install.ps1'))}
```

### CMD
```cmd
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{$wc=New-Object System.Net.WebClient;$wc.Proxy=[System.Net.WebRequest]::DefaultWebProxy;$wc.Proxy.Credentials=[System.Net.CredentialCache]::DefaultNetworkCredentials;Invoke-Expression($wc.DownloadString('https://raw.githubusercontent.com/danroth27/dotnettoolbox/master/install.ps1'))}"
```

### Bash
```bash
\curl -sSL https://raw.githubusercontent.com/danroth27/dotnettoolbox/master/install.sh | bash
```

### Update to latest build

After running the install script you can run the following to update to the latest build on [MyGet](https://www.myget.org/gallery/danroth27):
```cmd
dotnet toolbox install dotnet-toolbox -v 1.0.0-* -s https://www.myget.org/F/danroth27/api/v3/index.json
```

## Usage

```
dotnet toolbox install <tool_package> [-v <version>]
dotnet <installed_tool>
```

## Creating toolbox tools

Toolbox tools are just ordinary .NET Core console apps that have been packaged into a NuGet package using `dotnet pack`. 

The tool package currently must contain a deps file and a runtime config file to work with Toolbox. You can include these files by adding the following targets to your project:

```xml
<!-- workaround https://github.com/NuGet/Home/issues/4321

When fixed, replace with this instead
<None Include="$(ProjectRuntimeConfigFilePath)" Pack="true" PackagePath="lib\$(TargetFramework)\" />
-->
<PropertyGroup>
  <DefaultItemExcludes>$(DefaultItemExcludes);lib\**\*</DefaultItemExcludes>
</PropertyGroup>
<Target Name="PackRuntimeConfigurationFile" DependsOnTargets="GenerateBuildRuntimeConfigurationFiles" BeforeTargets="_GetPackageFiles">
  <Copy SourceFiles="$(ProjectRuntimeConfigFilePath)" DestinationFolder="$(MSBuildProjectDirectory)\lib\netcoreapp1.0\" />
  <Copy SourceFiles="$(ProjectDepsFilePath)" DestinationFolder="$(MSBuildProjectDirectory)\lib\netcoreapp1.0\" />
  <ItemGroup>
    <_PackageFiles Include="lib\netcoreapp1.0\*.json" BuildAction="None" PackagePath="%(Identity)" />
  </ItemGroup>
</Target>
<Target Name="CleanupTempRuntimeConfigurationFile" AfterTargets="Pack">
  <RemoveDir Directories="$(MSBuildProjectDirectory)\lib\" />
</Target>
```

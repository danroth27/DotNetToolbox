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

### Bash, sh, Fish, etc...
```bash
\curl -sSL https://raw.githubusercontent.com/danroth27/dotnettoolbox/master/install.sh | bash
```

## Usage

```
dotnet toolbox install <tool_package> [-v <version>]
dotnet <installed_tool>
```

# .NET Core Toolbox

## Installing

### Powershell
````powershell
&{$wc=New-Object System.Net.WebClient;$wc.Proxy=[System.Net.WebRequest]::DefaultWebProxy;$wc.Proxy.Credentials=[System.Net.CredentialCache]::DefaultNetworkCredentials;Invoke-Expression($wc.DownloadString('https://raw.githubusercontent.com/danroth27/dotnettoolbox/master/install.ps1'))}
````

### CMD
````cmd
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{$wc=New-Object System.Net.WebClient;$wc.Proxy=[System.Net.WebRequest]::DefaultWebProxy;$wc.Proxy.Credentials=[System.Net.CredentialCache]::DefaultNetworkCredentials;Invoke-Expression($wc.DownloadString('https://raw.githubusercontent.com/danroth27/dotnettoolbox/master/install.ps1'))}"
````

### Bash, sh, Fish, etc...
````bash
\curl -sSL https://raw.githubusercontent.com/danroth27/dotnettoolbox/master/install.sh | bash
````

# Zyborg.Vault
PowerShell client bindings for HashiCorp Vault

[![Build status](https://ci.appveyor.com/api/projects/status/ldby4js60k32mqtl?svg=true)](https://ci.appveyor.com/project/ebekker/zyborg-vault)
[![Powershellgallery Badge][psgallery-badge]][psgallery-status]
[![MyGet](https://img.shields.io/myget/zyborg-stage/v/Zyborg.Vault.svg)]()


## About

The aim of this project is to provide a PowerShell module that provides cmdlets
to interact with a HashiCorp Vault server in a natural way for PowerShell -- *the PowerShell way*<sup>TM</sup>.

The initial set of cmdlets is derived from the operations that are available
from the official [Vault CLI client](https://www.vaultproject.io/docs/commands/index.html).
We define a mapping of CLI functions to PowerShell cmdlets
[here](https://docs.google.com/spreadsheets/d/19Jt7iKim0CTmUPTF5sqga_D-yqYgCc3bmQSLmYmE6aQ/edit?usp=sharing).

[psgallery-badge]: https://img.shields.io/badge/PowerShell_Gallery-LATEST-green.svg
[psgallery-status]: https://www.powershellgallery.com/packages/Zyborg.Vault

## Status

The initial release of this project includes support for most commands found int the Vault CLI.

The cmdlets have been tested manually and verified to work properly against a Vault v7.x server.  Automated testing will be added in a forthcoming release.

Any feedback is welcome!

## Installation

The easiest way to get started is to install the latest published release from the [PowerShell Gallery](https://www.powershellgallery.com/packages/Zyborg.Vault).

```PowerShell
Install-Module -Name Zyborg.Vault
```

You can also get early access to pre-release builds.

```PowerShell
Import-Module PowerShellGet
Register-PSRepository -Name "zyborg-stage" -SourceLocation "https://www.myget.org/F/zyborg-stage/api/v3/index.json"
Install-Module -Name "Zyborg.Vault" -Repository "zyborg-stage"
```

## Vault Connection Profiles

One feature that this module includes that improves upon the official CLI is support for "Vault server connection profiles" or just *profiles*.  This is a feature that allows a user to define connection attributes under a named profile, including the Vault server endpoint URL and an authentication token, which is then stored securely for a given user.

You can then reference the profiles instead of having to specify these settings on each cmdlet invocation or having to resort to the use of environment variables which is a less secure option.  You can also define a default profile named **`default`** which will be used as the fallback if no other profile is specified explicitly.

Even when using a connection profile, you can override individual connection settings as parameters on each invocation.

You can use the following cmdlets to manage profiles:
* `Get-HCVaultProfile`
* `Set-HCVaultProfile`
* `New-HCVaultAuth` (when specifying the `-SaveAs` parameter)

## Examples

Here we provide a few usage examples to get a feel for the cmdlets.

### Example #1

Define a connection profile and use it in inquire about the setup of the Vault server.

```PowerShell
PS C:\> Set-HCVaultProfile my-vault -VaultAddress https://my-vault.contoso.local:8200 -VaultToken xxxx-yyyy-zzzz

PS C:\> Test-HCVaultInstance -VaultProfile my-vault  ## This is used to verify Vault server is initialized
True
PS C:\> Get-HCVaultStatus -VaultProfile my-vault


Sealed          : False
SecretThreshold : 3
SecretShares    : 5
Progress        : 0
Version         : 0.7.2
ClusterName     : ezs-vault-cluster-1
ClusterId       : a7f91311-dc25-91fb-1ed8-6bb530299a08
Nonce           :


PS C:\> Get-HCVaultKeyStatus -VaultProfile my-vault

SequentialKeyNumber InstallTime
------------------- -----------
                  1 5/3/2017 12:56:10 PM +00:00


PS C:\> Get-HCVaultAuthMounts -VaultProfile my-vault

AuthenticationPath BackendType Description
------------------ ----------- -----------
approle/           approle
okta/              okta
token/             token       token based credentials
user1/             userpass


PS C:\> Get-HCVaultSecretMounts -VaultProfile my-vault

MountPoint BackendType Description                                             MountConfiguration
---------- ----------- -----------                                             ------------------
cubbyhole/ cubbyhole   per-token private secret storage                        VaultSharp.Backends.System.Models.Mou...
secret/    generic     generic secret storage                                  VaultSharp.Backends.System.Models.Mou...
secret2b/  generic                                                             VaultSharp.Backends.System.Models.Mou...
sys/       system      system endpoints used for control, policy and debugging VaultSharp.Backends.System.Models.Mou...
```

### Example #2

Here we show a few typical use cases for working with secrets.

```PowerShell
## This creates a new profile named 'default' that uses the same
## attributes as the existing profile 'my-profile' -- if not
## overridden, the 'default' profile will be used by default
PS C:\> Set-HCVaultProfile default -VaultProfile my-profile

PS C:\> Get-HCVaultDataList secret

Keys
----
{foo1, foo2, s1, s2...}


PS C:\> Get-HCVaultDataList secret | select -ExpandProperty keys | select -First 3
foo1
foo2
s1


PS C:\> Write-HCVaultData secret/foo3 -Data @{ k1="v1"; k2="v2" }
PS C:\> Read-HCVaultData secret/foo3

Key Value
--- -----
k1  v1
k2  v2
```

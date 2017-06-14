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

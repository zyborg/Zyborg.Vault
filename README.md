# Zyborg.Vault
PowerShell client bindings for HashiCorp Vault

[![Browse API Docs][apidocs-badge]](docs/api)

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

[apidocs-badge]: https://img.shields.io/badge/API_Docs-BROWSE-blue.svg
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

## Home Page

For more information, including examples and API Documentation, please visit the [home page]().

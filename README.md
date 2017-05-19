# Zyborg.Vault
PowerShell bindings for HashiCorp Vault

The aim of this project is to provide a PowerShell module that provides cmdlets
to interact with a HashiCorp Vault server in a *natural* way for PowerShell.

The initial set of cmdlets is derived from the operations that are available
from the official [Vault CLI client](https://www.vaultproject.io/docs/commands/index.html).
We define a mapping of CLI functions to PowerShell cmdlets
[here](https://docs.google.com/spreadsheets/d/19Jt7iKim0CTmUPTF5sqga_D-yqYgCc3bmQSLmYmE6aQ/edit?usp=sharing).

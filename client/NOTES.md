

### Static IL Linking:
* https://github.com/dotnet/core/blob/master/samples/linker-instructions.md
* https://github.com/dotnet/core/blob/master/samples/linker-instructions-advanced.md

## Building Micro Services with ASP.NET Core but without MVC:
* http://www.strathweb.com/2017/01/building-microservices-with-asp-net-core-without-mvc/

## Required Query String params in route template
* https://www.strathweb.com/2016/09/required-query-string-parameters-in-asp-net-core-mvc/

## Web Developer Checklist:
* http://webdevchecklist.com

## DOEST NOT WORK:  For testing out the plugin feature, you can use the Oracle DB plugin:
* pre-built for Linux only:  https://releases.hashicorp.com/vault-plugin-database-oracle/0.1.0/
* For Windows:
    set GOPATH=your go work path
    cd %GOPATH%
    git clone https://github.com/hashicorp/vault-plugin-database-oracle.git %GOPATH%\src\github.com\hashicorp\vault-plugin-database-oracle
    cd %GOPATH%\src\github.com\hashicorp\vault-plugin-database-oracle
    go build -o my-plugin-database-oracle .\plugin

## For testing out the plugin feature, you can use the Mock plugin:
* For Windows:
    set GOPATH=your go work path
    cd %GOPATH%\src\github.com\hashicorp
    git clone https://github.com/hashicorp/vault.git
    cd logical\plugin\mock\mock-plugin
    go build -o mock-plugin.exe .\main.go
    copy .\mock-plugin.exe YOUR-VAULT-PLUGIN-DIR
    PS:  Get-FileHash -Algorithm SHA256 -Path .\mock-plugin.exe



# Vault Concerns and Criticisms
* https://news.ycombinator.com/item?id=15284976
* https://docs.google.com/presentation/d/1ipP2eB9pW5j3WDvzCGz9Wy4MBoK2SMvOTZtPdf5kSNs/edit#slide=id.g1fa55b980e_1_164
* https://github.com/ejcx/dssss

# Related:
* https://www.usenix.org/conference/enigma2017/conference-program/presentation/haken
* https://kev.inburke.com/kevin/things-to-use-instead-of-jwt/
* https://paragonie.com/blog/2017/03/jwt-json-web-tokens-is-bad-standard-that-everyone-should-avoid


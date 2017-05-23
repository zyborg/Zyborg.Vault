
@SETLOCAL
@SET THIS_DIR=%~dp0

@SET VAULT=\downloads\HashiCorp\vault.exe
@SET VAULT_CFG="%THIS_DIR%vault-test.hcl"

@SET VAULT_ARGS=server
@SET VAULT_ARGS=%VAULT_ARGS% -config=%VAULT_CFG%

"%VAULT%" %VAULT_ARGS%

@ENDLOCAL

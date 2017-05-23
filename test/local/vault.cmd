@SETLOCAL
@SET THIS_DIR= %~dp0
@SET VAULT=\downloads\HashiCorp\vault.exe
@%VAULT% %*
@ENDLOCAL

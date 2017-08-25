@SETLOCAL
@SET THIS_DIR= %~dp0

@REM Latest official release
@SET VAULT=\downloads\HashiCorp\vault.exe

@%VAULT% %*
@ENDLOCAL

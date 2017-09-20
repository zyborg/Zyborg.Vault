@SETLOCAL
@SET THIS_DIR= %~dp0

@SET VAULT_TOKEN=%1

@SET VAULT_ARGS=
:get_args
@IF "%~2"=="" GOTO :got_args
@SET VAULT_ARGS=%VAULT_ARGS% "%~2"
@SHIFT /2
@GOTO :get_args
:got_args

@REM Latest official release
@SET VAULT=\downloads\HashiCorp\vault.exe

@%VAULT% %VAULT_ARGS%
@ENDLOCAL

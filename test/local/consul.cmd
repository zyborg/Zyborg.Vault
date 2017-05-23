@SETLOCAL
@SET THIS_DIR= %~dp0
@SET CONSUL=\downloads\HashiCorp\consul.exe
@%CONSUL% %*
@ENDLOCAL

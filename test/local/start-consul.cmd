@REM PORTS USED
@REM ==========
@REM Consul requires up to 5 different ports to work properly, some on TCP, UDP, or both protocols.
@REM Below we document the requirements for each port.
@REM 
@REM   * Server RPC (Default 8300). This is used by servers to handle incoming requests from other
@REM     agents. TCP only.
@REM   * Serf LAN (Default 8301). This is used to handle gossip in the LAN. Required by all agents.
@REM     TCP and UDP.
@REM   * Serf WAN (Default 8302). This is used by servers to gossip over the WAN to other servers.
@REM     TCP and UDP.
@REM   * CLI RPC (Default 8400). This is used by all agents to handle RPC from the CLI, but is
@REM     deprecated in Consul 0.8 and later.
@REM     TCP only. In Consul 0.8 all CLI commands were changed to use the HTTP API and the RPC
@REM     interface was completely removed.
@REM   * HTTP API (Default 8500). This is used by clients to talk to the HTTP API. TCP only.
@REM   * DNS Interface (Default 8600). Used to resolve DNS queries. TCP and UDP.


@SETLOCAL
@SET THIS_DIR=%~dp0

@SET CONSUL=\downloads\HashiCorp\consul.exe
@SET CONSUL_DAT="%THIS_DIR%_IGNORE/dat"
@SET CONSUL_NET=127.0.0.1

@SET CONSUL_ARGS=agent
@SET CONSUL_ARGS=%CONSUL_ARGS% -server
@SET CONSUL_ARGS=%CONSUL_ARGS% -bind=%CONSUL_NET%
@SET CONSUL_ARGS=%CONSUL_ARGS% -data-dir=%CONSUL_DAT%
@SET CONSUL_ARGS=%CONSUL_ARGS% -bootstrap-expect=1
@SET CONSUL_ARGS=%CONSUL_ARGS% -ui

@REM @SET CONSUL_ARGS=%CONSUL_ARGS% -http-port=8500

IF NOT EXIST %CONSUL_DAT% MKDIR %CONSUL_DAT%

"%CONSUL%" %CONSUL_ARGS%

@ENDLOCAL

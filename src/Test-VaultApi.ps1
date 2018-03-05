$VaultTokenHeaderKey = "X-Vault-Token";
$VaultToken = 'c4184467-3c99-8a04-fe80-2ed288de4548'
$VaultAddr = 'https://vault.aws1.ezsops.net:8200'
$VaultBase = "$VaultAddr/v1"

$VaultHeaders = @{ $VaultTokenHeaderKey = $VaultToken }

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor [System.Net.SecurityProtocolType]::Tls12

   #Invoke-RestMethod -Headers $VaultHeaders $VaultBase/sys/health
   #Invoke-RestMethod -Headers $VaultHeaders $VaultBase/sys/seal-status
   #Invoke-RestMethod -Headers $VaultHeaders $VaultBase/sys/seal-status?help=1
    
    Invoke-RestMethod -Headers $VaultHeaders $VaultBase/sys/auth #okta?help=1
    Invoke-RestMethod -Headers $VaultHeaders $VaultBase/sys

    Invoke-RestMethod -Headers $VaultHeaders $VaultBase/sys/auth/okta/tune #okta?help=1
    Invoke-RestMethod -Headers $VaultHeaders $VaultBase/auth/okta/config #okta?help=1
    Invoke-RestMethod -Headers $VaultHeaders $VaultBase/auth/okta?help=1
    Invoke-RestMethod -Headers $VaultHeaders $VaultBase/auth/okta?help=1

    
    Invoke-RestMethod -Headers $VaultHeaders $VaultBase/auth/user1/login/jdoe1 -Method Post -Body (@{
        username = 'jdoe1'
        password = 'foo'
    }|ConvertTo-Json)


    New-VaultAuth -MethodType userpass -Mount user1 -AddPath jdoe1 -AuthInfo @{ password = "" }

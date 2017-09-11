
cluster_name = "vault-file"
#cluster_name = "foo-bar"

##
## Doc Ref:
##    https://www.vaultproject.io/docs/configuration/index.html#listener
##
listener "tcp" {
    address         = "0.0.0.0:8200"
    tls_disable     = 1
}

##
## Doc Ref:
##   https://www.vaultproject.io/docs/configuration/#storage
##
storage "file" {
    ## This path will get wiped out upon each restart
    path = "./_IGNORE/vault-initonly-data"
}


plugin_directory = "./_IGNORE/vault-file-plugins"

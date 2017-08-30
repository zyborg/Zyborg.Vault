
cluster_name = "vault-no-init"

##
## Doc Ref:
##    https://www.vaultproject.io/docs/configuration/index.html#listener
##
listener "tcp" {
    address         = "0.0.0.0:8299"
    tls_disable     = 1
}

##
## Doc Ref:
##   https://www.vaultproject.io/docs/configuration/#storage
##
storage "file" {
    path = "./_IGNORE/no-init"
}

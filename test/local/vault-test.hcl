
cluster_name = "vault-test-1"

##
## Doc Ref:
##    https://www.vaultproject.io/docs/configuration/index.html#listener
##

listener "tcp" {
    #address = "127.0.0.1:8200"
    address = "0.0.0.0:8200"
    tls_disable = 1
}


##
## Doc Ref:
##   https://www.vaultproject.io/docs/configuration/#storage
##


#storage "file" {
#    path = "./vault-data"
#}

storage "consul" {
    address = "127.0.0.1:8500"

    ## Path in KV store
    path = "vault-test"

    ## Service name to register
    service = "vault-test"
}

##
## Doc Ref:
##   https://www.vaultproject.io/docs/configuration/#ha_storage
##
## Because the S3 storage provider does not support HA
## semantics natively, we have to use a secondary storage
## provider as a surrogate to implement HA cooridination
##
/*
ha_storage "consul" {
}
*/

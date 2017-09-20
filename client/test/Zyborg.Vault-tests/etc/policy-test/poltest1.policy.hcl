
path "poltest/dir1" {
    capabilities = [ "list", "read" ]
}

path "poltest/dir2" {
    capabilities = [ "list", "read", "create", "update", "delete" ]
}

path "poltest/dir2/foo" {
    capabilities = [ "deny" ]
}

path "poltest/dir2/foo/bar" {
    capabilities = [ "list", "read", "create", "update", "delete" ]
}


path "poltest/dir1*" {
    capabilities = [ "list", "read" ]
}

path "poltest/dir2*" {
    capabilities = [ "list", "read", "create", "update", "delete" ]
}

path "poltest/dir2/foo*" {
    capabilities = [ "deny" ]
}

path "poltest/dir2/foo/bar*" {
    capabilities = [ "list", "read", "create", "update", "delete" ]
}

path "poltest/dir2/foo/baz*" {
    capabilities = [ "list", "read" ]
}

# path "poltest/dir2/foo/baz*" {
#     capabilities = [ "deny" ]
# }

path "poltest/dir2/foo/baz*" {
    capabilities = [ "delete" ]
}

path "poltest/dir2/foo/baz*" {
    capabilities = [ "create", ]
    allowed_parameters = {
        "foo" = ["bar1", "bar3"]
    }
}

path "poltest/dir2/foo/baz*" {
    capabilities = [ "update" ]
    allowed_parameters = {
        "foo" = ["bar2", "bar4"]
        "bar" = ["FOO"]
    }
}


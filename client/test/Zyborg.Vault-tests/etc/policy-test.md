

vault mount -path=poltest generic
vault unmount poltest


## poltest1

vault policy-write poltest1 policy-test/poltest1.policy.hcl
vault token-create -policy=poltest1

vault write poltest/dir0/foo a=dir0_1 b=dir0_2
vault write poltest/dir1/foo a=dir1_1 b=dir1_2
vault write poltest/dir2/fu a=dir2_1 b=dir2_2
vault write poltest/dir2/foo a=dir2_1 b=dir2_2
vault write poltest/dir2/foo2 a=dir2_1 b=dir2_2
vault write poltest/dir2/fee a=dir2_1 b=dir2_2
vault write poltest/dir2/foo/bar a=dir2_1bar b=dir2_2bar

vault-with-token 8d7a0439-34ee-0951-b49e-da53e260f39a list poltest
vault-with-token 8d7a0439-34ee-0951-b49e-da53e260f39a list poltest/dir0
vault-with-token 8d7a0439-34ee-0951-b49e-da53e260f39a list poltest/dir1
vault-with-token 8d7a0439-34ee-0951-b49e-da53e260f39a list poltest/dir2


## poltest2

vault policy-write poltest2 policy-test/poltest2.policy.hcl
vault policies poltest2
vault token-create -policy=poltest2

vault-with-token f160dd51-2835-e496-db5e-8997afc9dcce list poltest
vault-with-token f160dd51-2835-e496-db5e-8997afc9dcce list poltest/dir0
vault-with-token f160dd51-2835-e496-db5e-8997afc9dcce list poltest/dir1
vault-with-token f160dd51-2835-e496-db5e-8997afc9dcce list poltest/dir2

vault-with-token f160dd51-2835-e496-db5e-8997afc9dcce read poltest/dir0/foo
vault-with-token f160dd51-2835-e496-db5e-8997afc9dcce read poltest/dir1/foo
vault-with-token f160dd51-2835-e496-db5e-8997afc9dcce read poltest/dir2/fu
vault-with-token f160dd51-2835-e496-db5e-8997afc9dcce read poltest/dir2/foo
vault-with-token f160dd51-2835-e496-db5e-8997afc9dcce read poltest/dir2/foo2
vault-with-token f160dd51-2835-e496-db5e-8997afc9dcce read poltest/dir2/fee
vault-with-token f160dd51-2835-e496-db5e-8997afc9dcce read poltest/dir2/foo/bar

vault-with-token f160dd51-2835-e496-db5e-8997afc9dcce list poltest/dir2/foo/baz
vault-with-token f160dd51-2835-e496-db5e-8997afc9dcce write poltest/dir2/foo/baz/s1 "a=1" "b=2"
vault-with-token f160dd51-2835-e496-db5e-8997afc9dcce read poltest/dir2/foo/baz/s1

vault-with-token f160dd51-2835-e496-db5e-8997afc9dcce write poltest/dir2/foo/baz/s1 "a=1" "b=2" "foo=bar2"


## poltest3

vault policy-write poltest3 policy-test/poltest3.policy.json


## multi-policy tests

vault policy-write multi1 policy-test/multi1.policy.hcl
vault policy-write multi2 policy-test/multi2.policy.hcl
vault policy-write multi3 policy-test/multi3.policy.hcl

vault token-create -id=multi1-only -policy=multi1
vault token-create -id=multi2-only -policy=multi2
vault token-create -id=multi3-only -policy=multi3

vault token-create -id=multi1_2-exclusive -no-default-policy -policy=multi1 -policy=multi2
vault token-create -id=multi1_2_3-exclusive -no-default-policy -policy=multi1 -policy=multi2 -policy=multi3


vault-with-token.cmd multi1-only write poltest/multi/dir1/foo1 "a=1"
vault-with-token.cmd multi2-only write poltest/multi/dir1/foo1 "a=1"
vault-with-token.cmd multi1_2-exclusive write poltest/multi/dir1/foo2 "a=1"
vault-with-token.cmd multi1_2_3-exclusive write poltest/multi/dir1/foo2 "a=1"

vault-with-token.cmd multi1-only list poltest/multi/dir1
vault-with-token.cmd multi2-only list poltest/multi/dir1
vault-with-token.cmd multi1-only read poltest/multi/dir1/foo2
vault-with-token.cmd multi1_2-exclusive list poltest/multi/dir1
vault-with-token.cmd multi1_2-exclusive read poltest/multi/dir1/foo1

## All With/Without Sudo

vault policy-write all-without-sudo policy-test/all-without-sudo.policy.hcl
vault token-create -id=all-without-sudo -no-default-policy -policy=all-without-sudo

vault policy-write all-without-sudo policy-test/all-with-sudo.policy.hcl
vault token-create -id=all-with-sudo -no-default-policy -policy=all-with-sudo

vault token-create -id=root-test -no-default-policy -policy=root

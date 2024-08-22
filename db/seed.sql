COPY mail.contacts (email, subscribed, name, created_at, updated_at) FROM stdin;
test@test.com	t	Big Time	2024-01-11 11:47:10.402455-08	2024-01-11 11:47:10.402455-08
test0@test.com	t	Big Time	2024-01-11 11:47:10.402455-08	2024-01-11 11:47:10.402455-08
test1@test.com	t	Test User 1	2024-01-11 11:47:10.394175-08	2024-01-11 11:47:10.394175-08
test2@test.com	t	Test User 2	2024-01-11 11:47:10.394175-08	2024-01-11 11:47:10.394175-08
test3@test.com	t	Test User 3	2024-01-11 11:47:10.394175-08	2024-01-11 11:47:10.394175-08
test4@test.com	t	Test User 4	2024-01-11 11:47:10.394175-08	2024-01-11 11:47:10.394175-08
test5@test.com	t	Test User 5	2024-01-11 11:47:10.394175-08	2024-01-11 11:47:10.394175-08
test6@test.com	t	Test User 6	2024-01-11 11:47:10.394175-08	2024-01-11 11:47:10.394175-08
test7@test.com	t	Test User 7	2024-01-11 11:47:10.394175-08	2024-01-11 11:47:10.394175-08
test8@test.com	t	Test User 8	2024-01-11 11:47:10.394175-08	2024-01-11 11:47:10.394175-08
test9@test.com	t	Test User 9	2024-01-11 11:47:10.394175-08	2024-01-11 11:47:10.394175-08
test10@test.com	t	Test User 10	2024-01-11 11:47:10.394175-08	2024-01-11 11:47:10.394175-08
\.



insert into mail.tags(slug, name)
values ('test-tag','Test Tag');

insert into mail.tagged(contact_id, tag_id)
select id, 1 from mail.contacts;

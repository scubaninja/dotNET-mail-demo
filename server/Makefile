run:
	. ./start.sh

test: db
	dotnet test

db:
	psql tailwind < ../db/db.sql --quiet

seed: db
	psql tailwind < ../db/seed.sql --quiet

mailpit:
	docker run -d \
	--restart unless-stopped \
	--name=mailpit \
	-p 8025:8025 \
	-p 1025:1025 \
	axllent/mailpit


.phony: test db seed change_1 mailhog

#pg_dump -d tailwind --no-owner --no-privileges --no-password --table "mail.contacts" --inserts -f ./dump.sql
#have to find/replace using ^\d*.\t
# The Mail App Database

This project uses a PostgreSQL database. Instead of using migrations or generated tooling, this directory contains the raw SQL files that define the schema. The database stores contacts, tags, broadcasts, email templates, message queues, sequences, and activity logs under the `mail` schema. 
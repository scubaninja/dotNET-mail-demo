drop schema if exists mail cascade;
create schema mail;
set search_path=mail;


create table contacts(
  id serial primary key,
  email text not null unique,
  key text not null default gen_random_uuid(),
  subscribed boolean not null default true,
  name text,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create table activity(
  id serial primary key,
  contact_id int not null references contacts(id),
  key text not null,
  description text not null,
  created_at timestamptz not null default now()
);

create table tags(
  id serial not null primary key,
  slug text not null unique,
  name text,
  description text,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);


create table tagged(
  contact_id int not null references contacts(id),
  tag_id int not null references tags(id),
  primary key (contact_id, tag_id)
);

create table sequences(
  id serial not null primary key,
  slug text not null unique,
  name text,
  description text,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

-- people that are subscribed to a sequence
create table subscriptions(
  contact_id int not null references contacts(id),
  sequence_id int not null references sequences(id),
  created_at timestamptz not null default now(),
  primary key (contact_id, sequence_id)
);

-- templates, which can belong to 0/n sequences. Transactionals don't have a sequence, but broadcasts
-- are single-sequence emails as they need to have a title and description for tracking purposes
create table emails(
  id serial not null primary key,
  sequence_id int references sequences(id),
  slug text not null unique,
  subject text not null,
  preview text,
  delay_hours int not null default 0,
  html text,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create table broadcasts(
  id serial primary key,
  email_id int not null references emails(id),
  slug text not null unique,
  status text not null default 'pending',
  name text not null,
  send_to_tag text,
  reply_to text not null default 'noreply@tailwindtraders.dev',
  created_at timestamptz not null default now(),
  processed_at timestamptz
);

-- This is a log table of actual emails sent and is *not* supposed to be relational
-- it's a historical log table so we need to be sure it stands on its own as a 
-- record of what happened
create table messages(
  id serial not null primary key,
  source text not null default 'broadcast', -- sequence or transaction
  slug text, -- don't want a hardcoded ID
  status text not null default 'pending',
  send_to text not null,
  send_from text not null,
  subject text not null,
  html text not null,
  send_at timestamptz,
  sent_at timestamptz,
  created_at timestamptz not null default now()
);

--seeds for testing

-- this function will queue a segment with the passed in HTML body
-- not too certain how this will work but... let's see

-- drop function if exists queue_broadcast(int);

-- create function queue_broadcast(bid int) 
-- returns setof broadcasts as $$

-- declare

--   em emails;
--   b broadcasts;

-- begin 
--   -- grab our template
  
--   select * into b from broadcasts where id = bid;
--   select * into em from email where id = b.email_id;

--   -- pop into the messages queue
--   insert into messages(broadcast_id, send_to, send_from, subject, html, send_at)
--   select b.id, contacts.email, b.reply_to, em.subject, em.html, now()
--   from contacts
--   inner join segments on segments.contact_id = contacts.id
--   where segments.group_id = b.group_id and contacts.subscribed = true;

--   -- update the broadcast
--   update broadcasts set message_count = (select count(1) from messages where broadcast_id = b.id)
--   where id = b.id;

--   select pg_notify('broadcasts',b.id);

--   return query 
--   select * from broadcasts where id = b.id;

-- end;



-- $$ language plpgsql;

-- insert into groups(slug,name)
-- values('default','Everyone');
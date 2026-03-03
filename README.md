# Welcome to Tailwind Traders Mail Service

We all need email... for better or worse. This service sends transactional emails via API and batch emails to a list, using a tag or predefined segment, like MailChimp does.

## What This Project Does

Tailwind Traders Mail Service provides a full-featured email platform that lets you:

- **Manage contacts** — sign up subscribers, tag them into segments, and handle opt-outs with one-click unsubscribe links
- **Send broadcast campaigns** — write emails in Markdown, target subscribers by tag, and queue messages for delivery
- **Send transactional emails** — deliver one-off emails triggered by user actions (e.g., purchase confirmations)
- **Generate content with AI** — use Azure OpenAI (GPT-4) to draft email copy from a prompt
- **Track activity** — log signups, opt-outs, link clicks, and delivery status for every contact

## Work In Progress

We're building things out actively... hopefully getting close to showing something soon!

## Architecture Design

This project consists of multiple components working together:

- **Server** (ASP.NET Core / .NET 8): RESTful API that manages contacts, broadcasts, and email delivery
- **Jobs** (Go): Background workers that process email queues and batch operations via Mage targets
- **CLI** (Node.js): Command-line tools that handle contact management, broadcast creation, and sending
- **Database** (PostgreSQL): Stores contacts, messages, tags, broadcasts, and activity history
- **Infrastructure**: Docker Compose for local development, Azure deployment for production
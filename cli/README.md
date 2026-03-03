# The Tailwind Mail App

This CLI, built with Node and Commander, reads and parses Markdown files to create and send email broadcasts. Each Markdown file contains YAML frontmatter with the subject, summary, slug, and target tag, followed by the email body.

The CLI provides commands to:

- **Initialize** a local `/mail` directory structure for broadcasts, contacts, and sent emails
- **Create** new broadcast templates (optionally using AI to generate the email body)
- **Validate** a broadcast against the API and display the subscriber count
- **Send** a broadcast by queueing it through the API
- **Search** contacts by email or name
- **Tag** contacts in bulk from a CSV file

## Running Things

You'll need Node installed. I'm using LTS 20 for this. If you need a Node version manager, `n` is great.

You'll also want to setup (and source) a `.env` file:

```
alias mdmail="node ./bin/mdmail.js"
alias mt="npm run test"

API_ROOT="http://localhost:5000/admin" #dotnet watch
```

This makes life easy as it emulates the CLI experience.

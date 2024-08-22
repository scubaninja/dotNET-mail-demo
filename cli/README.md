# The Tailwind Mail App

This is a prototype for now, using Node and Commander, which I know well. The goal is to create a CLI that reads in and parses markdown files, which will be used for broadcasts and, eventually, sequences.

The markdown bits will contain all the data needed for the broadcast.

## Running Things

You'll need Node installed. I'm using LTS 20 for this. If you need a Node version manager, `n` is great.

You'll also want to setup (and source) a `.env` file:

```
alias mdmail="node ./bin/mdmail.js"
alias mt="npm run test"

API_ROOT="http://localhost:5000/admin" #dotnet watch
```

This makes life easy as it emulates the CLI experience.

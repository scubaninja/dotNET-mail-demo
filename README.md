# Tailwind Traders Mail Service

Welcome to the Tailwind Traders Mail Service repository! This project is designed to handle email functionalities, including sending transactional emails via API and batch emails to a list using tags or predefined segments, similar to MailChimp.

## Table of Contents
- [Tailwind Traders Mail Service](#tailwind-traders-mail-service)
  - [Table of Contents](#table-of-contents)
  - [Overview](#overview)
  - [Work In Progress](#work-in-progress)
  - [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Installation](#installation)
    - [Running the Application](#running-the-application)
  - [CLI Application](#cli-application)
    - [Running the CLI](#running-the-cli)
  - [Database](#database)
  - [Contributing](#contributing)
  - [License](#license)

## Overview
This service aims to provide a robust email solution for transactional and batch email needs. It supports sending emails via API calls and managing email lists with tags or predefined segments.

## Work In Progress
Please note that this project is actively being developed. Features and functionalities are being built out, and we hope to have a stable release soon!

## Getting Started

### Prerequisites
To get started, ensure you have the following installed:
- [.NET](https://dotnet.microsoft.com/download)
- [Node.js (LTS 20)](https://nodejs.org/)
- [PostgreSQL](https://www.postgresql.org/)

### Installation
1. Clone the repository:
   ```sh
   git clone https://github.com/scubaninja/dotNET-mail-demo.git
   cd dotNET-mail-demo
   ```

2. Set up the environment variables by creating and sourcing a `.env` file:
   ```sh
   touch .env
   echo 'API_ROOT="http://localhost:5000/admin"' >> .env
   source .env
   ```

### Running the Application
To run the application, use the following commands:
```sh
dotnet run --project YourProjectName # Replace 'YourProjectName' with the actual project name
```

## CLI Application
The Tailwind Mail App includes a CLI for managing email broadcasts using markdown files. This prototype is built using Node.js and Commander.

### Running the CLI
1. Ensure Node.js is installed. If you need a Node version manager, `n` is recommended.
2. Set up your `.env` file and add the following aliases:
   ```sh
   alias mdmail="node ./bin/mdmail.js"
   alias mt="npm run test"
   source .env
   ```
3. Run the CLI commands using the aliases defined above.

## Database
The Mail App uses a PostgreSQL database. The main SQL scripts are located in the `db` directory. Instead of using migrations, the SQL scripts are manually maintained.

## Contributing
We welcome contributions! Please read our [Contributing Guide](CONTRIBUTING.md) to learn how you can help.

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

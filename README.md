# Welcome to Tailwind Traders Mail Service

We all need email... for better or worse. This service will send transactional emails via API or batch emails to a list, using a tag or predefined segment, like MailChimp does.

## Features

- **Transactional Email Delivery**: Send individual transactional emails via API
- **Broadcast Campaigns**: Create and send batch email campaigns to targeted segments
- **Contact Management**: Maintain a database of contacts with subscription status
- **Tagging System**: Organize contacts with tags for targeted communications
- **Markdown Support**: Create email content easily using Markdown format
- **Subscription Management**: Track opt-ins and opt-outs with full compliance features

## Architecture

The Tailwind Traders Mail Service consists of several components:

- **.NET Core Backend**: Provides the API and core functionality
- **PostgreSQL Database**: Stores contacts, broadcasts, and email templates
- **Background Jobs**: Processes email sending tasks asynchronously
- **CLI Tools**: Command-line interface for system management

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- PostgreSQL database
- Docker (optional, for containerized deployment)

### Setup

1. Clone the repository:

   ```bash
   git clone https://github.com/scubaninja/dotNET-mail-demo.git
   cd dotNET-mail-demo
   ```

2. Set up the database:

   ```bash
   cd db
   make setup
   ```

3. Run the server:

   ```bash
   cd ../server
   dotnet run
   ```

## Usage

### Creating a Broadcast

Broadcasts can be created from Markdown files that contain email content and metadata:

```csharp
// Create a broadcast from markdown content
var broadcast = Broadcast.FromMarkdown(markdownContent);
```

### Managing Contacts

Contacts can be added with tags for segmentation:

```csharp
// API endpoint or command for adding contacts with tags
```

### Sending Emails

Emails can be sent via API call or through the CLI tools:

```bash
./bin/mdmail-broadcast send --campaign=welcome-series
```

## Work In Progress

We're building things out actively... getting close to showing more features soon!

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

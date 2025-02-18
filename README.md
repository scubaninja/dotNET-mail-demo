# .NET Mail Demo

This project demonstrates email handling capabilities using .NET Core.

## Prerequisites

- .NET Core SDK (latest version)
- Docker (for development environment)
- Go and Mage (for build automation)

## Setup

1. Clone the repository:
```bash
git clone https://github.com/yourusername/dotNET-mail-demo.git
cd dotNET-mail-demo
```

2. Build the development container:
```bash
docker build -f jobs/dev.Dockerfile -t dotnet-mail-demo-dev .
```

3. Run the container:
```bash
docker run dotnet-mail-demo-dev
```

## Development

The project uses Mage for build automation. Common commands:

- `mage test:hello` - Run basic test
- `mage build` - Build the project
- `mage test` - Run all tests

## License

This project is licensed under the MIT License - see the LICENSE file for details.
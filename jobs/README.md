# The Jobs Service for Tailwind Traders Mail Service

jobs uses [mage](https://magefile.org/) a [magefile.go](./magefile.go) to run "jobs" both locally and within a container.

We support a more flexible container, [dev.Dockerfile](./dev.Dockerfile), and smaller and more secure container, [Dockerfile](./Dockerfile).

Finally, we use [GitHub Actions Workflows](./.github/workflows/build-and-publish.yaml) to build and push our container image
to GitHub Packages [Container Registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry).

Mage targets prefixed with `docker:*` are designed to help our
local "inner loop" during development and testing.

Common use cases include:
- Running the resulting container image on a serverless platform (e.g. Azure Container Apps), on Kubernetes, or a VM.
- Cloning and running mage directly, or pre-compiling a binary, to run on remote compute such as a VM.

See [docs/](./docs/) for for more documentation, or run `mage` to see currently available mage targets as follows:

```
$ mage
Targets:
  azure:containerRegistry        outputs ACCESS_TOKEN and LOGIN_SERVER environment variables for AZURE_CONTAINER_REGISTRY_NAME
  azure:keyVault                 connects to AZURE_KEY_VAULT_URL and lists the secrets
  azure:postgres                 outputs the environment variables to connect to the Postgres database in the provided <resource group>
  azure:serviceBus               connects to AZURE_SERVICEBUS_HOSTNAME and peeks 5 messages in AZURE_SERVICEBUS_HOSTNAME
  azure:storage                  connects to AZURE_STORAGE_ACCOUNT_NAME and lists the blogs in AZURE_STORAGE_CONTAINER_NAME
  deploy:containerApps           deploys the Container App(s) via containerapp.bicep into the provided <resource group> Requires: AZURE_SERVICEBUS_CONNECTION_STRING
  deploy:empty                   empties the <resource group> via empty.bicep
  deploy:group                   creates the <resource group> in <location>
  deploy:rbac                    deploys Role Based Access Control using rbac.bicep with the principalID of the currently signed in user
  deploy:storage                 deploys 5 "Storage" services via main.bicep into the provided <resource group>.
  deploy:storageAndPostgres      deploys Storage and Azure Database for Postgres
  docker:build                   builds the container image, "jobs", with --no-cache and Dockerfile which builds a static binary and multi-stage builds to utilize a distroless image
  docker:buildDev                builds the container image, "jobs", with --no-cache and dev.Dockerfile which uses the golang:latest image, installs mage and vim, for more interactive development
  docker:run                     runs the jobs container with the mage target
  email:getResult                gets the result of <id> from Azure Communication Services
  email:sendOne                  sends one test email to <to> via Azure Communications Services
  messages:queue                 creates messages in the queue that are ready to send using the Queuer and Sender defined by the MESSAGES_TYPE environment variable with options "test" (default), "smtp", or "azure"
  messages:send                  iterates over messages that have been inserted and sends them using the Queuer and Sender defined by the MESSAGES_TYPE environment variable with options "test" (default), "smtp", or "azure"
  serviceBus:receive             receives and completes a batch of 5 messages from the service bus queue with a 1 second delay between each message.
  serviceBus:receiveAll          receives and completes all messages from the Service Bus queue, in batches of 5, and exits when complete
  serviceBus:send                sends a single message to the service bus queue
  serviceBus:sendMessageBatch    sends a batch of 10 messages to the service bus queue with a 1 second delay between each message
  test:goodbye                   is an alternative mage target we can call
  test:hello                     is our default mage target which we also call by default within our Docker container
```

# Queuers

The `message` job (and `message:*` targets) combine senders and queuers to send emails. We support the following queuers:

## Test

Use this queuer for testing. It does not send any emails, but instead logs the email to the console using the `log/slog` package.

We recommend using this queuer when you develop a new one.

## Azure Service Bus

Azure Service Bus handles production queues.

To use this queuer, set the `AZURE_SERVICEBUS_CONNECTION_STRING` and `AZURE_SERVICEBUS_QUEUE_NAME` environment variables.

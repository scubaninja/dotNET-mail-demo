# Queuers

The `message` job (and `message:*` targets) combine senders and queuers to send emails. We support the following senders:

## Test

This queuer is used for testing. It does not send any emails, but instead logs the email to the console using the `log/slog` package.

We recommend using this queuer if you are developing a new queuer.

## Azure Service Bus

Azure Service Bus is used for produciton queues.

In order to use this queuer, you must set `AZURE_SERVICEBUS_CONNECTION_STRING` and `AZURE_SERVICEBUS_QUEUE_NAME` environment variables.

# Senders

The `message` job (and `message:*` targets) combine senders and queuers to send emails. We support the following senders:

## Test

This queuer is used for testing. It does not send any emails, but instead logs the email to the console using the `log/slog` package.

We recommend using this sender if you are developing a new sender.

## Azure Communication Services

Azure Communication Service is the recommended sender for sending emails in production. We have written a wrapper for the REST API at [azurecontainerservices.go](../senders/azurecontainerservices.go).

## SMTP

This queuer sends emails via SMTP.

We recommend either a production SMTP service, such as Twiliio Sendgrid, or a development SMTP service such as ethereal.email.

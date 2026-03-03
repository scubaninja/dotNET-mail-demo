# Senders

The `message` job (and `message:*` targets) combine senders and queuers to send emails. We support the following senders:

## Test

Use this sender for testing. It does not send any emails, but instead logs the email to the console using the `log/slog` package.

We recommend using this sender when you develop a new one.

## Azure Communication Services

We recommend Azure Communication Services as your sender for production emails. We wrote a wrapper for the REST API at [azurecontainerservices.go](../senders/azurecontainerservices.go).

## SMTP

This sender delivers emails via SMTP.

We recommend either a production SMTP service, such as Twilio Sendgrid, or a development SMTP service such as ethereal.email.

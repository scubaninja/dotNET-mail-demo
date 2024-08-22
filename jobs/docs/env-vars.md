# Environment variables

The following are example environment variables for local development, and deployment to Azure.

```bash
# service bus
export AZURE_SERVICEBUS_CONNECTION_STRING='...'
#export AZURE_SERVICEBUS_QUEUE_NAME='tailwind'
# for local testing -- we don't want prod to drain queues
export AZURE_SERVICEBUS_QUEUE_NAME='tailwind-test'
# for default
export AZURE_SERVICEBUS_HOSTNAME='sb231000.servicebus.windows.net'
# azure communication services
export ACS_ENDPOINT='https://acs231000.unitedstates.communication.azure.com/'
export ACS_KEY='...'
export ACS_FROM="DoNotReply@d018bc51-f36f-4493-82fe-0ff5c3377a9e.azurecomm.net"
# smtp via twilio sendgrid
export SMTP_FROM='user@example.com'
export SMTP_SERVER='smtp.sendgrid.net'
export SMTP_PORT='587'
export SMTP_USERNAME='apikey'
export SMTP_PASSWORD='...'
# ethereal.email
export ETHEREAL_NAME='Hello World'
export ETHEREAL_USERNAME='hello.world@ethereal.email'
export ETHEREAL_PASSWORD='...'
export ETHEREAL_SMTP_SERVER='smtp.ethereal.email'
#export SMTP_FROM=$ETHEREAL_USERNAME
#export SMTP_SERVER=$ETHEREAL_SMTP_SERVER
#export SMTP_PORT='587'
#export SMTP_USERNAME=$ETHEREAL_USERNAME
#export SMTP_PASSWORD=$ETHEREAL_PASSWORD
```
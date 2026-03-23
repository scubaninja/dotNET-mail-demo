# Welkom bij de Tailwind Traders Mailservice

E-mail — of we het nou leuk vinden of niet — is onmisbaar. Deze service verstuurt transactionele e-mails via een API of batchgewijs naar een lijst met ontvangers. Daarbij kun je filteren op tag of een vooraf gedefinieerd segment, vergelijkbaar met MailChimp.

## Architectuuroverzicht

Dit project bestaat uit meerdere componenten die samenwerken:

- **Server** (ASP.NET Core / .NET 8): RESTful API voor het beheren van contacten, broadcasts en e-mailbezorging
- **Jobs** (Go): Achtergrondwerkers voor het verwerken van e-mailwachtrijen en batchbewerkingen
- **CLI** (Node.js): Opdrachtregelprogramma's voor contactbeheer en broadcastbewerkingen
- **Database** (PostgreSQL): Gegevenspersistentie voor contacten, berichten en activiteitsregistratie
- **Infrastructuur**: Docker Compose voor lokale ontwikkeling, Azure-implementatie voor productie

---

## Server (ASP.NET Core / .NET 8)

De server is een .NET Minimal API die de backend vormt voor de Tailwind Traders maillijst.

### Omgevingsvariabelen

Stel de volgende variabelen in voordat je de server start:

```
ASPNETCORE_ENVIRONMENT="Development"
DATABASE_URL="postgres://gebruiker:wachtwoord@host/databasenaam:poort"

SMTP_USER=""
SMTP_PASSWORD=""
SMTP_HOST=""

DEFAULT_FROM="test@tailwind.dev"

ETHEREAL_USER="maak eenvoudig een account aan op ethereal.email"
ETHEREAL_PASSWORD=""
```

### Gebruiksscenario's

- **Jill wacht een broadcast naar 10.000 contacten**: Jill heeft 10.001 contacten in haar database, waarvan er één zich heeft afgemeld. De applicatie plaatst 10.000 berichten in de wachtrij voor verzending.
- **Jim wil zich aanmelden voor Jills lijst**: Hij dient een formulier in en ontvangt een dubbele opt-in link om zijn aanmelding te bevestigen.
- **Kim krijgt te veel e-mail** en besluit zich af te melden via een afmeldlink.
- **Jill stuurt een transactionele e-mail** naar mensen die haar boek kopen. Deze worden afzonderlijk verstuurd zonder afmeldlink.

### Implementatie op Azure

De map `server/Deployment/Azure/` bevat kant-en-klare implementatiescripts. Vereiste: Azure CLI (`az`) geïnstalleerd en ingelogd.

**Resources instellen:**

```bash
source ./Deployment/Azure/app_service.sh
```

Lees het script door en pas de variabelen bovenaan aan **voordat** je het uitvoert.

**Implementeren:**

```bash
source ./Deployment/Azure/zip.sh
```

Of via Make:

```bash
make app_service && make
```

Na `dotnet publish` zijn de implementatiebestanden te vinden in `/bin/Release/net8.0/publish`, inclusief de Svelte-applicatie en de `wwwroot`-map.

---

## Jobs (Go)

De Jobs-service verwerkt e-mailwachtrijen en batchbewerkingen op de achtergrond.

### Vereisten

- Een **Azure-abonnement** (bijv. [Gratis](https://aka.ms/azure-free-account) of [Student](https://aka.ms/azure-student-account))
- De [Azure CLI](https://docs.microsoft.com/nl-nl/cli/azure/install-azure-cli)
- Bash-shell (bijv. macOS, Linux, [WSL](https://docs.microsoft.com/nl-nl/windows/wsl/about), [Azure Cloud Shell](https://docs.microsoft.com/nl-nl/azure/cloud-shell/quickstart), [GitHub Codespaces](https://github.com/features/codespaces), enz.)
- [Go](https://go.dev/dl/) (optioneel)
- [Mage](https://magefile.org/) (`go install github.com/magefile/mage@latest`) (optioneel)

### Starten

Het project gebruikt [Mage](https://magefile.org/) als taakuitvoerder. Voer `mage` uit om alle beschikbare doelen te bekijken:

```bash
mage
```

Veelgebruikte doelen:

| Doel | Omschrijving |
|------|--------------|
| `messages:queue` | Plaatst berichten in de wachtrij klaar voor verzending |
| `messages:send` | Verzendt berichten die in de wachtrij staan |
| `docker:build` | Bouwt de container-image `jobs` |
| `docker:run` | Start de `jobs`-container |
| `deploy:containerApps` | Implementeert de Container App(s) op Azure |
| `azure:serviceBus` | Verbindt met Azure Service Bus en bekijkt berichten |

### Omgevingsvariabelen

```bash
# Service Bus
export AZURE_SERVICEBUS_CONNECTION_STRING='...'
export AZURE_SERVICEBUS_QUEUE_NAME='tailwind-test'   # gebruik 'tailwind' voor productie
export AZURE_SERVICEBUS_HOSTNAME='sb231000.servicebus.windows.net'

# Azure Communication Services
export ACS_ENDPOINT='https://acs231000.unitedstates.communication.azure.com/'
export ACS_KEY='...'
export ACS_FROM="DoNotReply@d018bc51-f36f-4493-82fe-0ff5c3377a9e.azurecomm.net"

# SMTP via Twilio SendGrid
export SMTP_FROM='gebruiker@voorbeeld.nl'
export SMTP_SERVER='smtp.sendgrid.net'
export SMTP_PORT='587'
export SMTP_USERNAME='apikey'
export SMTP_PASSWORD='...'

# ethereal.email (voor lokale ontwikkeling)
export ETHEREAL_NAME='Hallo Wereld'
export ETHEREAL_USERNAME='hallo.wereld@ethereal.email'
export ETHEREAL_PASSWORD='...'
export ETHEREAL_SMTP_SERVER='smtp.ethereal.email'
```

### Verzenders (Senders)

De `messages`-job combineert verzenders en wachtrijen om e-mails te versturen.

| Verzender | Omschrijving |
|-----------|--------------|
| **Test** | Logt e-mails naar de console via `log/slog`. Geen echte verzending. Aanbevolen voor het ontwikkelen van nieuwe verzenders. |
| **Azure Communication Services** | Aanbevolen verzender voor productie. Maakt gebruik van de REST API. |
| **SMTP** | Verstuurt e-mails via SMTP. Gebruik een productiedienst zoals Twilio SendGrid of een ontwikkelomgeving zoals ethereal.email. |

Stel de gewenste verzender in via de omgevingsvariabele `MESSAGES_TYPE` met waarden `test` (standaard), `smtp`, of `azure`.

### Wachtrijen (Queuers)

| Wachtrij | Omschrijving |
|----------|--------------|
| **Test** | Logt berichten naar de console. Geen echte wachtrij. Aanbevolen voor het ontwikkelen van nieuwe wachtrijen. |
| **Azure Service Bus** | Aanbevolen wachtrij voor productie. Vereist `AZURE_SERVICEBUS_CONNECTION_STRING` en `AZURE_SERVICEBUS_QUEUE_NAME`. |

### Containers

De `jobs`-service kan lokaal of op een VM worden uitgevoerd met Go en Mage, of als container worden gebouwd.

- **[Dockerfile](./jobs/Dockerfile)**: Bouwt een kleine, veilige container met een statisch binair bestand via multi-stage builds en een distroless image.
- **[dev.Dockerfile](./jobs/dev.Dockerfile)**: Flexibeler ontwikkelcontainer gebaseerd op `golang:latest`, inclusief Mage en Vim.

De [GitHub Actions-workflow](./.github/workflows/build-and-publish.yaml) bouwt en publiceert de container automatisch naar het GitHub Container Registry.

### Implementatie op Azure

Gebruik [Bicep](https://learn.microsoft.com/nl-nl/azure/azure-resource-manager/bicep/overview) voor het implementeren van Azure-resources. Alle implementatiescripts staan in de map [`deploy/`](./deploy/).

**Voorbeeld met Mage:**

```bash
# Implementeer compute in een aparte resourcegroep
mage deploy:group 231000-compute
export AZURE_SERVICEBUS_CONNECTION_STRING='...'
mage deploy:containerApps 231000-compute

# Implementeer opslag in een aparte resourcegroep
mage deploy:group 231000-opslag
mage deploy:storage 231000-opslag
mage deploy:rbac 231000-opslag

# Leeg beide resourcegroepen
mage deploy:empty 231000-compute
mage deploy:empty 231000-opslag
```

**Ondersteunde compute-platformen:**
- Azure Virtual Machines
- Azure Container Instances
- Azure Kubernetes Service

**Ondersteunde opslagplatformen:**
- Azure Blob Storage
- Azure Service Bus
- Azure Key Vault
- Azure Container Registry
- Azure Database for PostgreSQL (optioneel)

---

## CLI (Node.js)

De CLI is een prototype gebouwd met Node.js en Commander. Het doel is het inlezen en verwerken van Markdown-bestanden voor broadcasts en, uiteindelijk, e-mailsequenties.

### Vereisten

- Node.js LTS 20 (gebruik eventueel versiebeheerder `n`)

### Instellen

Maak een `.env`-bestand aan en stel de volgende aliassen in:

```bash
alias mdmail="node ./bin/mdmail.js"
alias mt="npm run test"

API_ROOT="http://localhost:5000/admin"   # dotnet watch
```

---

## Database (PostgreSQL)

De database is een PostgreSQL-database. In plaats van migraties of gegenereerde code te gebruiken, staan de SQL-scripts rechtstreeks in de map [`db/`](./db/).

---

## Lokale Ontwikkeling met Docker Compose

Voor lokale ontwikkeling is Docker Compose beschikbaar om alle services tegelijk op te starten. Zorg dat Docker en Docker Compose geïnstalleerd zijn en voer het volgende uit vanuit de projectroot:

```bash
docker compose up
```

---

## Toekomstige Plannen

De volgende functies zijn in ontwikkeling of gepland:

- Ondersteuning voor **Managed Identity** om de vereiste voor `AZURE_SERVICEBUS_CONNECTION_STRING` te verwijderen.
- Ondersteuning voor **Azure Database for PostgreSQL** als opslagback-end voor de wachtrij — een lokaal inzetbaar alternatief voor Azure Service Bus.
- Implementatie op **Azure Kubernetes Service (AKS)** met auto-scaling via KEDA (Kubernetes Event-driven Autoscaling).
- Implementatie op **Azure Virtual Machines** met ondersteuning voor Flatcar Linux.

Heb je aan een van deze functies gewerkt? Laat het ons weten via een issue of pull request!

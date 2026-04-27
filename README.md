# Tailwind Traders Mail Service

Een e-mailbeheerplatform voor het verzenden van transactionele en broadcast e-mails via een REST API. Organiseer je contacten met tags en verstuur gerichte campagnes—vergelijkbaar met diensten zoals MailChimp.

## Wat Dit Project Doet

Met deze mailservice kun je:

- **Transactionele e-mails versturen** - Activeer individuele e-mails programmatisch
- **Broadcast campagnes maken** - Zet bulk e-mails in de wachtrij voor alle abonnees of specifieke segmenten
- **Contacten beheren** - Volledige CRUD-operaties voor je abonneelijst
- **Segmenteren met tags** - Organiseer contacten in groepen voor gerichte berichten
- **Abonnementen afhandelen** - Publieke endpoints laten gebruikers opt-in en opt-out doen
- **E-mails schrijven in Markdown** - Schrijf e-mailinhoud in markdown met YAML frontmatter metadata
- **E-mails asynchroon verwerken** - Achtergrondwerker handelt het daadwerkelijke verzenden af
- **De API verkennen** - Swagger UI documentatie beschikbaar op de root URL

## Architectuur Overzicht

Het project is georganiseerd in vier hoofdmodules:

| Module | Technologie | Beschrijving |
|--------|-------------|--------------|
| `server/` | .NET 8 Minimal API | De hoofd REST API die alle e-mailoperaties afhandelt |
| `cli/` | Node.js + Commander | Command-line interface voor broadcast beheer |
| `jobs/` | Go + Mage | Achtergrondjob processor voor e-mail wachtrij |
| `db/` | PostgreSQL | Persistente opslag met het `mail` schema |

## Wat Je Nodig Hebt

Zorg dat je deze hebt geïnstalleerd:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js LTS 20](https://nodejs.org/) (nodig voor de CLI)
- [Go 1.21 of later](https://golang.org/) (nodig voor de jobs processor)
- [PostgreSQL 14 of later](https://www.postgresql.org/)
- [Docker](https://www.docker.com/) (optioneel, handig voor lokale e-mail tests)

## Aan De Slag

### 1. Haal De Code Op

Kloon deze repository naar je lokale machine:

```sh
cd jouw-projecten-map
git clone <repository-url>
cd dotNET-mail-demo
```

### 2. PostgreSQL Instellen

Voer het schema script uit tegen je database:

```sh
cd db
psql mijndb < db.sql
psql mijndb < seed.sql  # voegt wat testcontacten toe
```

### 3. Omgevingsvariabelen Configureren

Je moet deze omgevingsvariabelen instellen (maak een `.env` bestand of exporteer ze):

**Vereist:**
- `DATABASE_URL` - PostgreSQL connectiestring in formaat `postgres://gebruiker:wachtwoord@host:poort/dbnaam`
- `ASPNETCORE_ENVIRONMENT` - Zet op `Development` voor lokaal werk

**SMTP Configuratie:**
- `SMTP_HOST` - Je SMTP server hostnaam
- `SMTP_USER` - SMTP gebruikersnaam
- `SMTP_PASSWORD` - SMTP wachtwoord

**Optioneel:**
- `DEFAULT_FROM` - Standaard afzender e-mailadres
- `ETHEREAL_USER` / `ETHEREAL_PASSWORD` - Voor testen met Ethereal
- `SEND_WORKER` - Zet op `local` om de achtergrond e-mail verzender in te schakelen

### 4. Start De Server

Navigeer naar de server map en voer uit:

```sh
cd server
dotnet run
```

Bezoek `http://localhost:5000` om de Swagger UI met interactieve API documentatie te zien.

### 5. Lokale E-mail Capture Instellen (Optioneel)

Tijdens ontwikkeling, gebruik Mailpit om uitgaande e-mails op te vangen zonder ze daadwerkelijk te verzenden:

```sh
cd server
make mailpit
```

Dit start:
- SMTP capture op poort 1025
- Web UI op poort 8025 (bekijk opgevangen e-mails op http://localhost:8025)

## Hoe De Code Is Georganiseerd

```
dotNET-mail-demo/
├── server/                 # .NET 8 REST API
│   ├── Api/               # Route definities
│   │   ├── Admin/         # Admin-only routes (broadcasts, contacten, bulk ops)
│   │   └── PublicRoutes.cs # Publieke abonnement routes
│   ├── Commands/          # Business logica commando's
│   ├── Data/              # Database toegangslaag
│   ├── Models/            # Entiteit definities
│   ├── Services/          # E-mail verzender implementaties
│   └── Tests/             # Unit tests met xUnit
├── cli/                    # Node.js CLI tool
│   ├── commands/          # CLI commando handlers
│   └── bin/               # Entry point scripts
├── jobs/                   # Go achtergrond processor
│   ├── queuers/           # Wachtrij afhandeling
│   ├── senders/           # E-mail verzend implementaties
│   └── deploy/            # Deployment configuraties
├── db/                     # SQL schema en seeds
│   ├── db.sql             # Schema creatie script
│   └── seed.sql           # Testdata
├── deploy/                 # Deployment resources
└── docs/                   # Aanvullende documentatie
```

## Beschikbare API Endpoints

### Publieke Routes

| Methode | Pad | Wat Het Doet |
|---------|-----|--------------|
| `GET` | `/about` | Geeft info over de API terug |
| `GET` | `/unsubscribe/{key}` | Schrijft een contact uit met hun unieke sleutel |
| `GET` | `/link/clicked/{key}` | Logt een link klik voor tracking |
| `POST` | `/signup` | Voegt een nieuw contact toe aan de lijst |

### Admin Routes

| Methode | Pad | Wat Het Doet |
|---------|-----|--------------|
| `POST` | `/admin/validate` | Valideert markdown e-mail inhoud |
| `POST` | `/admin/queue-broadcast` | Maakt een broadcast en zet alle berichten in de wachtrij |
| `POST` | `/admin/get-chat` | Genereert e-mail inhoud met AI |
| `GET` | `/admin/contacts/search?term={term}` | Zoekt contacten op naam of e-mail |

## Database Tabellen

Alle tabellen bevinden zich in het `mail` schema:

| Tabel | Doel |
|-------|------|
| `contacts` | Abonnee informatie en abonnementsstatus |
| `tags` | Labels voor het organiseren van contacten in groepen |
| `tagged` | Koppeltabel die contacten aan hun toegewezen tags linkt |
| `emails` | E-mail template opslag |
| `broadcasts` | Campagne metadata en configuratie |
| `messages` | Individuele e-mails in de wachtrij met bezorgstatus |
| `activity` | Volgt abonnee interacties |
| `sequences` | Drip campagne definities (geplande functie) |

## Ontwikkel Workflow

### Tests Uitvoeren

Voer de testsuite uit:

```sh
cd server
dotnet test
```

Of gebruik de Makefile:

```sh
cd server
make test
```

### Het Project Bouwen

Compileer de server applicatie:

```sh
cd server
dotnet build --configuration Debug
```

> **Let op:** Dit project heeft een aangepaste configuratie waarbij package references alleen worden opgenomen wanneer niet in Release modus wordt gebouwd (zie `Tailwind.Mail.csproj`). Gebruik altijd `--configuration Debug` voor ontwikkeling om te zorgen dat alle dependencies beschikbaar zijn.

### Werken met de CLI

Stel de command-line tool in en gebruik deze:

```sh
cd cli
npm install
source .env  # Stelt aliases in
mdmail --help
```

### De Jobs Processor Gebruiken

Werk met de Go-gebaseerde job runner:

```sh
cd jobs
mage  # Toont alle beschikbare targets
mage test:hello  # Test of mage werkt
```

## De Applicatie Deployen

### Met Docker Compose

Bouw en draai alle services:

```sh
docker-compose up
```

### Azure Container Apps

Zie [jobs/README.md](./jobs/README.md) voor gedetailleerde Azure deployment instructies met Mage automatisering targets.

## Bijdragen

We verwelkomen bijdragen! Zo doe je dat:

1. Fork de repository
2. Maak een feature branch
3. Maak je wijzigingen met duidelijke commit berichten
4. Push je branch
5. Open een Pull Request

## Licentie

Dit project is gelicenseerd onder de MIT Licentie.

## Team & Contact

Gebouwd door Rob Conery, Aaron Wislang, en het Tailwind Traders Team.

Meer informatie op https://tailwindtraders.dev

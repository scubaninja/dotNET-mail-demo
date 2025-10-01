# Tervetuloa Tailwind Tradersin postipalveluun / Welcome to Tailwind Traders Mail Service

Tarvitsemme kaikki sähköpostia... paremmassa tai huonommassa. Tämä palvelu lähettää transaktio-sähköposteja API:n kautta tai erälähetyksiä listalle käyttäen tagia tai ennalta määritettyä segmenttiä, kuten MailChimp tekee.

We all need email... for better or worse. This service will send transactional emails via API or batch emails to a list, using a tag or predefined segment, like MailChimp does.

## Työn alla / Work In Progress

Rakennamme asioita aktiivisesti... toivottavasti pääsemme pian näyttämään jotain!

We're building things out actively... hopefully getting close to showing something soon!

## Ominaisuudet / Features

### Broadcast-lähetykset / Broadcast Sending
Broadcast-malli mahdollistaa massasähköpostilähetysten luomisen markdown-dokumenteista. Voit:

The Broadcast model enables creating mass email sends from markdown documents. You can:

- Luoda lähetyksiä markdown-sähköposteista / Create broadcasts from markdown emails
- Kohdistaa lähetykset tageilla / Target sends with tags
- Seurata kontaktien määrää / Track contact counts
- Hallita lähetyksen tilaa / Manage broadcast status

### Testikattavuus / Test Coverage

Projekti sisältää kattavat yksikkötestit Broadcast-mallille:

The project includes comprehensive unit tests for the Broadcast model:

- ✅ 17 läpäistyä testiä / 17 passing tests
- ✅ Täydellinen kattavuus kaikille julkisille metodeille / Full coverage of all public methods
- ✅ Poikkeustapausten testaus / Edge case testing
- ✅ Virheidenkäsittelyn validointi / Error handling validation

## Kehitys / Development

### Testien suorittaminen / Running Tests

```bash
cd server
dotnet test
```

### Testien suorittaminen kattavuusraportilla / Running Tests with Coverage

```bash
cd server
dotnet test --collect:"XPlat Code Coverage"
```

### Tietokannan alustaminen / Database Setup

```bash
cd server
make db      # Luo tietokanta / Create database
make seed    # Täytä testidatalla / Seed with test data
```

## Teknologiat / Technologies

- **.NET 8.0** - Web API framework
- **PostgreSQL** - Tietokanta / Database
- **Dapper** - Kevyt ORM / Lightweight ORM
- **Markdig** - Markdown-käsittely / Markdown processing
- **xUnit** - Testausframework / Testing framework
- **Moq** - Mocking-kirjasto testaukseen / Mocking library for tests

## Dokumentaatio / Documentation

Lisää dokumentaatiota löytyy:

More documentation can be found in:

- `server/README.md` - API-dokumentaatio / API documentation
- `cli/README.md` - CLI-työkalun dokumentaatio / CLI tool documentation
- `db/README.md` - Tietokantarakenne / Database structure
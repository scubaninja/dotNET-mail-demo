# Barrierefreiheit - Zugänglichkeit in Tailwind Mail

Diese Dokumentation beschreibt die Barrierefreiheitsfunktionen von Tailwind Mail, die eine verbesserte Benutzererfahrung für Menschen mit Behinderungen ermöglichen.

## Übersicht

Tailwind Mail implementiert verschiedene Barrierefreiheitsfunktionen gemäß den Web Content Accessibility Guidelines (WCAG) 2.1:

- **Bildschirmleser-Unterstützung**: Verbesserte UX für Screenreader-Nutzer
- **Live-Regionen**: Dynamische Inhaltsaktualisierungen werden angekündigt
- **Semantische Struktur**: Korrekte HTML-Struktur für assistive Technologien
- **Tastaturnavigation**: Vollständige Bedienbarkeit ohne Maus

## E-Mail-Barrierefreiheit

### Zugängliche E-Mail-Vorlage

Alle E-Mails, die über Tailwind Mail versendet werden, verwenden automatisch eine barrierefreie HTML-Vorlage mit folgenden Merkmalen:

#### 1. Semantische HTML-Struktur

```html
<header role="banner">...</header>
<main role="main" aria-label="E-Mail-Inhalt">...</main>
<footer role="contentinfo">...</footer>
```

Die Vorlage verwendet semantische HTML5-Elemente und ARIA-Rollen für eine klare Dokumentstruktur.

#### 2. Live-Regionen für dynamische Inhalte

```html
<main aria-live="polite" aria-atomic="true">
    <!-- Inhalt wird bei Änderungen von Screenreadern angekündigt -->
</main>
```

Das `aria-live="polite"` Attribut stellt sicher, dass Screenreader Inhaltsänderungen ankündigen, ohne den aktuellen Lesefluss zu unterbrechen.

#### 3. Übersprunglinks

```html
<a href="#main-content" class="skip-link">Zum Hauptinhalt springen</a>
```

Ermöglicht Tastaturnutzern, direkt zum Hauptinhalt zu navigieren.

#### 4. Versteckter Vorschautext

```html
<div aria-hidden="true">Vorschautext für E-Mail-Clients</div>
```

Der Vorschautext ist für Screenreader ausgeblendet, um Redundanz zu vermeiden.

### Verwendung im Markdown

Sie können die Sprache Ihrer E-Mail im Frontmatter angeben:

```yaml
---
Subject: "Willkommen bei Tailwind Mail"
Summary: "Ihre Anmeldung war erfolgreich"
Lang: "de"
---

# Willkommen!

Vielen Dank für Ihre Anmeldung...
```

Die `Lang`-Angabe setzt das `lang`-Attribut im HTML-Dokument, was für Screenreader essentiell ist.

## API für Barrierefreiheit

### AccessibleEmailTemplate-Service

Der `AccessibleEmailTemplate`-Service bietet folgende Methoden:

#### Wrap

```csharp
string html = AccessibleEmailTemplate.Wrap(
    subject: "E-Mail-Betreff",
    preview: "Vorschautext",
    bodyHtml: "<p>E-Mail-Inhalt</p>",
    lang: "de"  // Sprachcode
);
```

Wickelt E-Mail-Inhalte in eine barrierefreie HTML-Vorlage.

#### CreateStatusMessage

```csharp
string status = AccessibleEmailTemplate.CreateStatusMessage(
    message: "Ihre E-Mail wurde gesendet",
    isAssertive: false  // Verwendet aria-live="polite"
);
```

Erstellt eine zugängliche Statusmeldung für dynamische Aktualisierungen.

#### CreateAlert

```csharp
string alert = AccessibleEmailTemplate.CreateAlert(
    message: "Wichtiger Hinweis: Aktion erforderlich"
);
```

Erstellt eine Warnmeldung mit `role="alert"` für sofortige Ankündigung.

## CSS-Funktionen für Barrierefreiheit

Die Vorlage enthält spezielle CSS-Regeln:

### Hoher Kontrast

```css
@media (prefers-contrast: high) {
    body {
        background-color: #ffffff;
        color: #000000;
    }
    a {
        color: #0000ee;
    }
}
```

Unterstützt Betriebssystem-Einstellungen für hohen Kontrast.

### Reduzierte Bewegung

```css
@media (prefers-reduced-motion: reduce) {
    * {
        animation: none !important;
        transition: none !important;
    }
}
```

Respektiert Nutzereinstellungen für reduzierte Bewegung.

### Screenreader-Only-Klasse

```css
.sr-only {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip: rect(0, 0, 0, 0);
    white-space: nowrap;
    border: 0;
}
```

Blendet Inhalte visuell aus, aber macht sie für Screenreader verfügbar.

## CLI-Barrierefreiheit

Die Kommandozeilen-Anwendung (CLI) verwendet klare, strukturierte Ausgaben:

- **Erfolgs-/Fehlermeldungen**: Deutlich gekennzeichnet mit Symbolen
- **Interaktive Prompts**: Klare Anweisungen und Standardwerte
- **Fortschrittsanzeigen**: Visuelle Spinner mit Textbeschreibungen

## Best Practices

### E-Mail-Inhalte erstellen

1. **Überschriftenhierarchie**: Verwenden Sie `#`, `##`, `###` in logischer Reihenfolge
2. **Alt-Texte für Bilder**: Beschreiben Sie Bilder im Markdown mit `![Beschreibung](url)`
3. **Linktext**: Verwenden Sie aussagekräftige Linktexte statt "hier klicken"
4. **Farbkontrast**: Stellen Sie sicher, dass Text-Hintergrund-Kontrast mindestens 4.5:1 beträgt

### Beispiel für barrierefreien Markdown

```markdown
---
Subject: "Newsletter März 2024"
Summary: "Produktupdates und Tipps für diesen Monat"
Lang: "de"
---

# Newsletter März 2024

## Neue Produktfunktionen

Wir haben diesen Monat einige spannende Neuerungen:

- **Barrierefreie E-Mails**: Verbesserte Screenreader-Unterstützung
- **Live-Regionen**: Dynamische Inhalte werden angekündigt

[Mehr erfahren über unsere Barrierefreiheitsfunktionen](/accessibility)

## Kontakt

Bei Fragen erreichen Sie uns unter [support@tailwind.dev](mailto:support@tailwind.dev).
```

## WCAG 2.1 Konformität

Diese Implementierung adressiert folgende WCAG-Erfolgskriterien:

| Kriterium | Stufe | Beschreibung |
|-----------|-------|--------------|
| 1.3.1 | A | Info und Beziehungen |
| 1.4.3 | AA | Kontrast (Minimum) |
| 2.1.1 | A | Tastatur |
| 2.4.1 | A | Blöcke umgehen |
| 2.4.2 | A | Seite mit Titel versehen |
| 4.1.1 | A | Syntaxanalyse |
| 4.1.2 | A | Name, Rolle, Wert |
| 4.1.3 | AA | Statusmeldungen |

## Weitere Ressourcen

- [WCAG 2.1 Richtlinien (Deutsch)](https://www.w3.org/Translations/WCAG21-de/)
- [BITV 2.0 - Barrierefreie-Informationstechnik-Verordnung](https://www.gesetze-im-internet.de/bitv_2_0/)
- [Aktion Mensch - Barrierefreie E-Mails](https://www.aktion-mensch.de/inklusion/barrierefreiheit)

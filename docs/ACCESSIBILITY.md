# Accessibility Features in Tailwind Mail

This documentation describes the accessibility features implemented in Tailwind Mail to provide an improved user experience for people with disabilities.

## Overview

Tailwind Mail implements various accessibility features in accordance with the Web Content Accessibility Guidelines (WCAG) 2.1:

- **Screen Reader Support**: Enhanced UX for screen reader users
- **Live Regions**: Dynamic content updates are announced to assistive technologies
- **Semantic Structure**: Correct HTML structure for assistive technologies
- **Keyboard Navigation**: Full functionality without a mouse

## Email Accessibility

### Accessible Email Template

All emails sent through Tailwind Mail automatically use an accessible HTML template with the following features:

#### 1. Semantic HTML Structure

```html
<header role="banner">...</header>
<main role="main" aria-label="Email content">...</main>
<footer role="contentinfo">...</footer>
```

The template uses semantic HTML5 elements and ARIA roles for clear document structure.

#### 2. Live Regions for Dynamic Content

```html
<main aria-live="polite" aria-atomic="true">
    <!-- Content changes are announced by screen readers -->
</main>
```

The `aria-live="polite"` attribute ensures that screen readers announce content changes without interrupting the current reading flow.

#### 3. Skip Links

```html
<a href="#main-content" class="skip-link">Skip to main content</a>
```

Allows keyboard users to navigate directly to the main content.

#### 4. Hidden Preview Text

```html
<div aria-hidden="true">Preview text for email clients</div>
```

Preview text is hidden from screen readers to avoid redundancy.

### Usage in Markdown

You can specify the language of your email in the frontmatter:

```yaml
---
Subject: "Welcome to Tailwind Mail"
Summary: "Your registration was successful"
Lang: "en"
---

# Welcome!

Thank you for signing up...
```

The `Lang` attribute sets the `lang` attribute in the HTML document, which is essential for screen readers.

## Accessibility API

### AccessibleEmailTemplate Service

The `AccessibleEmailTemplate` service provides the following methods:

#### Wrap

```csharp
string html = AccessibleEmailTemplate.Wrap(
    subject: "Email Subject",
    preview: "Preview text",
    bodyHtml: "<p>Email content</p>",
    lang: "en"  // Language code
);
```

Wraps email content in an accessible HTML template.

#### CreateStatusMessage

```csharp
string status = AccessibleEmailTemplate.CreateStatusMessage(
    message: "Your email has been sent",
    isAssertive: false  // Uses aria-live="polite"
);
```

Creates an accessible status message for dynamic updates.

#### CreateAlert

```csharp
string alert = AccessibleEmailTemplate.CreateAlert(
    message: "Important notice: Action required"
);
```

Creates an alert message with `role="alert"` for immediate announcement.

## CSS Accessibility Features

The template includes special CSS rules:

### High Contrast Mode

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

Supports operating system settings for high contrast.

### Reduced Motion

```css
@media (prefers-reduced-motion: reduce) {
    * {
        animation: none !important;
        transition: none !important;
    }
}
```

Respects user settings for reduced motion.

### Screen Reader Only Class

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

Hides content visually but makes it available to screen readers.

## CLI Accessibility

The command-line application (CLI) uses clear, structured outputs:

- **Success/Error Messages**: Clearly marked with symbols and prefixes
- **Interactive Prompts**: Clear instructions and default values
- **Progress Indicators**: Visual spinners with text descriptions

### CLI Accessibility Utilities

The CLI includes an accessibility module (`cli/lib/accessibility.js`) with helpers for:

- `announceStatus(status, message)` - Announces status with consistent format
- `announceSuccess(message)` - Announces successful operations
- `announceError(message)` - Announces errors clearly
- `announceProgress(current, total, item)` - Announces progress updates
- `announceList(title, items)` - Announces lists in accessible format

## Best Practices

### Creating Email Content

1. **Heading Hierarchy**: Use `#`, `##`, `###` in logical order
2. **Image Alt Text**: Describe images in markdown with `![Description](url)`
3. **Link Text**: Use descriptive link text instead of "click here"
4. **Color Contrast**: Ensure text-background contrast is at least 4.5:1

### Example of Accessible Markdown

```markdown
---
Subject: "March 2024 Newsletter"
Summary: "Product updates and tips for this month"
Lang: "en"
---

# March 2024 Newsletter

## New Product Features

We have some exciting new features this month:

- **Accessible Emails**: Improved screen reader support
- **Live Regions**: Dynamic content is announced

[Learn more about our accessibility features](/accessibility)

## Contact

For questions, reach us at [support@tailwind.dev](mailto:support@tailwind.dev).
```

## WCAG 2.1 Compliance

This implementation addresses the following WCAG success criteria:

| Criterion | Level | Description |
|-----------|-------|-------------|
| 1.3.1 | A | Info and Relationships |
| 1.4.3 | AA | Contrast (Minimum) |
| 2.1.1 | A | Keyboard |
| 2.4.1 | A | Bypass Blocks |
| 2.4.2 | A | Page Titled |
| 4.1.1 | A | Parsing |
| 4.1.2 | A | Name, Role, Value |
| 4.1.3 | AA | Status Messages |

## Additional Resources

- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [WAI-ARIA Authoring Practices](https://www.w3.org/WAI/ARIA/apg/)
- [WebAIM Email Accessibility](https://webaim.org/techniques/email/)

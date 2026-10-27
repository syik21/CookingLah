# CookingLah Design Guidelines

> **Mandatory before any UI work:** Read this document and inspect `Style.css` before creating, modifying, or redesigning any CookingLah page or component. For map work, inspect the shared map section in `Style.css` before implementation. Keep this document aligned with the actual implementation whenever shared design rules change.

## 1. Global design system (`Style.css`)

### Colour tokens
| Token | Value | Usage |
|---|---|---|
| `--color-bg` | `#FFF9EB` | Cream background and surfaces |
| `--color-primary` | `#F93209` | Red-orange accent and primary buttons |
| `--color-ink` | `#35030C` | Text, borders, hard shadows |
| `--color-text-soft` | `#555555` | Secondary text |
| `--color-lime` | `#CDF58A` | Accent |
| `--color-pink` | `#FBB8EE` | Accent |
| `--color-yellow` | `#FFD24C` | Accent |
| `--color-lavender` | `#B99CFF` | Accent |
| `--color-grid` | `#F1E6CC` | Existing grid colour token (no gradient grid background) |
| `--color-tan` | `#F3E5C3` | Available map regions and surfaces |
| `--color-tan-light` | `#F7EFD9` | Light tan surface and hover |

Typography: `--font-hand` is Caveat Brush, `--font-display` is Archivo Black, and `--font-body` is Space Mono, each with fallbacks. Headings use uppercase handwritten display styling; body text is 15px / 1.65. Fonts are imported by `Style.css`.

Layout: `--content-width: 1120px`, `--gutter: 24px`, `--grid-size: 28px`; `.container` centers content and `.section` uses 80px vertical padding. There is no general-purpose spacing scale beyond these defined values; follow established component spacing instead of inventing tokens.

Borders: `--border: 2px solid var(--color-ink)` and `--border-dashed: 2px dashed var(--color-ink)`. Radii: `--radius-sm: 10px`, `--radius-md: 16px`, `--radius-lg: 24px`, `--radius-sheet: 36px`, `--radius-pill: 999px`. Shadows are hard offset: `--shadow-sm`, `--shadow-sm-hover`, `--shadow-md` (4px, 2px, 6px offsets respectively). Avoid introducing blurred shadows in shared components.

Shared components: `.btn`, `.btn-primary`, `.btn-secondary` use outlined, offset-shadow buttons; `.card`, `.card-title`, `.card-eyebrow` define card treatment; `.sticker` variants, `.pill`, `.tag`, `.tag-list`, `.form-select` define reusable UI. `.site-header`, `.site-logo`, `.site-footer`, `.section-title`, `.section-description` and utility classes are also global. Prefer these classes before introducing page-only equivalents. `.form-select` now uses a solid background without gradient-drawn arrow decoration.

Course thumbnails without an uploaded image use the shared `.course-image-placeholder`: the mascot appears above “Food Image Coming Soon” within the same dimensions as that page's populated course-image frame.

## 2. Layout and responsiveness

- Use `.container` for the standard centered 1120px maximum-width content region with 24px gutters.
- Global header is a flex row with spaced alignment; footer is flex with wrapping. Do not impose a universal sidebar: the dashboard has its own structure.
- Use grid for structured content and map stages, flex for horizontal controls and groups, following existing page patterns.
- Global mobile adjustment occurs at `max-width: 420px` (header wrapping, logo and button sizing). `Main.css` uses `760px` and `420px` breakpoints for its page-only layout; shared map breakpoints now live in `Style.css`. The learner dashboard additionally contains `860px` and `760px` rules. Retain these breakpoints and test mobile overflow, stacked cards, and map scaling.
- Preserve consistent text alignment, whitespace, and focus indicators. Global reduced-motion rules disable transitions/animations where requested.

## 3. Shared map design (`Style.css`)

**Mandatory map rule:** Whenever implementing or modifying a map in CookingLah, inspect `DESIGN_GUIDELINES.md` and the map components in `Style.css` first. Reuse the established map design rather than creating a new one.

- `.main-map-section`: rounded sheet using the global border and `--radius-sheet`, with 64px/80px vertical padding (44px/56px at 420px).
- `.country-chips`: centered, wrapping flex controls with 8px gap.
- `.map-stage`: one-column grid with 20px gap and relative positioning.
- `.world-map`: global outlined, rounded map surface with 8px inner padding, hidden overflow, and responsive SVG (`width:100%; height:auto`). Paths have ink outlines, tan available states, ink hover, and primary selected state.
- `.map-tooltip`: absolutely positioned dark-ink tooltip, cream text, 12px bold body font, pointer-events disabled.
- The shared world map has `.map-controls` for enlargement and 100–300% zoom. `.map-dialog` uses a native modal dialog with Escape dismissal and focus restoration; the map surface scrolls at higher zoom. `.map-selection-marker` keeps a pin on selected land and a readable country label at each zoom level. Controls and marker styles live in `Style.css`.
- `.country-card`: two-column detail panel, stacking to one column at 760px.
- The world map toolbar sits inside the bordered `.world-map` surface, above a separately scrollable `.map-viewport`, keeping controls accessible while zoomed. Marker anchors use the largest land polygon's area centroid, with an interior fallback for concave outlines; detached islands do not pull the pin toward the coastline.
- The learner progress map reuses `.map-stage`, `.world-map`, and `.map-tooltip`. Its reusable `.pm-*` map layout, completed/selected country state styling, legend, detail card, lesson display and responsive rules also live in `Style.css`. JavaScript interactions and map data remain in `Learner.aspx`. Do not duplicate map CSS in page-specific stylesheets.
- Map interactions and keyboard access must remain operational; map data and event logic belong in page functionality, not shared CSS.
- World map country paths use a dashed geographic border for keyboard focus instead of a rectangular outline; mouse selection retains the normal country border.
- The world map viewport supports mouse/pen dragging with grab cursors and pointer capture. A 5px threshold preserves country clicks; a completed drag suppresses accidental selection. Touch retains native scrolling and pinch zoom, and the focusable viewport supports keyboard scrolling.

## 4. Mandatory styling rules

1. **No colour gradients anywhere.** Never use `linear-gradient()`, `radial-gradient()`, `conic-gradient()`, repeating variants, gradient masks, overlays, or gradient hover effects. Use solid colours from the existing palette; maintain hierarchy through spacing, borders, typography and shadows.
2. `Style.css` is the single source of truth for global tokens and reusable components. Reuse existing variables; do not redeclare them in page stylesheets.
3. Scope page-specific selectors where practical, and never allow page CSS to unintentionally change unrelated pages.
4. Avoid inline styles, embedded style blocks, duplicate declarations and conflicting overrides. Dynamic inline width values set by dashboard JavaScript for progress indicators are a functional exception.
5. Preserve the established CookingLah visual language unless a redesign is explicitly requested.
6. Maintain responsive behaviour, accessibility, and map keyboard interactions.
7. Update this document whenever the established design system changes, based on the actual code, not hypothetical values.

## 5. CSS and page file organisation

| File | Responsibility |
|---|---|
| `Style.css` | Global tokens, reset, typography, shared components, **all shared map CSS**, map states and responsive map rules |
| `Main.css` | Main-page-only hero, stickers, recipe section and their responsive layout; **no map CSS** |
| `learnerDashboard.css` | Learner-only non-map layout and components; **no map styling** |
| `Learner.aspx` | Dashboard HTML/ASPX markup and existing client-side functionality |
| `DESIGN_GUIDELINES.md` | Continuously maintained design and implementation reference |

Load `Style.css` first on every page. Load only the required page stylesheet afterward (`Main.css` for the main page, `learnerDashboard.css` for the learner dashboard). Pages do **not** need to load `Main.css` to use maps. The learner dashboard now links only `Style.css` and `learnerDashboard.css`; the main page uses `Style.css` and `Main.css`.

**Shared map class inventory:** `.main-map-section`, `.country-chips`, `.map-stage`, `.world-map`, `.map-tooltip`, `.country-card` and the `.pm-*` progress-map classes are all defined in `Style.css`. The `760px` card stacking and `420px` map-sheet adjustments also belong there. For new map pages, link `Style.css` and reuse the existing class patterns; avoid creating new map selectors in page CSS unless genuinely page-specific functionality demands them.

## 6. Implementation checks

Before merging: check stylesheet paths; inspect all `var(...)` references; search CSS for `gradient(`; review duplicate selectors; test map hover, selection, completion and tooltip states; test the dashboard at desktop and 860px/760px/420px breakpoints; test ASP.NET Web Forms compilation in the actual project; and update this document when conventions change.

The supplied files contain a standalone HTML dashboard with client-side JavaScript and no code-behind. ASPX project-level compilation, authentication integration, and visual browser regression testing require the full ASP.NET solution and cannot be established from these standalone files alone.

## 7. Public master page

`Site.Master` owns the public document, single server form, and shared navigation. `Main.aspx` supplies content through `HeadContent`, `MainContent`, and `FooterContent`; `ScriptsContent` is available for page scripts. The FriendlyUrls mobile master forwards these slots to the same responsive shell.

Load `Style.css`, then `F_CSS/SiteMaster.css`, then page CSS. Header-only styles belong in `SiteMaster.css` and menu interactions in `F_JavaScripts/SiteMaster.js`. The public shell uses a cream SVG graph-paper background. Its full-width header has the existing logo and a centred cream navigation panel with a fine, subtly irregular ink border and regular-weight Space Mono labels. Navigation has no icons or offset shadow; the active link uses a small red hand-drawn underline. Join Us uses the unmodified shared `.btn.btn-primary` appearance outside the navigation. The menu collapses at 1000px with a 600px two-column menu layout; without JavaScript all links remain visible. Preserve keyboard focus, Escape dismissal, and reduced-motion support.

Header destinations are Main.aspx, Main.aspx#map, Main.aspx#recipes (Cooking Basics), Main.aspx#about, and the existing login/signup page. Main.aspx keeps its shared world-map control; its About layout belongs in Main.css.

## Signup country picker

The shared login/signup page uses `F_JavaScripts/LoginSignupPage.js` and `F_CSS/LoginSignupPage.css` for its country picker. A native server dropdown is populated from .NET region data on every initialization and remains available without JavaScript. With JavaScript it becomes a searchable native dialog with a cream paper surface, fine ink border, country names/codes, selected state, result count and empty state. Escape closes it and restores focus; search supports Arrow Down and the results support arrow-key navigation. Both learner and chef signups use this same required country field.

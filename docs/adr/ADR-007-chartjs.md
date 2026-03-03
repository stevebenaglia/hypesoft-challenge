# ADR-007: Chart.js over Recharts for Dashboard Visualizations

- **Status**: Accepted
- **Date**: 2025-01-15

## Context

The dashboard requires a bar chart displaying product count grouped by category. Two popular React chart libraries were evaluated: **Recharts** (SVG-based, React-native) and **Chart.js** with **react-chartjs-2** (Canvas-based, framework-agnostic).

## Decision

Use **Chart.js v4** with **react-chartjs-2 v5**:

- Renders to `<canvas>`, which avoids SVG DOM overhead for datasets with many data points.
- `react-chartjs-2` provides a thin React wrapper with full access to Chart.js configuration options.
- Loaded via `next/dynamic` with `{ ssr: false }` to avoid server-side canvas hydration errors (canvas API is not available in Node.js).

## Consequences

**Positive:**
- Canvas rendering is more performant than SVG for large datasets.
- Chart.js has a mature, well-documented configuration API for fine-grained styling.
- Widely adopted with extensive community examples.

**Negative:**
- Canvas-based charts are less accessible than SVG (no DOM nodes for screen readers without additional ARIA attributes).
- Requires `{ ssr: false }` dynamic import, which introduces a loading flash on initial render.
- Recharts would have been more idiomatic in a React-first codebase (declarative component composition vs imperative configuration objects).

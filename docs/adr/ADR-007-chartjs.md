# ADR-007: Chart.js em vez de Recharts para Visualizações / Chart.js over Recharts for Dashboard Visualizations

- **Status**: Aceito / Accepted
- **Data / Date**: 2025-01-15

---

## Contexto

O dashboard requer um gráfico de barras exibindo a contagem de produtos agrupados por categoria. Duas bibliotecas populares de gráficos para React foram avaliadas: **Recharts** (baseado em SVG, nativa React) e **Chart.js** com **react-chartjs-2** (baseado em Canvas, agnóstica de framework).

## Decisão

Usar **Chart.js v4** com **react-chartjs-2 v5**:

- Renderiza em `<canvas>`, evitando overhead de DOM SVG para datasets com muitos pontos de dados.
- `react-chartjs-2` fornece um wrapper React thin com acesso completo às opções de configuração do Chart.js.
- Carregado via `next/dynamic` com `{ ssr: false }` para evitar erros de hidratação server-side com canvas (a API canvas não está disponível no Node.js).

## Consequências

**Positivas:**
- Renderização em canvas é mais performática que SVG para datasets grandes.
- Chart.js tem uma API de configuração madura e bem documentada para estilização detalhada.
- Amplamente adotado com vasta comunidade e exemplos disponíveis.

**Negativas:**
- Gráficos baseados em canvas são menos acessíveis que SVG (sem nós DOM para leitores de tela sem atributos ARIA adicionais).
- Requer dynamic import com `{ ssr: false }`, introduzindo um flash de carregamento no render inicial.
- Recharts teria sido mais idiomático em um codebase centrado em React (composição declarativa de componentes vs objetos de configuração imperativos).

---

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

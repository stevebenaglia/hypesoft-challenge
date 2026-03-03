import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";
import type { Product, Category, DashboardSummary } from "@/types/api";
import { formatCurrency } from "@/utils/formatters";

const BRAND = "HYPESOFT";
const BRAND_COLOR: [number, number, number] = [109, 40, 217]; // violet-700
const LOW_STOCK_THRESHOLD = 10;

function formatDate(): string {
  return new Intl.DateTimeFormat("pt-BR", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date());
}

function addHeader(doc: jsPDF, title: string) {
  const pageW = doc.internal.pageSize.getWidth();

  // Brand name
  doc.setFont("helvetica", "bold");
  doc.setFontSize(16);
  doc.setTextColor(...BRAND_COLOR);
  doc.text(BRAND, 14, 18);

  // Date (top-right)
  doc.setFont("helvetica", "normal");
  doc.setFontSize(9);
  doc.setTextColor(120, 120, 120);
  doc.text(formatDate(), pageW - 14, 18, { align: "right" });

  // Report title
  doc.setFont("helvetica", "bold");
  doc.setFontSize(13);
  doc.setTextColor(30, 30, 30);
  doc.text(title, 14, 28);

  // Divider line
  doc.setDrawColor(200, 200, 200);
  doc.line(14, 32, pageW - 14, 32);
}

export function exportProductsPdf(
  products: Product[],
  filters?: { search?: string; category?: string; stockFilter?: string }
) {
  const doc = new jsPDF();
  addHeader(doc, "Relatório de Produtos");

  const rows = products.map((p) => [
    p.name,
    p.categoryName ?? "—",
    formatCurrency(p.price),
    `${p.stockQuantity} un.`,
    p.stockQuantity < LOW_STOCK_THRESHOLD ? "Estoque Baixo" : "Normal",
  ]);

  autoTable(doc, {
    startY: 37,
    head: [["Produto", "Categoria", "Preço", "Estoque", "Status"]],
    body: rows,
    headStyles: {
      fillColor: BRAND_COLOR,
      textColor: 255,
      fontStyle: "bold",
      fontSize: 9,
    },
    bodyStyles: { fontSize: 8.5 },
    alternateRowStyles: { fillColor: [248, 248, 252] },
    didParseCell(data) {
      if (
        data.section === "body" &&
        data.column.index === 4 &&
        data.cell.raw === "Estoque Baixo"
      ) {
        data.cell.styles.textColor = [180, 80, 0];
        data.cell.styles.fontStyle = "bold";
      }
    },
  });

  // Footer with totals
  const finalY = (doc as jsPDF & { lastAutoTable: { finalY: number } })
    .lastAutoTable.finalY + 6;
  doc.setFontSize(8.5);
  doc.setTextColor(100, 100, 100);
  doc.setFont("helvetica", "normal");

  const activeFilters: string[] = [];
  if (filters?.search) activeFilters.push(`Busca: "${filters.search}"`);
  if (filters?.category && filters.category !== "all")
    activeFilters.push(`Categoria: ${filters.category}`);
  if (filters?.stockFilter === "low") activeFilters.push("Estoque baixo");

  const footerParts = [`Total: ${products.length} produto(s)`];
  if (activeFilters.length) footerParts.push(`Filtros: ${activeFilters.join(" | ")}`);

  doc.text(footerParts.join("   ·   "), 14, finalY);

  doc.save(`hypesoft-produtos-${Date.now()}.pdf`);
}

export function exportCategoriesPdf(categories: Category[]) {
  const doc = new jsPDF();
  addHeader(doc, "Relatório de Categorias");

  const rows = categories.map((c) => [
    c.name,
    c.description ?? "—",
    String(c.productCount),
  ]);

  autoTable(doc, {
    startY: 37,
    head: [["Nome", "Descrição", "Produtos"]],
    body: rows,
    headStyles: {
      fillColor: BRAND_COLOR,
      textColor: 255,
      fontStyle: "bold",
      fontSize: 9,
    },
    bodyStyles: { fontSize: 8.5 },
    alternateRowStyles: { fillColor: [248, 248, 252] },
    columnStyles: { 2: { halign: "center" } },
  });

  const finalY = (doc as jsPDF & { lastAutoTable: { finalY: number } })
    .lastAutoTable.finalY + 6;
  doc.setFontSize(8.5);
  doc.setTextColor(100, 100, 100);
  doc.text(`Total: ${categories.length} categoria(s)`, 14, finalY);

  doc.save(`hypesoft-categorias-${Date.now()}.pdf`);
}

export function exportDashboardPdf(summary: DashboardSummary) {
  const doc = new jsPDF();
  addHeader(doc, "Resumo do Dashboard");

  let currentY = 40;

  // KPIs section
  doc.setFont("helvetica", "bold");
  doc.setFontSize(10);
  doc.setTextColor(50, 50, 50);
  doc.text("Indicadores", 14, currentY);
  currentY += 6;

  doc.setFont("helvetica", "normal");
  doc.setFontSize(9);
  doc.setTextColor(70, 70, 70);
  doc.text(`• Total de Produtos: ${summary.totalProducts}`, 18, currentY);
  currentY += 5;
  doc.text(`• Valor Total do Estoque: ${formatCurrency(summary.totalStockValue)}`, 18, currentY);
  currentY += 5;
  doc.text(`• Produtos com Estoque Baixo: ${summary.lowStockProducts.length}`, 18, currentY);
  currentY += 10;

  // Products by category
  autoTable(doc, {
    startY: currentY,
    head: [["Categoria", "Produtos"]],
    body: summary.productsByCategory.map((c) => [c.categoryName, String(c.productCount)]),
    headStyles: {
      fillColor: BRAND_COLOR,
      textColor: 255,
      fontStyle: "bold",
      fontSize: 9,
    },
    bodyStyles: { fontSize: 8.5 },
    alternateRowStyles: { fillColor: [248, 248, 252] },
    columnStyles: { 1: { halign: "center" } },
    didDrawPage(data) {
      if (data.pageNumber === 1) {
        doc.setFont("helvetica", "bold");
        doc.setFontSize(10);
        doc.setTextColor(50, 50, 50);
        doc.text("Produtos por Categoria", 14, currentY - 3);
      }
    },
  });

  currentY = (doc as jsPDF & { lastAutoTable: { finalY: number } })
    .lastAutoTable.finalY + 12;

  // Low stock products
  if (summary.lowStockProducts.length > 0) {
    doc.setFont("helvetica", "bold");
    doc.setFontSize(10);
    doc.setTextColor(50, 50, 50);
    doc.text("Produtos com Estoque Baixo", 14, currentY);
    currentY += 4;

    autoTable(doc, {
      startY: currentY,
      head: [["Produto", "Categoria", "Estoque"]],
      body: summary.lowStockProducts.map((p) => [
        p.name,
        p.categoryName ?? "—",
        `${p.stockQuantity} un.`,
      ]),
      headStyles: {
        fillColor: [217, 119, 6],
        textColor: 255,
        fontStyle: "bold",
        fontSize: 9,
      },
      bodyStyles: { fontSize: 8.5, textColor: [120, 50, 0] },
      alternateRowStyles: { fillColor: [255, 251, 235] },
      columnStyles: { 2: { halign: "center" } },
    });
  }

  doc.save(`hypesoft-dashboard-${Date.now()}.pdf`);
}

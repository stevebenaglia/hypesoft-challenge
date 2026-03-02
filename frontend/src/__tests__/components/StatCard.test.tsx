import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import StatCard from "@/components/ui/StatCard";
import { Package } from "lucide-react";

describe("StatCard", () => {
  it("renders title and value", () => {
    render(<StatCard title="Total Products" value={42} />);
    expect(screen.getByText("Total Products")).toBeInTheDocument();
    expect(screen.getByText("42")).toBeInTheDocument();
  });

  it("renders subtitle when provided", () => {
    render(<StatCard title="Revenue" value="R$ 1.000,00" subtitle="Last 30 days" />);
    expect(screen.getByText("Last 30 days")).toBeInTheDocument();
  });

  it("does not render subtitle when not provided", () => {
    render(<StatCard title="Stock" value={10} />);
    expect(screen.queryByText("Last 30 days")).not.toBeInTheDocument();
  });

  it("renders icon when provided", () => {
    const { container } = render(
      <StatCard title="Products" value={5} icon={Package} />
    );
    // The icon renders an SVG element
    expect(container.querySelector("svg")).toBeInTheDocument();
  });

  it("does not render icon container when icon is not provided", () => {
    const { container } = render(<StatCard title="No Icon" value={0} />);
    expect(container.querySelector("svg")).not.toBeInTheDocument();
  });
});

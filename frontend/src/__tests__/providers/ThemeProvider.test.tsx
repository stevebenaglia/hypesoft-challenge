import { describe, it, expect, beforeEach, vi } from "vitest";
import { render, screen, act } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ThemeProvider, useTheme } from "@/components/providers/ThemeProvider";

// Helper component that exposes theme state via the UI
function ThemeConsumer() {
  const { theme, toggle } = useTheme();
  return (
    <div>
      <span data-testid="theme">{theme}</span>
      <button onClick={toggle}>Toggle</button>
    </div>
  );
}

describe("ThemeProvider", () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.classList.remove("dark");
    // Reset matchMedia to return "light" preference by default
    Object.defineProperty(window, "matchMedia", {
      writable: true,
      value: vi.fn().mockImplementation((query: string) => ({
        matches: false,
        media: query,
        onchange: null,
        addListener: vi.fn(),
        removeListener: vi.fn(),
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        dispatchEvent: vi.fn(),
      })),
    });
  });

  it("starts with light theme when no stored preference", async () => {
    render(
      <ThemeProvider>
        <ThemeConsumer />
      </ThemeProvider>
    );
    // After initial useEffect runs
    await act(async () => {});
    expect(screen.getByTestId("theme").textContent).toBe("light");
  });

  it("reads stored theme from localStorage", async () => {
    localStorage.setItem("theme", "dark");

    render(
      <ThemeProvider>
        <ThemeConsumer />
      </ThemeProvider>
    );
    await act(async () => {});

    expect(screen.getByTestId("theme").textContent).toBe("dark");
  });

  it("toggles theme from light to dark on button click", async () => {
    const user = userEvent.setup();
    render(
      <ThemeProvider>
        <ThemeConsumer />
      </ThemeProvider>
    );
    await act(async () => {});

    await user.click(screen.getByRole("button", { name: "Toggle" }));

    expect(screen.getByTestId("theme").textContent).toBe("dark");
  });

  it("persists toggled theme to localStorage", async () => {
    const user = userEvent.setup();
    render(
      <ThemeProvider>
        <ThemeConsumer />
      </ThemeProvider>
    );
    await act(async () => {});

    await user.click(screen.getByRole("button", { name: "Toggle" }));

    expect(localStorage.getItem("theme")).toBe("dark");
  });

  it("toggles dark class on document.documentElement", async () => {
    const user = userEvent.setup();
    render(
      <ThemeProvider>
        <ThemeConsumer />
      </ThemeProvider>
    );
    await act(async () => {});

    await user.click(screen.getByRole("button", { name: "Toggle" }));

    expect(document.documentElement.classList.contains("dark")).toBe(true);
  });
});

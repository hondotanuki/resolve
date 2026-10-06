import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it } from "vitest";
import App from "../App";
import "@testing-library/jest-dom/vitest";

describe("App", () => {
  it("Homeを表示する", () => {
    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    );

    expect(screen.getByText("Home")).toBeInTheDocument();
  });
});

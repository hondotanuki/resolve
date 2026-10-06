import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it } from "vitest";
import App from "../App";
import userEvent from "@testing-library/user-event";

describe("App", () => {
  it("Homeを表示する", () => {
    render(
      <MemoryRouter initialEntries={["/"]}>
        <App />
      </MemoryRouter>,
    );

    expect(screen.getByRole("heading", { name: "Home" })).toBeInTheDocument();
  });

  it("Learning Items画面へ遷移できる", async () => {
    const user = userEvent.setup();

    render(
      <MemoryRouter initialEntries={["/"]}>
        <App />
      </MemoryRouter>,
    );

    await user.click(screen.getByRole("link", { name: "Learning Items" }));

    expect(
      screen.getByRole("heading", { name: "Learning Items" }),
    ).toBeInTheDocument();
  });

  it("Learning Itemsを表示する", () => {
    render(
      <MemoryRouter initialEntries={["/learning-items"]}>
        <App />
      </MemoryRouter>,
    );

    expect(
      screen.getByRole("heading", { name: "Learning Items" }),
    ).toBeInTheDocument();
  });
});

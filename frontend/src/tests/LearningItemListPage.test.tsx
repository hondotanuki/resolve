import { screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { useListLearningItems } from "../api/generated";
import { LearningItemListPage } from "../pages/LearningItemListPage";
import { renderWithProviders } from "./test-utils";

vi.mock("../api/generated", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../api/generated")>();

  return {
    ...actual,
    useListLearningItems: vi.fn(),
  };
});

describe("LearningItemListPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("APIから取得したLearningItemを表示する", async () => {
    vi.mocked(useListLearningItems).mockReturnValue({
      data: {
        status: 200,
        headers: new Headers(),
        data: [
          {
            id: 1,
            itemType: "algorithm_problem",
            title: "二分探索",
            archivedAt: null,
          },
        ],
      },
      isPending: false,
      isError: false,
    } as never);

    renderWithProviders(<LearningItemListPage />);

    expect(await screen.findByText("二分探索")).toBeInTheDocument();
    expect(screen.getByText("algorithm_problem")).toBeInTheDocument();
  });

  it("取得中はLoadingを表示する", () => {
    vi.mocked(useListLearningItems).mockReturnValue({
      data: undefined,
      isPending: true,
      isError: false,
    } as never);

    renderWithProviders(<LearningItemListPage />);

    expect(screen.getByText("Loading...")).toBeInTheDocument();
  });

  it("データがなければ空状態を表示する", async () => {
    vi.mocked(useListLearningItems).mockReturnValue({
      data: {
        status: 200,
        headers: new Headers(),
        data: [],
      },
      isPending: false,
      isError: false,
    } as never);

    renderWithProviders(<LearningItemListPage />);

    expect(
      await screen.findByText("LearningItemはまだありません。"),
    ).toBeInTheDocument();
  });

  it("APIエラーを表示する", async () => {
    vi.mocked(useListLearningItems).mockReturnValue({
      data: undefined,
      isPending: false,
      isError: true,
    } as never);

    renderWithProviders(<LearningItemListPage />);

    expect(
      await screen.findByText("LearningItemの取得に失敗しました。"),
    ).toBeInTheDocument();
  });
});

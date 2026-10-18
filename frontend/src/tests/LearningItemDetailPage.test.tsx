import { screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { LearningItemDetailPage } from "../pages/LearningItemDetailPage";
import { useGetLearningItemById } from "../api/generated";
import { renderWithRouter } from "./test-utils";

vi.mock("../api/generated", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../api/generated")>();

  return {
    ...actual,
    useGetLearningItemById: vi.fn(),
  };
});

function renderPage(path = "/learning-items/1") {
  return renderWithRouter(
    [
      {
        path: "/learning-items/:id",
        element: <LearningItemDetailPage />,
      },
    ],
    [path],
  );
}

describe("LearningItemDetailPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("LearningItemの詳細を表示する", async () => {
    vi.mocked(useGetLearningItemById).mockReturnValue({
      data: {
        status: 200,
        headers: new Headers(),
        data: {
          id: 1,
          itemType: "algorithm_problem",
          title: "二分探索",
          content: "二分探索を解く",
          archivedAt: null,
          createdAt: "2026-10-01T10:00:00Z",
          updatedAt: "2026-10-01T10:00:00Z",
        },
      },
      isPending: false,
      isError: false,
    } as never);

    renderPage();

    expect(
      await screen.findByRole("heading", {
        name: "二分探索",
      }),
    ).toBeInTheDocument();
    expect(screen.getByText("algorithm_problem")).toBeInTheDocument();
    expect(screen.getByText("二分探索を解く")).toBeInTheDocument();
  });

  it("取得中はLoadingを表示する", async () => {
    vi.mocked(useGetLearningItemById).mockReturnValue({
      data: undefined,
      isPending: true,
      isError: false,
    } as never);

    renderPage();

    expect(await screen.findByText("Loading...")).toBeInTheDocument();
  });

  it("存在しないIDならNot Foundを表示する", async () => {
    vi.mocked(useGetLearningItemById).mockReturnValue({
      data: {
        status: 404,
        headers: new Headers(),
        data: undefined,
      },
      isPending: false,
      isError: false,
    } as never);

    renderPage("/learning-items/999");

    expect(
      await screen.findByText("LearningItemが見つかりません。"),
    ).toBeInTheDocument();
  });

  it("APIエラーならエラー表示する", async () => {
    vi.mocked(useGetLearningItemById).mockReturnValue({
      data: undefined,
      isPending: false,
      isError: true,
    } as never);

    renderPage();

    expect(
      await screen.findByText("LearningItemの取得に失敗しました。"),
    ).toBeInTheDocument();
  });

  it("IDが整数でなければNot Foundを表示する", async () => {
    vi.mocked(useGetLearningItemById).mockReturnValue({
      data: undefined,
      isPending: false,
      isError: false,
    } as never);

    renderPage("/learning-items/abc");

    expect(
      await screen.findByText("LearningItemが見つかりません。"),
    ).toBeInTheDocument();
  });
});

import { screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { LearningItemDetailPage } from "../pages/LearningItemDetailPage";
import { useGetLearningItemById, useListStudyRecords } from "../api/generated";
import { renderWithRouter } from "./test-utils";

vi.mock("../api/generated", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../api/generated")>();

  return {
    ...actual,
    useGetLearningItemById: vi.fn(),
    useListStudyRecords: vi.fn(),
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
    vi.mocked(useListStudyRecords).mockReturnValue({
      data: { status: 200, headers: new Headers(), data: [] },
      isPending: false,
      isError: false,
    } as never);
  });

  it("LearningItemの詳細を表示する", async () => {
    vi.mocked(useGetLearningItemById).mockReturnValue({
      data: {
        status: 200,
        headers: new Headers(),
        data: {
          id: 1,

          source: "AtCoder",
          category: "Beginner",
          problemIndex: "A",
          problemUrl: "https://atcoder.jp/contests/abc482/tasks/abc482_a",
          contestId: "abc482",
          contestName: "ABC 482",
          contestStartDate: "2026-10-31",
          targetSeconds: 600,
          title: "二分探索",
          status: "unstarted",
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
    expect(screen.getByText("AtCoder / Beginner / A")).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: /https:\/\/atcoder.jp/ }),
    ).toHaveAttribute(
      "href",
      "https://atcoder.jp/contests/abc482/tasks/abc482_a",
    );
    expect(screen.getByText("ABC 482 (abc482)")).toBeInTheDocument();
    expect(screen.getByText("2026-10-31")).toBeInTheDocument();
    expect(screen.getByText("10分")).toBeInTheDocument();
    expect(screen.getByText("現在の状態: unstarted")).toBeInTheDocument();
    expect(screen.getByText("学習履歴はまだありません。")).toBeInTheDocument();
    expect(vi.mocked(useListStudyRecords).mock.calls[0][0]).toBe(1);
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

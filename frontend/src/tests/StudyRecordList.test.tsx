import { screen, within } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { useListStudyRecords } from "../api/generated";
import { StudyRecordList } from "../components/study-records/StudyRecordList";
import { renderWithProviders } from "./test-utils";

vi.mock("../api/generated", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../api/generated")>();
  return { ...actual, useListStudyRecords: vi.fn() };
});

describe("StudyRecordList", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("履歴をAPIの順に表示し未入力と0秒を区別する", () => {
    const studiedAt = "2026-10-15T09:00:00Z";
    vi.mocked(useListStudyRecords).mockReturnValue({
      data: {
        status: 200,
        headers: new Headers(),
        data: [
          {
            id: 2,
            learningItemId: 42,
            result: "success",
            studiedAt,
            durationSeconds: 0,
            memo: "復習した\n次も確認",
          },
          {
            id: 1,
            learningItemId: 42,
            result: "failure",
            studiedAt,
            durationSeconds: null,
            memo: null,
          },
        ],
      },
      isPending: false,
      isError: false,
    } as never);

    renderWithProviders(<StudyRecordList learningItemId={42} />);

    expect(vi.mocked(useListStudyRecords).mock.calls[0][0]).toBe(42);
    const items = screen.getAllByRole("listitem");
    expect(items).toHaveLength(2);
    expect(within(items[0]).getByText("success")).toBeInTheDocument();
    expect(within(items[0]).getByText("0 seconds")).toBeInTheDocument();
    expect(within(items[0]).getByText(/復習した/)).toHaveTextContent(
      "次も確認",
    );
    expect(
      within(items[0]).getByText(new Date(studiedAt).toLocaleString()),
    ).toBeInTheDocument();
    expect(within(items[1]).getByText("failure")).toBeInTheDocument();
    expect(within(items[1]).getAllByText("—")).toHaveLength(2);
  });

  it("履歴がない場合は空状態を表示する", () => {
    vi.mocked(useListStudyRecords).mockReturnValue({
      data: { status: 200, headers: new Headers(), data: [] },
      isPending: false,
      isError: false,
    } as never);

    renderWithProviders(<StudyRecordList learningItemId={42} />);
    expect(screen.getByText("学習履歴はまだありません。")).toBeInTheDocument();
  });

  it("取得中は読み込み中を表示する", () => {
    vi.mocked(useListStudyRecords).mockReturnValue({
      data: undefined,
      isPending: true,
      isError: false,
    } as never);

    renderWithProviders(<StudyRecordList learningItemId={42} />);
    expect(screen.getByText("学習履歴を読み込み中...")).toBeInTheDocument();
  });

  it.each(["network", "404", "500"])("%sエラーを表示する", (errorType) => {
    vi.mocked(useListStudyRecords).mockReturnValue({
      data:
        errorType === "network"
          ? undefined
          : { status: Number(errorType), headers: new Headers(), data: {} },
      isPending: false,
      isError: errorType === "network",
    } as never);

    renderWithProviders(<StudyRecordList learningItemId={42} />);
    expect(screen.getByRole("alert")).toHaveTextContent(
      "学習履歴の取得に失敗しました。",
    );
  });
});

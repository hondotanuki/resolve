import { screen, within } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  getGetLearningItemByIdQueryKey,
  getListStudyRecordsQueryKey,
  useDeleteStudyRecord,
  useListStudyRecords,
} from "../api/generated";
import { StudyRecordList } from "../components/study-records/StudyRecordList";
import { renderWithProviders } from "./test-utils";

vi.mock("../api/generated", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../api/generated")>();
  return {
    ...actual,
    useListStudyRecords: vi.fn(),
    useDeleteStudyRecord: vi.fn(),
  };
});

describe("StudyRecordList", () => {
  afterEach(() => vi.restoreAllMocks());
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useDeleteStudyRecord).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as never);
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
      within(items[0]).getByText(
        `学習日時: ${new Date(studiedAt).toLocaleString()}`,
      ),
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

  describe("学習記録の削除", () => {
    beforeEach(() => {
      vi.mocked(useListStudyRecords).mockReturnValue({
        data: {
          status: 200,
          headers: new Headers(),
          data: [
            {
              id: 7,
              learningItemId: 42,
              result: "success",
              studiedAt: "2026-10-15T09:00:00Z",
              durationSeconds: 60,
              memo: "記録",
            },
          ],
        },
        isPending: false,
        isError: false,
      } as never);
    });

    it("確認をキャンセルすると削除せずキャッシュも無効化しない", async () => {
      const confirm = vi.spyOn(window, "confirm").mockReturnValue(false);
      const mutateAsync = vi.fn();
      vi.mocked(useDeleteStudyRecord).mockReturnValue({
        mutateAsync,
        isPending: false,
      } as never);
      const { user, queryClient } = renderWithProviders(
        <StudyRecordList learningItemId={42} />,
      );
      const invalidate = vi.spyOn(queryClient, "invalidateQueries");

      await user.click(screen.getByRole("button", { name: "削除" }));

      expect(confirm).toHaveBeenCalledOnce();
      expect(mutateAsync).not.toHaveBeenCalled();
      expect(invalidate).not.toHaveBeenCalled();
      confirm.mockRestore();
    });

    it("削除成功後に対象の履歴と現在状態を再取得する", async () => {
      const confirm = vi.spyOn(window, "confirm").mockReturnValue(true);
      const mutateAsync = vi.fn().mockResolvedValue({ status: 204 });
      vi.mocked(useDeleteStudyRecord).mockReturnValue({
        mutateAsync,
        isPending: false,
      } as never);
      const { user, queryClient } = renderWithProviders(
        <StudyRecordList learningItemId={42} />,
      );
      const invalidate = vi.spyOn(queryClient, "invalidateQueries");

      await user.click(screen.getByRole("button", { name: "削除" }));

      expect(mutateAsync).toHaveBeenCalledWith({ studyRecordId: 7 });
      expect(invalidate).toHaveBeenCalledTimes(2);
      expect(invalidate).toHaveBeenCalledWith({
        queryKey: getListStudyRecordsQueryKey(42),
      });
      expect(invalidate).toHaveBeenCalledWith({
        queryKey: getGetLearningItemByIdQueryKey(42),
      });
      confirm.mockRestore();
    });

    it.each([
      {
        name: "対象が存在しない",
        response: { status: 404 },
        message: "削除対象が存在しません",
      },
      {
        name: "サーバーエラー",
        response: { status: 500 },
        message: "削除に失敗しました",
      },
    ])(
      "$name: エラーを表示しキャッシュを無効化しない",
      async ({ response, message }) => {
        const confirm = vi.spyOn(window, "confirm").mockReturnValue(true);
        const mutateAsync = vi.fn().mockResolvedValue(response);
        vi.mocked(useDeleteStudyRecord).mockReturnValue({
          mutateAsync,
          isPending: false,
        } as never);
        const { user, queryClient } = renderWithProviders(
          <StudyRecordList learningItemId={42} />,
        );
        const invalidate = vi.spyOn(queryClient, "invalidateQueries");

        await user.click(screen.getByRole("button", { name: "削除" }));

        expect(await screen.findByRole("alert")).toHaveTextContent(message);
        expect(invalidate).not.toHaveBeenCalled();
        confirm.mockRestore();
      },
    );

    it("通信に失敗した場合はエラーを表示しキャッシュを無効化しない", async () => {
      const confirm = vi.spyOn(window, "confirm").mockReturnValue(true);
      const mutateAsync = vi.fn().mockRejectedValue(new Error("network error"));
      vi.mocked(useDeleteStudyRecord).mockReturnValue({
        mutateAsync,
        isPending: false,
      } as never);
      const { user, queryClient } = renderWithProviders(
        <StudyRecordList learningItemId={42} />,
      );
      const invalidate = vi.spyOn(queryClient, "invalidateQueries");

      await user.click(screen.getByRole("button", { name: "削除" }));

      expect(await screen.findByRole("alert")).toHaveTextContent(
        "削除処理中にエラーが発生しました。",
      );
      expect(invalidate).not.toHaveBeenCalled();
      confirm.mockRestore();
    });

    it("削除中はボタンを無効化する", () => {
      vi.mocked(useDeleteStudyRecord).mockReturnValue({
        mutateAsync: vi.fn(),
        isPending: true,
      } as never);
      renderWithProviders(<StudyRecordList learningItemId={42} />);

      expect(screen.getByRole("button", { name: "処理中..." })).toBeDisabled();
    });
  });
});

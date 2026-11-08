import { useQuery } from "@tanstack/react-query";
import { fireEvent, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import {
  getGetLearningItemByIdQueryKey,
  getListStudyRecordsQueryKey,
  useUpdateStudyRecord,
} from "../api/generated";
import type { ListStudyRecordsResponse } from "../api/generated/models";
import { StudyRecordEditForm } from "../components/study-records/StudyRecordEditForm";
import { renderWithProviders } from "./test-utils";

vi.mock("../api/generated", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../api/generated")>();
  return { ...actual, useUpdateStudyRecord: vi.fn() };
});

const record: ListStudyRecordsResponse = {
  id: 7,
  learningItemId: 42,
  result: "success",
  studiedAt: "2026-10-15T09:00:37.123Z",
  durationSeconds: 60,
  targetSeconds: 600,
  memo: "元のメモ",
};

describe("StudyRecordEditForm", () => {
  const mutateAsync = vi.fn();
  const onClose = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useUpdateStudyRecord).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as never);
  });

  it("既存値を表示し、キャンセルでは更新しない", async () => {
    const { user } = renderWithProviders(
      <StudyRecordEditForm record={record} onClose={onClose} />,
    );
    expect(screen.getByLabelText("学習結果")).toHaveValue("success");
    expect(screen.getByLabelText("所要時間（秒）")).toHaveValue(60);
    expect(screen.getByLabelText("メモ")).toHaveValue("元のメモ");
    const local = new Date(record.studiedAt);
    const pad = (n: number) => String(n).padStart(2, "0");
    expect(screen.getByLabelText("学習日時")).toHaveValue(
      `${local.getFullYear()}-${pad(local.getMonth() + 1)}-${pad(local.getDate())}T${pad(local.getHours())}:${pad(local.getMinutes())}`,
    );
    await user.click(screen.getByRole("button", { name: "キャンセル" }));
    expect(onClose).toHaveBeenCalledOnce();
    expect(mutateAsync).not.toHaveBeenCalled();
  });

  it("変更がない場合はAPIを呼ばない", async () => {
    const { user } = renderWithProviders(
      <StudyRecordEditForm record={record} onClose={onClose} />,
    );
    await user.click(screen.getByRole("button", { name: "保存" }));
    expect(screen.getByRole("status")).toHaveTextContent("変更はありません。");
    expect(mutateAsync).not.toHaveBeenCalled();
    expect(onClose).not.toHaveBeenCalled();
  });

  it("変更された結果だけを送り、未変更の日時を含めない", async () => {
    mutateAsync.mockResolvedValue({ status: 200 });
    const { user, queryClient } = renderWithProviders(
      <StudyRecordEditForm record={record} onClose={onClose} />,
    );
    const invalidate = vi.spyOn(queryClient, "invalidateQueries");
    await user.selectOptions(screen.getByLabelText("学習結果"), "failure");
    await user.click(screen.getByRole("button", { name: "保存" }));
    expect(mutateAsync).toHaveBeenCalledWith({
      studyRecordId: 7,
      data: { result: "failure" },
    });
    expect(invalidate).toHaveBeenCalledWith({
      queryKey: getListStudyRecordsQueryKey(42),
    });
    expect(invalidate).toHaveBeenCalledWith({
      queryKey: getGetLearningItemByIdQueryKey(42),
    });
    expect(onClose).toHaveBeenCalledOnce();
  });

  it("変更した日時をUTCに変換し、任意項目の消去はnullで送る", async () => {
    mutateAsync.mockResolvedValue({ status: 200 });
    const { user } = renderWithProviders(
      <StudyRecordEditForm record={record} onClose={onClose} />,
    );
    fireEvent.change(screen.getByLabelText("学習日時"), {
      target: { value: "2026-11-03T10:30" },
    });
    await user.clear(screen.getByLabelText("メモ"));
    await user.click(screen.getByRole("button", { name: "保存" }));
    expect(mutateAsync).toHaveBeenCalledWith({
      studyRecordId: 7,
      data: {
        studiedAt: new Date("2026-11-03T10:30").toISOString(),
        memo: null,
      },
    });
  });

  it("日時が空なら入力エラーを表示する", async () => {
    const { user } = renderWithProviders(
      <StudyRecordEditForm record={record} onClose={onClose} />,
    );
    fireEvent.change(screen.getByLabelText("学習日時"), {
      target: { value: "" },
    });
    await user.click(screen.getByRole("button", { name: "保存" }));
    expect(screen.getByRole("alert")).toHaveTextContent(
      "学習日時を入力してください。",
    );
    expect(screen.getByLabelText("学習日時")).toHaveAttribute(
      "aria-invalid",
      "true",
    );
    expect(mutateAsync).not.toHaveBeenCalled();
  });

  it.each(["", "-1", "1.5", "2147483648"])(
    "所要時間が%sなら入力エラーを表示する",
    async (value) => {
      const { user } = renderWithProviders(
        <StudyRecordEditForm record={record} onClose={onClose} />,
      );
      fireEvent.change(screen.getByLabelText("所要時間（秒）"), {
        target: { value },
      });
      await user.click(screen.getByRole("button", { name: "保存" }));
      expect(screen.getByRole("alert")).toHaveTextContent(
        "0以上の整数を入力してください。",
      );
      expect(mutateAsync).not.toHaveBeenCalled();
    },
  );

  it.each([
    { status: 400, message: "入力内容を確認してください。" },
    { status: 404, message: "対象の学習記録が見つかりません。" },
    { status: 500, message: "更新に失敗しました。" },
  ])(
    "HTTP $status では入力を残してエラーを表示する",
    async ({ status, message }) => {
      mutateAsync.mockResolvedValue({ status });
      const { user, queryClient } = renderWithProviders(
        <StudyRecordEditForm record={record} onClose={onClose} />,
      );
      const invalidate = vi.spyOn(queryClient, "invalidateQueries");
      await user.selectOptions(screen.getByLabelText("学習結果"), "failure");
      await user.click(screen.getByRole("button", { name: "保存" }));
      expect(await screen.findByRole("alert")).toHaveTextContent(message);
      expect(screen.getByLabelText("学習結果")).toHaveValue("failure");
      expect(invalidate).not.toHaveBeenCalled();
      expect(onClose).not.toHaveBeenCalled();
    },
  );

  it("通信失敗時はフォームを閉じない", async () => {
    mutateAsync.mockRejectedValue(new Error("network error"));
    const { user } = renderWithProviders(
      <StudyRecordEditForm record={record} onClose={onClose} />,
    );
    await user.selectOptions(screen.getByLabelText("学習結果"), "failure");
    await user.click(screen.getByRole("button", { name: "保存" }));
    expect(await screen.findByRole("alert")).toHaveTextContent(
      "通信エラーが発生しました。",
    );
    expect(onClose).not.toHaveBeenCalled();
  });

  it("保存中は入力と操作を無効化する", () => {
    vi.mocked(useUpdateStudyRecord).mockReturnValue({
      mutateAsync,
      isPending: true,
    } as never);
    renderWithProviders(
      <StudyRecordEditForm record={record} onClose={onClose} />,
    );
    expect(screen.getByRole("button", { name: "保存中..." })).toBeDisabled();
    expect(screen.getByRole("button", { name: "キャンセル" })).toBeDisabled();
    expect(screen.getByLabelText("メモ")).toBeDisabled();
  });

  it("保存後に表示中の履歴と現在状態を再取得する", async () => {
    mutateAsync.mockResolvedValue({ status: 200 });
    const fetchHistory = vi
      .fn()
      .mockResolvedValueOnce("更新前の履歴")
      .mockResolvedValue("更新後の履歴");
    const fetchDetail = vi
      .fn()
      .mockResolvedValueOnce("completed")
      .mockResolvedValue("needs_review");
    function ActiveQueries() {
      const history = useQuery({
        queryKey: getListStudyRecordsQueryKey(42),
        queryFn: fetchHistory,
      });
      const detail = useQuery({
        queryKey: getGetLearningItemByIdQueryKey(42),
        queryFn: fetchDetail,
      });
      return (
        <>
          <p>{history.data}</p>
          <p>状態: {detail.data}</p>
          <StudyRecordEditForm record={record} onClose={onClose} />
        </>
      );
    }
    const { user } = renderWithProviders(<ActiveQueries />);
    expect(await screen.findByText("更新前の履歴")).toBeInTheDocument();
    expect(await screen.findByText("状態: completed")).toBeInTheDocument();
    await user.selectOptions(screen.getByLabelText("学習結果"), "failure");
    await user.click(screen.getByRole("button", { name: "保存" }));
    expect(await screen.findByText("更新後の履歴")).toBeInTheDocument();
    expect(await screen.findByText("状態: needs_review")).toBeInTheDocument();
  });
});

import { useQuery } from "@tanstack/react-query";
import { fireEvent, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import {
  getGetLearningItemByIdQueryKey,
  getListStudyRecordsQueryKey,
  useCreateStudyRecord,
} from "../api/generated";
import { StudyRecordCreateForm } from "../components/study-records/StudyRecordCreateForm";
import { renderWithProviders } from "./test-utils";

vi.mock("../api/generated", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../api/generated")>();

  return { ...actual, useCreateStudyRecord: vi.fn() };
});

describe("StudyRecordCreateForm", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("正常な入力ならLearningItem IDと変換済みの値を送信する", async () => {
    const mutateAsync = vi.fn().mockResolvedValue({
      status: 201,
      headers: new Headers(),
      data: {
        id: 1,
        learningItemId: 42,
        result: "success",
        studiedAt: "2026-11-03T01:30:00.000Z",
        durationSeconds: 300,
        memo: "復習した",
        createdAt: "2026-11-03T01:30:00.000Z",
        updatedAt: "2026-11-03T01:30:00.000Z",
        status: "completed",
      },
    });

    vi.mocked(useCreateStudyRecord).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as never);

    const { user, queryClient } = renderWithProviders(
      <StudyRecordCreateForm learningItemId={42} />,
    );
    const invalidateQueries = vi.spyOn(queryClient, "invalidateQueries");

    await user.selectOptions(screen.getByLabelText("Result"), "failure");
    fireEvent.change(screen.getByLabelText("Studied At"), {
      target: { value: "2026-11-03T10:30" },
    });
    await user.type(screen.getByLabelText("Duration (seconds)"), "300");
    await user.type(screen.getByLabelText("Memo"), "  復習した  ");
    await user.click(screen.getByRole("button", { name: "Create" }));

    expect(mutateAsync).toHaveBeenCalledWith({
      learningItemId: 42,
      data: {
        result: "failure",
        studiedAt: new Date("2026-11-03T10:30").toISOString(),
        durationSeconds: 300,
        memo: "復習した",
      },
    });
    expect(await screen.findByRole("status")).toHaveTextContent(
      "学習記録を登録しました。",
    );
    expect(invalidateQueries).toHaveBeenCalledTimes(2);
    expect(invalidateQueries).toHaveBeenCalledWith({
      queryKey: getGetLearningItemByIdQueryKey(42),
    });
    expect(invalidateQueries).toHaveBeenCalledWith({
      queryKey: getListStudyRecordsQueryKey(42),
    });
  });

  it("登録後に表示中の詳細と履歴が再取得される", async () => {
    const mutateAsync = vi.fn().mockResolvedValue({
      status: 201,
      headers: new Headers(),
      data: {
        id: 1,
        learningItemId: 42,
        result: "failure",
        studiedAt: "2026-11-03T01:30:00.000Z",
        durationSeconds: null,
        memo: null,
        createdAt: "2026-11-03T01:30:00.000Z",
        updatedAt: "2026-11-03T01:30:00.000Z",
        status: "needs_review",
      },
    });

    vi.mocked(useCreateStudyRecord).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as never);

    const fetchDetail = vi
      .fn()
      .mockResolvedValueOnce("unstarted")
      .mockResolvedValue("needs_review");
    const fetchHistory = vi
      .fn()
      .mockResolvedValueOnce("履歴なし")
      .mockResolvedValue("履歴あり");

    function ActiveQueries() {
      const detail = useQuery({
        queryKey: getGetLearningItemByIdQueryKey(42),
        queryFn: fetchDetail,
      });
      const history = useQuery({
        queryKey: getListStudyRecordsQueryKey(42),
        queryFn: fetchHistory,
      });
      return (
        <>
          <p>現在の状態: {detail.data}</p>
          <p>{history.data}</p>
          <StudyRecordCreateForm learningItemId={42} />
        </>
      );
    }

    const { user } = renderWithProviders(<ActiveQueries />);
    expect(
      await screen.findByText("現在の状態: unstarted"),
    ).toBeInTheDocument();
    expect(await screen.findByText("履歴なし")).toBeInTheDocument();

    await user.selectOptions(screen.getByLabelText("Result"), "failure");
    fireEvent.change(screen.getByLabelText("Studied At"), {
      target: { value: "2026-11-03T10:30" },
    });
    await user.click(screen.getByRole("button", { name: "Create" }));

    expect(
      await screen.findByText("現在の状態: needs_review"),
    ).toBeInTheDocument();
    expect(await screen.findByText("履歴あり")).toBeInTheDocument();
    expect(fetchDetail).toHaveBeenCalledTimes(2);
    expect(fetchHistory).toHaveBeenCalledTimes(2);
  });

  it("日時が未入力ならAPIを呼ばずにエラーを表示する", async () => {
    const mutateAsync = vi.fn();

    vi.mocked(useCreateStudyRecord).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as never);

    const { user, queryClient } = renderWithProviders(
      <StudyRecordCreateForm learningItemId={42} />,
    );
    const invalidateQueries = vi.spyOn(queryClient, "invalidateQueries");

    await user.click(screen.getByRole("button", { name: "Create" }));

    const input = screen.getByLabelText("Studied At");
    expect(mutateAsync).not.toHaveBeenCalled();
    expect(invalidateQueries).not.toHaveBeenCalled();
    expect(screen.getByRole("alert")).toHaveTextContent(
      "学習日時を入力してください。",
    );
    expect(input).toHaveAttribute("aria-invalid", "true");
    expect(input).toHaveAttribute("aria-describedby", "studiedAt-error");
  });

  it("秒数が負数ならAPIを呼ばずにエラーを表示する", async () => {
    const mutateAsync = vi.fn();

    vi.mocked(useCreateStudyRecord).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as never);

    const { user } = renderWithProviders(
      <StudyRecordCreateForm learningItemId={42} />,
    );

    fireEvent.change(screen.getByLabelText("Studied At"), {
      target: { value: "2026-11-03T10:30" },
    });
    fireEvent.change(screen.getByLabelText("Duration (seconds)"), {
      target: { value: "-1" },
    });
    await user.click(screen.getByRole("button", { name: "Create" }));

    const input = screen.getByLabelText("Duration (seconds)");
    expect(mutateAsync).not.toHaveBeenCalled();
    expect(screen.getByRole("alert")).toHaveTextContent(
      "学習時間は0以上の整数で入力してください。",
    );
    expect(input).toHaveAttribute("aria-invalid", "true");
    expect(input).toHaveAttribute("aria-describedby", "durationSeconds-error");
  });

  it("任意項目が未入力ならnullとして送信する", async () => {
    const mutateAsync = vi.fn().mockResolvedValue({
      status: 201,
      headers: new Headers(),
      data: {},
    });

    vi.mocked(useCreateStudyRecord).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as never);

    const { user } = renderWithProviders(
      <StudyRecordCreateForm learningItemId={42} />,
    );

    fireEvent.change(screen.getByLabelText("Studied At"), {
      target: { value: "2026-11-03T10:30" },
    });
    await user.click(screen.getByRole("button", { name: "Create" }));

    expect(mutateAsync).toHaveBeenCalledWith({
      learningItemId: 42,
      data: {
        result: "success",
        studiedAt: new Date("2026-11-03T10:30").toISOString(),
        durationSeconds: null,
        memo: null,
      },
    });
  });

  it("対象LearningItemが見つからなければAPIエラーを表示する", async () => {
    const mutateAsync = vi.fn().mockResolvedValue({
      status: 404,
      headers: new Headers(),
      data: undefined,
    });

    vi.mocked(useCreateStudyRecord).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as never);

    const { user, queryClient } = renderWithProviders(
      <StudyRecordCreateForm learningItemId={42} />,
    );
    const invalidateQueries = vi.spyOn(queryClient, "invalidateQueries");

    fireEvent.change(screen.getByLabelText("Studied At"), {
      target: { value: "2026-11-03T10:30" },
    });
    await user.click(screen.getByRole("button", { name: "Create" }));

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "対象のLearningItemが見つかりません。",
    );
    expect(invalidateQueries).not.toHaveBeenCalled();
  });

  it("400の場合はキャッシュを無効化せず入力エラーを表示する", async () => {
    vi.mocked(useCreateStudyRecord).mockReturnValue({
      mutateAsync: vi.fn().mockResolvedValue({ status: 400 }),
      isPending: false,
    } as never);

    const { user, queryClient } = renderWithProviders(
      <StudyRecordCreateForm learningItemId={42} />,
    );
    const invalidateQueries = vi.spyOn(queryClient, "invalidateQueries");
    fireEvent.change(screen.getByLabelText("Studied At"), {
      target: { value: "2026-11-03T10:30" },
    });
    await user.click(screen.getByRole("button", { name: "Create" }));

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "入力内容を確認してください。",
    );
    expect(invalidateQueries).not.toHaveBeenCalled();
  });

  it("登録中はボタンを無効化する", () => {
    vi.mocked(useCreateStudyRecord).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: true,
    } as never);

    renderWithProviders(<StudyRecordCreateForm learningItemId={42} />);

    expect(screen.getByRole("button", { name: "Saving..." })).toBeDisabled();
  });
});

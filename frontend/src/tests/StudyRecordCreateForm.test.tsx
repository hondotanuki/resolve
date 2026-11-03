import { fireEvent, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { useCreateStudyRecord } from "../api/generated";
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

    const { user } = renderWithProviders(
      <StudyRecordCreateForm learningItemId={42} />,
    );

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
    expect(screen.getByRole("status")).toHaveTextContent(
      "現在の状態: Completed",
    );
  });

  it("Failureの登録後はNeeds Reviewを表示する", async () => {
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

    const { user } = renderWithProviders(
      <StudyRecordCreateForm learningItemId={42} />,
    );

    await user.selectOptions(screen.getByLabelText("Result"), "failure");
    fireEvent.change(screen.getByLabelText("Studied At"), {
      target: { value: "2026-11-03T10:30" },
    });
    await user.click(screen.getByRole("button", { name: "Create" }));

    expect(await screen.findByRole("status")).toHaveTextContent(
      "現在の状態: Needs Review",
    );
  });

  it("日時が未入力ならAPIを呼ばずにエラーを表示する", async () => {
    const mutateAsync = vi.fn();

    vi.mocked(useCreateStudyRecord).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as never);

    const { user } = renderWithProviders(
      <StudyRecordCreateForm learningItemId={42} />,
    );

    await user.click(screen.getByRole("button", { name: "Create" }));

    const input = screen.getByLabelText("Studied At");
    expect(mutateAsync).not.toHaveBeenCalled();
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

    const { user } = renderWithProviders(
      <StudyRecordCreateForm learningItemId={42} />,
    );

    fireEvent.change(screen.getByLabelText("Studied At"), {
      target: { value: "2026-11-03T10:30" },
    });
    await user.click(screen.getByRole("button", { name: "Create" }));

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "対象のLearningItemが見つかりません。",
    );
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

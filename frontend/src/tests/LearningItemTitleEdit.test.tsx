import { screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import {
  getGetLearningItemByIdQueryKey,
  getListLearningItemsQueryKey,
  useUpdateLearningItemTitle,
} from "../api/generated";
import { LearningItemTitleEdit } from "../components/learning-items/LearningItemTitleEdit";
import { renderWithProviders } from "./test-utils";

vi.mock("../api/generated", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../api/generated")>();
  return { ...actual, useUpdateLearningItemTitle: vi.fn() };
});

describe("タイトル訂正", () => {
  beforeEach(() => vi.clearAllMocks());
  it("保存後に詳細と一覧を再取得する", async () => {
    const mutateAsync = vi.fn().mockResolvedValue({ status: 204 });
    vi.mocked(useUpdateLearningItemTitle).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as never);
    const { user, queryClient } = renderWithProviders(
      <LearningItemTitleEdit id={1} title="元のタイトル" />,
    );
    const invalidate = vi.spyOn(queryClient, "invalidateQueries");
    await user.click(screen.getByRole("button", { name: "タイトルを訂正" }));
    await user.clear(screen.getByLabelText("タイトル"));
    await user.type(screen.getByLabelText("タイトル"), "新しいタイトル");
    await user.click(screen.getByRole("button", { name: "保存" }));
    expect(mutateAsync).toHaveBeenCalledWith({
      id: 1,
      data: { title: "新しいタイトル" },
    });
    expect(invalidate).toHaveBeenCalledWith({
      queryKey: getGetLearningItemByIdQueryKey(1),
    });
    expect(invalidate).toHaveBeenCalledWith({
      queryKey: getListLearningItemsQueryKey(),
    });
    expect(
      await screen.findByRole("button", { name: "タイトルを訂正" }),
    ).toBeInTheDocument();
  });
  it("空のタイトルは送信しない", async () => {
    const mutateAsync = vi.fn();
    vi.mocked(useUpdateLearningItemTitle).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as never);
    const { user } = renderWithProviders(
      <LearningItemTitleEdit id={1} title="元のタイトル" />,
    );
    await user.click(screen.getByRole("button", { name: "タイトルを訂正" }));
    await user.clear(screen.getByLabelText("タイトル"));
    await user.click(screen.getByRole("button", { name: "保存" }));
    expect(screen.getByRole("alert")).toHaveTextContent(
      "タイトルを入力してください。",
    );
    expect(mutateAsync).not.toHaveBeenCalled();
  });
  it("存在しない問題のエラーを表示する", async () => {
    vi.mocked(useUpdateLearningItemTitle).mockReturnValue({
      mutateAsync: vi.fn().mockResolvedValue({ status: 404 }),
      isPending: false,
    } as never);
    const { user } = renderWithProviders(
      <LearningItemTitleEdit id={1} title="元のタイトル" />,
    );
    await user.click(screen.getByRole("button", { name: "タイトルを訂正" }));
    await user.click(screen.getByRole("button", { name: "保存" }));
    expect(await screen.findByRole("alert")).toHaveTextContent(
      "対象の問題が見つかりません。",
    );
  });
});

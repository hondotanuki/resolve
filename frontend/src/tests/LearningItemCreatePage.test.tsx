import { screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { LearningItemCreatePage } from "../pages/LearningItemCreatePage";
import {
  getListLearningItemsQueryKey,
  useCreateLearningItem,
} from "../api/generated";
import { renderWithProviders, renderWithRouter } from "./test-utils";

vi.mock("../api/generated", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../api/generated")>();
  return { ...actual, useCreateLearningItem: vi.fn() };
});

const url = "https://atcoder.jp/contests/abc482/tasks/abc482_a";
describe("問題登録", () => {
  beforeEach(() => vi.clearAllMocks());

  it.each([200, 201])("%iなら詳細へ遷移する", async (status) => {
    const mutateAsync = vi
      .fn()
      .mockResolvedValue({ status, data: { id: 1 }, headers: new Headers() });
    vi.mocked(useCreateLearningItem).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as never);
    const { user, queryClient } = renderWithRouter(
      [
        { path: "/learning-items/new", element: <LearningItemCreatePage /> },
        { path: "/learning-items/:id", element: <div>詳細画面</div> },
      ],
      ["/learning-items/new"],
    );
    const invalidate = vi.spyOn(queryClient, "invalidateQueries");
    await user.type(screen.getByLabelText("問題URL"), url);
    await user.click(screen.getByRole("button", { name: "登録" }));
    expect(mutateAsync).toHaveBeenCalledWith({ data: { url } });
    expect(invalidate).toHaveBeenCalledWith({
      queryKey: getListLearningItemsQueryKey(),
    });
    expect(await screen.findByText("詳細画面")).toBeInTheDocument();
  });

  it("URL未入力なら送信しない", async () => {
    const mutateAsync = vi.fn();
    vi.mocked(useCreateLearningItem).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as never);
    const { user } = renderWithProviders(<LearningItemCreatePage />);
    await user.click(screen.getByRole("button", { name: "登録" }));
    expect(screen.getByRole("alert")).toHaveTextContent(
      "問題URLを入力してください。",
    );
    expect(mutateAsync).not.toHaveBeenCalled();
  });

  it.each([400, 422, 502, 504])("%iのエラーを表示する", async (status) => {
    vi.mocked(useCreateLearningItem).mockReturnValue({
      mutateAsync: vi.fn().mockResolvedValue({
        status,
        data: { title: "取得できませんでした。" },
      }),
      isPending: false,
    } as never);
    const { user } = renderWithProviders(<LearningItemCreatePage />);
    await user.type(screen.getByLabelText("問題URL"), url);
    await user.click(screen.getByRole("button", { name: "登録" }));
    expect(await screen.findByRole("alert")).toHaveTextContent(
      "取得できませんでした。",
    );
  });

  it("取得中はボタンを無効化する", () => {
    vi.mocked(useCreateLearningItem).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: true,
    } as never);
    renderWithProviders(<LearningItemCreatePage />);
    expect(screen.getByRole("button", { name: "取得中..." })).toBeDisabled();
  });

  it("通信失敗を表示する", async () => {
    vi.mocked(useCreateLearningItem).mockReturnValue({
      mutateAsync: vi.fn().mockRejectedValue(new Error("失敗")),
      isPending: false,
    } as never);
    const { user } = renderWithProviders(<LearningItemCreatePage />);
    await user.type(screen.getByLabelText("問題URL"), url);
    await user.click(screen.getByRole("button", { name: "登録" }));
    expect(await screen.findByRole("alert")).toHaveTextContent(
      "通信に失敗しました。",
    );
  });
});

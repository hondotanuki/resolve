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

describe("CreateLearningItemPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("正常な入力なら登録して一覧へ遷移する", async () => {
    const mutateAsync = vi.fn().mockResolvedValue({
      status: 201,
      headers: new Headers(),
      data: {
        id: 1,
        itemType: "english_word",
        title: "apple",
        archivedAt: null,
      },
    });

    vi.mocked(useCreateLearningItem).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as never);

    const { user, queryClient } = renderWithRouter(
      [
        {
          path: "/learning-items/new",
          element: <LearningItemCreatePage />,
        },
        {
          path: "/learning-items",
          element: <div>一覧画面</div>,
        },
      ],
      ["/learning-items/new"],
    );
    const invalidateQueriesSpy = vi.spyOn(queryClient, "invalidateQueries");

    await user.selectOptions(
      screen.getByLabelText("Item Type"),
      "english_word",
    );
    await user.type(screen.getByLabelText("Title"), "  apple  ");
    await user.type(screen.getByLabelText("Content"), "  りんご  ");
    await user.click(
      screen.getByRole("button", {
        name: "Create",
      }),
    );

    expect(mutateAsync).toHaveBeenCalledWith({
      data: {
        itemType: "english_word",
        title: "apple",
        content: "りんご",
      },
    });
    expect(invalidateQueriesSpy).toHaveBeenCalledWith({
      queryKey: getListLearningItemsQueryKey(),
    });
    expect(await screen.findByText("一覧画面")).toBeInTheDocument();
  });

  it("タイトル未入力ならエラーを表示して登録しない", async () => {
    const mutateAsync = vi.fn();

    vi.mocked(useCreateLearningItem).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as never);

    const { user } = renderWithProviders(<LearningItemCreatePage />);

    await user.click(
      screen.getByRole("button", {
        name: "Create",
      }),
    );

    expect(
      screen.getByText("タイトルを入力してください。"),
    ).toBeInTheDocument();
    expect(mutateAsync).not.toHaveBeenCalled();
  });
  it("Content未入力ならnullで登録する", async () => {
    const mutateAsync = vi.fn().mockResolvedValue({
      status: 201,
      headers: new Headers(),
      data: {
        id: 1,
        itemType: "algorithm_problem",
        title: "二分探索",
        archivedAt: null,
      },
    });

    vi.mocked(useCreateLearningItem).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as never);

    const { user } = renderWithRouter(
      [
        {
          path: "/learning-items/new",
          element: <LearningItemCreatePage />,
        },
        {
          path: "/learning-items",
          element: <div>一覧画面</div>,
        },
      ],
      ["/learning-items/new"],
    );

    await user.type(screen.getByLabelText("Title"), "二分探索");
    await user.click(
      screen.getByRole("button", {
        name: "Create",
      }),
    );

    expect(mutateAsync).toHaveBeenCalledWith({
      data: {
        itemType: "algorithm_problem",
        title: "二分探索",
        content: null,
      },
    });
  });

  it("登録中はSavingを表示してボタンを無効化する", () => {
    vi.mocked(useCreateLearningItem).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: true,
    } as never);

    renderWithProviders(<LearningItemCreatePage />);

    const button = screen.getByRole("button", {
      name: "Saving...",
    });

    expect(button).toBeDisabled();
  });

  it("201以外なら入力エラーを表示する", async () => {
    const mutateAsync = vi.fn().mockResolvedValue({
      status: 400,
      headers: new Headers(),
      data: undefined,
    });

    vi.mocked(useCreateLearningItem).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as never);

    const { user } = renderWithProviders(<LearningItemCreatePage />);

    await user.type(screen.getByLabelText("Title"), "二分探索");
    await user.click(
      screen.getByRole("button", {
        name: "Create",
      }),
    );

    expect(
      await screen.findByText("入力内容を確認してください。"),
    ).toBeInTheDocument();
  });

  it("登録処理が失敗したらAPIエラーを表示する", async () => {
    const mutateAsync = vi.fn().mockRejectedValue(new Error("Network error"));

    vi.mocked(useCreateLearningItem).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as never);

    const { user } = renderWithProviders(<LearningItemCreatePage />);

    await user.type(screen.getByLabelText("Title"), "二分探索");
    await user.click(
      screen.getByRole("button", {
        name: "Create",
      }),
    );

    expect(
      await screen.findByText("LearningItemの登録に失敗しました。"),
    ).toBeInTheDocument();
  });
});

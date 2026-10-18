import { useState, type SubmitEvent } from "react";
import { useNavigate } from "react-router-dom";
import {
  getListLearningItemsQueryKey,
  useCreateLearningItem,
} from "../api/generated";
import { useQueryClient } from "@tanstack/react-query";

const itemTypes = [
  {
    value: "algorithm_problem",
    label: "Algorithm Problem",
  },
  {
    value: "english_word",
    label: "English Word",
  },
  {
    value: "english_article",
    label: "English Article",
  },
] as const;

export function LearningItemCreatePage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [itemType, setItemType] = useState("algorithm_problem");
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");

  const [titleError, setTitleError] = useState<string | null>(null);
  const [apiError, setApiError] = useState<string | null>(null);

  const createMutation = useCreateLearningItem();

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    setTitleError(null);
    setApiError(null);

    if (title.trim() === "") {
      setTitleError("タイトルを入力してください。");
      return;
    }

    try {
      const response = await createMutation.mutateAsync({
        data: {
          itemType,
          title: title.trim(),
          content: content.trim() || null,
        },
      });

      if (response.status !== 201) {
        setApiError("入力内容を確認してください。");
        return;
      }

      await queryClient.invalidateQueries({
        queryKey: getListLearningItemsQueryKey(),
      });

      navigate("/learning-items");
    } catch {
      setApiError("LearningItemの登録に失敗しました。");
    }
  }

  return (
    <div className="max-w-2xl">
      <h1 className="mb-8 text-2xl font-semibold">New Learning Item</h1>

      <form onSubmit={handleSubmit} className="space-y-6">
        <div>
          <label htmlFor="itemType" className="mb-2 block text-sm font-medium">
            Item Type
          </label>

          <select
            id="itemType"
            value={itemType}
            onChange={(event) => setItemType(event.target.value)}
            className="w-full rounded-lg border border-zinc-300 bg-white px-3 py-2 outline-none focus:border-zinc-900"
          >
            {itemTypes.map((item) => (
              <option key={item.value} value={item.value}>
                {item.label}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label htmlFor="title" className="mb-2 block text-sm font-medium">
            Title
          </label>

          <input
            id="title"
            value={title}
            onChange={(event) => setTitle(event.target.value)}
            className="w-full rounded-lg border border-zinc-300 bg-white px-3 py-2 outline-none focus:border-zinc-900"
          />

          {titleError && (
            <p className="mt-2 text-sm text-red-600">{titleError}</p>
          )}
        </div>

        <div>
          <label htmlFor="content" className="mb-2 block text-sm font-medium">
            Content
          </label>

          <textarea
            id="content"
            value={content}
            onChange={(event) => setContent(event.target.value)}
            rows={8}
            className="w-full rounded-lg border border-zinc-300 bg-white px-3 py-2 outline-none focus:border-zinc-900"
          />
        </div>

        {apiError && (
          <p className="rounded-lg bg-red-50 p-4 text-sm text-red-700">
            {apiError}
          </p>
        )}

        <button
          type="submit"
          disabled={createMutation.isPending}
          className="rounded-lg bg-zinc-900 px-5 py-2.5 text-sm font-medium text-white hover:bg-zinc-700 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {createMutation.isPending ? "Saving..." : "Create"}
        </button>
      </form>
    </div>
  );
}

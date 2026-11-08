import { useState, type SubmitEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import {
  getListLearningItemsQueryKey,
  useCreateLearningItem,
} from "../api/generated";
import type {
  CreateLearningItemRequestCategory,
  CreateLearningItemRequestSource,
} from "../api/generated/models";

const categories: Record<
  CreateLearningItemRequestSource,
  readonly CreateLearningItemRequestCategory[]
> = {
  AtCoder: ["Beginner", "Regular", "RegularPlus", "Grand"],
  OnlineMathContest: ["Beginner", "Regular", "Expert"],
};

export function LearningItemCreatePage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [source, setSource] =
    useState<CreateLearningItemRequestSource>("AtCoder");
  const [category, setCategory] =
    useState<CreateLearningItemRequestCategory>("Beginner");
  const [problemIndex, setProblemIndex] = useState("A");
  const [title, setTitle] = useState("");
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
          title: title.trim(),
          source,
          category,
          problemIndex,
        },
      });
      if (response.status !== 201) {
        setApiError("入力内容を確認してください。");
        return;
      }
      await queryClient.invalidateQueries({
        queryKey: getListLearningItemsQueryKey(),
      });
      navigate(`/learning-items/${response.data.id}`);
    } catch {
      setApiError("LearningItemの登録に失敗しました。");
    }
  }

  return (
    <div className="max-w-2xl">
      <h1 className="mb-8 text-2xl font-semibold">New Learning Item</h1>
      <form noValidate onSubmit={handleSubmit} className="space-y-6">
        <div>
          <label htmlFor="title" className="mb-2 block text-sm font-medium">
            Title
          </label>
          <input
            id="title"
            value={title}
            onChange={(event) => setTitle(event.target.value)}
            className="w-full rounded-lg border border-zinc-300 px-3 py-2"
          />
          {titleError && (
            <p role="alert" className="mt-2 text-sm text-red-600">
              {titleError}
            </p>
          )}
        </div>
        <div>
          <label htmlFor="source" className="mb-2 block text-sm font-medium">
            Source
          </label>
          <select
            id="source"
            value={source}
            onChange={(event) => {
              const value = event.target.value;
              if (value === "AtCoder" || value === "OnlineMathContest") {
                setSource(value);
                setCategory("Beginner");
                setProblemIndex("A");
              }
            }}
            className="w-full rounded-lg border border-zinc-300 px-3 py-2"
          >
            <option value="AtCoder">AtCoder</option>
            <option value="OnlineMathContest">OnlineMathContest</option>
          </select>
        </div>
        <div>
          <label htmlFor="category" className="mb-2 block text-sm font-medium">
            Contest Category
          </label>
          <select
            id="category"
            value={category}
            onChange={(event) => {
              const selected = categories[source].find(
                (value) => value === event.target.value,
              );
              if (selected) {
                setCategory(selected);
                setProblemIndex("A");
              }
            }}
            className="w-full rounded-lg border border-zinc-300 px-3 py-2"
          >
            {categories[source].map((value) => (
              <option key={value} value={value}>
                {value}
              </option>
            ))}
          </select>
        </div>
        <div>
          <label
            htmlFor="problemIndex"
            className="mb-2 block text-sm font-medium"
          >
            Problem Index
          </label>
          <select
            id="problemIndex"
            value={problemIndex}
            onChange={(event) => setProblemIndex(event.target.value)}
            className="w-full rounded-lg border border-zinc-300 px-3 py-2"
          >
            {(source === "AtCoder" && category === "Grand"
              ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
              : "ABCDEFG"
            )
              .split("")
              .map((value) => (
                <option key={value} value={value}>
                  {value}
                </option>
              ))}
          </select>
        </div>
        {apiError && (
          <p
            role="alert"
            className="rounded-lg bg-red-50 p-4 text-sm text-red-700"
          >
            {apiError}
          </p>
        )}
        <button
          type="submit"
          disabled={createMutation.isPending}
          className="rounded-lg bg-zinc-900 px-5 py-2.5 text-sm text-white disabled:opacity-50"
        >
          {createMutation.isPending ? "Saving..." : "Create"}
        </button>
      </form>
    </div>
  );
}

import { useState, type SubmitEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import {
  getListLearningItemsQueryKey,
  useCreateLearningItem,
} from "../api/generated";

export function LearningItemCreatePage() {
  const [url, setUrl] = useState("");
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const createMutation = useCreateLearningItem();

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    if (!url.trim()) {
      setError("問題URLを入力してください。");
      return;
    }
    try {
      const response = await createMutation.mutateAsync({
        data: { url: url.trim() },
      });
      if (response.status === 200 || response.status === 201) {
        await queryClient.invalidateQueries({
          queryKey: getListLearningItemsQueryKey(),
        });
        navigate(`/learning-items/${response.data.id}`);
        return;
      }
      setError(response.data?.title ?? "問題情報を取得できませんでした。");
    } catch {
      setError("通信に失敗しました。時間をおいて再度お試しください。");
    }
  }

  return (
    <div className="max-w-2xl">
      <h1 className="mb-8 text-2xl font-semibold">問題を登録</h1>
      <form noValidate onSubmit={handleSubmit} className="space-y-6">
        <div>
          <label
            htmlFor="problem-url"
            className="mb-2 block text-sm font-medium"
          >
            問題URL
          </label>
          <input
            id="problem-url"
            type="url"
            value={url}
            onChange={(event) => setUrl(event.target.value)}
            aria-invalid={error !== null}
            aria-describedby={error ? "problem-url-error" : undefined}
            className="w-full rounded-lg border border-zinc-300 px-3 py-2"
          />
        </div>
        {error && (
          <p
            id="problem-url-error"
            role="alert"
            className="rounded-lg bg-red-50 p-4 text-sm text-red-700"
          >
            {error}
          </p>
        )}
        <button
          type="submit"
          disabled={createMutation.isPending}
          className="rounded-lg bg-zinc-900 px-5 py-2.5 text-sm text-white disabled:opacity-50"
        >
          {createMutation.isPending ? "取得中..." : "登録"}
        </button>
      </form>
    </div>
  );
}

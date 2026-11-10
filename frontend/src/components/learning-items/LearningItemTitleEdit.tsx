import { useState, type SubmitEvent } from "react";
import { useQueryClient } from "@tanstack/react-query";
import {
  getGetLearningItemByIdQueryKey,
  getListLearningItemsQueryKey,
  useUpdateLearningItemTitle,
} from "../../api/generated";

export function LearningItemTitleEdit({
  id,
  title,
}: {
  id: number;
  title: string;
}) {
  const [editing, setEditing] = useState(false);
  const [value, setValue] = useState(title);
  const [error, setError] = useState<string | null>(null);
  const mutation = useUpdateLearningItemTitle();
  const queryClient = useQueryClient();

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    if (!value.trim()) {
      setError("タイトルを入力してください。");
      return;
    }
    try {
      const response = await mutation.mutateAsync({
        id,
        data: { title: value.trim() },
      });
      if (response.status !== 204) {
        setError(
          response.status === 404
            ? "対象の問題が見つかりません。"
            : "タイトルを更新できませんでした。",
        );
        return;
      }
      await Promise.all([
        queryClient.invalidateQueries({
          queryKey: getGetLearningItemByIdQueryKey(id),
        }),
        queryClient.invalidateQueries({
          queryKey: getListLearningItemsQueryKey(),
        }),
      ]);
      setEditing(false);
    } catch {
      setError("通信に失敗しました。時間をおいて再度お試しください。");
    }
  }

  if (!editing)
    return (
      <button
        type="button"
        onClick={() => {
          setValue(title);
          setError(null);
          setEditing(true);
        }}
        className="mt-2 text-sm underline"
      >
        タイトルを訂正
      </button>
    );
  return (
    <form onSubmit={handleSubmit} className="mt-4 space-y-3">
      <label htmlFor="edit-title" className="block text-sm font-medium">
        タイトル
      </label>
      <input
        id="edit-title"
        value={value}
        onChange={(event) => setValue(event.target.value)}
        aria-invalid={error !== null}
        aria-describedby={error ? "title-error" : undefined}
        className="w-full rounded-lg border border-zinc-300 px-3 py-2"
      />
      {error && (
        <p id="title-error" role="alert" className="text-sm text-red-700">
          {error}
        </p>
      )}
      <div className="flex gap-3">
        <button
          type="submit"
          disabled={mutation.isPending}
          className="rounded-lg bg-zinc-900 px-4 py-2 text-sm text-white disabled:opacity-50"
        >
          {mutation.isPending ? "保存中..." : "保存"}
        </button>
        <button
          type="button"
          disabled={mutation.isPending}
          onClick={() => setEditing(false)}
          className="text-sm underline"
        >
          キャンセル
        </button>
      </div>
    </form>
  );
}

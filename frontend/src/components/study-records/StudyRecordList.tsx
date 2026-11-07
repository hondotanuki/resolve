import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import {
  useListStudyRecords,
  useDeleteStudyRecord,
  getListStudyRecordsQueryKey,
  getGetLearningItemByIdQueryKey,
} from "../../api/generated";
import { StudyRecordEditForm } from "./StudyRecordEditForm";

type StudyRecordListProps = {
  learningItemId: number;
};

export function StudyRecordList({ learningItemId }: StudyRecordListProps) {
  const {
    data: response,
    isPending,
    isError,
  } = useListStudyRecords(learningItemId);

  const deleteMutation = useDeleteStudyRecord();
  const queryClient = useQueryClient();
  const [deleteError, setDeleteError] = useState<string | null>(null);
  const [editingRecordId, setEditingRecordId] = useState<number | null>(null);

  async function handleDelete(recordId: number) {
    const confirmed = window.confirm("この学習記録を削除しますか？");
    if (!confirmed) {
      return;
    }

    setDeleteError(null);

    try {
      const deleteResponse = await deleteMutation.mutateAsync({
        studyRecordId: recordId,
      });

      if (deleteResponse.status === 404) {
        setDeleteError(
          `削除対象が存在しません（HTTP ${deleteResponse.status}）。`,
        );
        return;
      }

      if (deleteResponse.status !== 204) {
        setDeleteError("削除に失敗しました。");
        return;
      }

      queryClient.invalidateQueries({
        queryKey: getListStudyRecordsQueryKey(learningItemId),
      });
      queryClient.invalidateQueries({
        queryKey: getGetLearningItemByIdQueryKey(learningItemId),
      });
    } catch {
      setDeleteError("削除処理中にエラーが発生しました。");
    }
  }

  return (
    <section className="mt-8" aria-labelledby="study-records-heading">
      <h2 id="study-records-heading" className="mb-4 text-xl font-semibold">
        Study Records
      </h2>

      {deleteError && (
        <p role="alert" className="mb-4 text-sm text-red-700">
          {deleteError}
        </p>
      )}

      {isPending ? (
        <p className="text-sm text-zinc-500">学習履歴を読み込み中...</p>
      ) : isError || !response || response.status !== 200 ? (
        <p role="alert" className="text-sm text-red-700">
          学習履歴の取得に失敗しました。
        </p>
      ) : response.data.length === 0 ? (
        <p className="text-sm text-zinc-500">学習履歴はまだありません。</p>
      ) : (
        <ul className="space-y-3">
          {response.data.map((record) => (
            <li
              key={record.id}
              className="rounded-xl border border-zinc-200 bg-white p-5"
            >
              {editingRecordId === record.id ? (
                <StudyRecordEditForm
                  record={record}
                  onClose={() => setEditingRecordId(null)}
                />
              ) : (
                <>
                  <p className="text-sm font-semibold text-zinc-900">
                    学習日時: {new Date(record.studiedAt).toLocaleString()}
                  </p>
                  <dl className="mt-3 space-y-1 text-sm text-zinc-600">
                    <div className="flex gap-2">
                      <dt className="font-medium">Result:</dt>
                      <dd>{record.result}</dd>
                    </div>
                    <div className="flex gap-2">
                      <dt className="font-medium">Duration:</dt>
                      <dd>
                        {record.durationSeconds == null
                          ? "—"
                          : `${record.durationSeconds} seconds`}
                      </dd>
                    </div>
                    <div className="flex gap-2">
                      <dt className="font-medium">Memo:</dt>
                      <dd className="whitespace-pre-wrap">
                        {record.memo ?? "—"}
                      </dd>
                    </div>
                  </dl>
                  <div className="mt-4 flex gap-2">
                    <button
                      type="button"
                      onClick={() => setEditingRecordId(record.id)}
                      disabled={
                        editingRecordId !== null || deleteMutation.isPending
                      }
                      className="rounded border border-zinc-300 px-3 py-1 text-sm"
                    >
                      編集
                    </button>
                    <button
                      type="button"
                      onClick={() => void handleDelete(record.id)}
                      disabled={
                        editingRecordId !== null || deleteMutation.isPending
                      }
                      className="rounded border border-red-300 px-3 py-1 text-sm text-red-700"
                    >
                      {deleteMutation.isPending ? "処理中..." : "削除"}
                    </button>
                  </div>
                </>
              )}
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}

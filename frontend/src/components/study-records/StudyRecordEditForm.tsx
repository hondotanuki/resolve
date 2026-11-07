import { useState, type SubmitEvent } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { format } from "date-fns";
import {
  useUpdateStudyRecord,
  getListStudyRecordsQueryKey,
  getGetLearningItemByIdQueryKey,
} from "../../api/generated";
import {
  type ListStudyRecordsResponse,
  type UpdateStudyRecordRequest,
} from "../../api/generated/models";

type StudyRecordEditFormProps = {
  record: ListStudyRecordsResponse;
  onClose: () => void;
};

export function StudyRecordEditForm({
  record,
  onClose,
}: StudyRecordEditFormProps) {
  const queryClient = useQueryClient();
  const updateMutation = useUpdateStudyRecord();

  // 入力
  const initialStudiedAt = format(
    new Date(record.studiedAt),
    "yyyy-MM-dd'T'HH:mm",
  );

  const [result, setResult] = useState(record.result);
  const [studiedAt, setStudiedAt] = useState(initialStudiedAt);
  const [durationSeconds, setDurationSeconds] = useState(
    record.durationSeconds?.toString() ?? "",
  );
  const [memo, setMemo] = useState(record.memo ?? "");

  // エラー・メッセージ
  const [studiedAtError, setStudiedAtError] = useState<string | null>(null);
  const [durationError, setDurationError] = useState<string | null>(null);
  const [apiError, setApiError] = useState<string | null>(null);
  const [noChangesMessage, setNoChangesMessage] = useState<string | null>(null);

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    // 前回のエラー・メッセージを消す
    setStudiedAtError(null);
    setDurationError(null);
    setApiError(null);
    setNoChangesMessage(null);

    // 検証
    const date = new Date(studiedAt);
    if (!studiedAt.trim()) {
      setStudiedAtError("学習日時を入力してください。");
      return;
    } else if (Number.isNaN(date.getTime())) {
      setStudiedAtError("有効な学習日時を入力してください。");
      return;
    }

    const durationValue =
      durationSeconds.trim() === "" ? null : Number(durationSeconds);
    if (
      durationValue !== null &&
      (!Number.isSafeInteger(durationValue) || durationValue < 0)
    ) {
      setDurationError("0以上の整数を入力してください。");
      return;
    }

    // 更新されたものを拾う
    const data: UpdateStudyRecordRequest = {};

    if (result !== record.result) {
      data.result = result;
    }
    if (studiedAt !== initialStudiedAt) {
      data.studiedAt = date.toISOString();
    }
    if (durationValue !== (record.durationSeconds ?? null)) {
      data.durationSeconds = durationValue;
    }
    // 空文字列をnullに統一して扱う
    const initialMemo = record.memo ?? "";
    if (memo !== initialMemo) {
      data.memo = memo === "" ? null : memo;
    }
    // 変更なしならPATCHしない
    if (Object.keys(data).length === 0) {
      setNoChangesMessage("変更はありません。");
      return;
    }

    // PATCHを実行
    try {
      const response = await updateMutation.mutateAsync({
        studyRecordId: record.id,
        data,
      });

      if (response.status === 400) {
        setApiError("入力内容を確認してください。");
        return;
      }
      if (response.status === 404) {
        setApiError("対象の学習記録が見つかりません。");
        return;
      }
      if (response.status !== 200) {
        setApiError(`更新に失敗しました。`);
        return;
      }

      queryClient.invalidateQueries({
        queryKey: getListStudyRecordsQueryKey(record.learningItemId),
      });
      queryClient.invalidateQueries({
        queryKey: getGetLearningItemByIdQueryKey(record.learningItemId),
      });
      onClose();
    } catch {
      setApiError("通信エラーが発生しました。時間をおいて再度お試しください。");
      return;
    }
  }

  return (
    <form onSubmit={handleSubmit} noValidate className="space-y-4">
      <h3 className="font-semibold">学習記録の編集</h3>

      <fieldset disabled={updateMutation.isPending} className="space-y-4">
        <div>
          <label
            htmlFor={`study-result-${record.id}`}
            className="mb-1 block text-sm font-medium"
          >
            学習結果
          </label>
          <select
            id={`study-result-${record.id}`}
            value={result}
            onChange={(event) => setResult(event.target.value)}
            className="w-full rounded border border-zinc-300 p-2"
          >
            <option value="success">success</option>
            <option value="failure">failure</option>
            <option value="reviewed">reviewed</option>
          </select>
        </div>

        <div>
          <label
            htmlFor={`study-studied-at-${record.id}`}
            className="mb-1 block text-sm font-medium"
          >
            学習日時
          </label>
          <input
            id={`study-studied-at-${record.id}`}
            type="datetime-local"
            value={studiedAt}
            onChange={(event) => setStudiedAt(event.target.value)}
            aria-invalid={studiedAtError !== null}
            aria-describedby={
              studiedAtError ? `studied-at-error-${record.id}` : undefined
            }
            className="w-full rounded border border-zinc-300 p-2"
          />
          {studiedAtError && (
            <p
              id={`studied-at-error-${record.id}`}
              role="alert"
              className="mt-1 text-sm text-red-700"
            >
              {studiedAtError}
            </p>
          )}
        </div>

        <div>
          <label
            htmlFor={`study-duration-${record.id}`}
            className="mb-1 block text-sm font-medium"
          >
            所要時間（秒）
          </label>
          <input
            id={`study-duration-${record.id}`}
            type="number"
            min="0"
            value={durationSeconds}
            onChange={(event) => setDurationSeconds(event.target.value)}
            aria-invalid={durationError !== null}
            aria-describedby={
              durationError ? `duration-error-${record.id}` : undefined
            }
            className="w-full rounded border border-zinc-300 p-2"
          />
          {durationError && (
            <p
              id={`duration-error-${record.id}`}
              role="alert"
              className="mt-1 text-sm text-red-700"
            >
              {durationError}
            </p>
          )}
        </div>

        <div>
          <label
            htmlFor={`study-memo-${record.id}`}
            className="mb-1 block text-sm font-medium"
          >
            メモ
          </label>
          <textarea
            id={`study-memo-${record.id}`}
            value={memo}
            onChange={(event) => setMemo(event.target.value)}
            rows={3}
            className="w-full rounded border border-zinc-300 p-2"
          />
        </div>

        <div className="flex gap-2">
          <button
            type="submit"
            className="rounded bg-zinc-900 px-3 py-2 text-sm text-white disabled:opacity-50"
          >
            {updateMutation.isPending ? "保存中..." : "保存"}
          </button>

          <button
            type="button"
            onClick={onClose}
            className="rounded border border-zinc-300 px-3 py-2 text-sm"
          >
            キャンセル
          </button>
        </div>
      </fieldset>
      {noChangesMessage && (
        <p role="status" className="text-sm text-zinc-600">
          {noChangesMessage}
        </p>
      )}

      {apiError && (
        <p role="alert" className="text-sm text-red-700">
          {apiError}
        </p>
      )}
    </form>
  );
}

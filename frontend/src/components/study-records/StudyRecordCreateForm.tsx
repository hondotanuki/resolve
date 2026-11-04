import { useState, type SubmitEvent } from "react";
import { useQueryClient } from "@tanstack/react-query";
import {
  getGetLearningItemByIdQueryKey,
  getListStudyRecordsQueryKey,
  useCreateStudyRecord,
} from "../../api/generated";

const results = [
  {
    value: "success",
    label: "Success",
  },
  {
    value: "failure",
    label: "Failure",
  },
  {
    value: "reviewed",
    label: "Reviewed",
  },
] as const;

type StudyRecordCreateFormProps = {
  learningItemId: number;
};

export function StudyRecordCreateForm({
  learningItemId,
}: StudyRecordCreateFormProps) {
  const [result, setResult] = useState("success");
  const [studiedAt, setStudiedAt] = useState("");
  const [durationSeconds, setDurationSeconds] = useState("");
  const [memo, setMemo] = useState("");

  const [studiedAtError, setStudiedAtError] = useState<string | null>(null);
  const [durationSecondsError, setDurationSecondsError] = useState<
    string | null
  >(null);
  const [apiError, setApiError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const createMutation = useCreateStudyRecord();
  const queryClient = useQueryClient();

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    setStudiedAtError(null);
    setDurationSecondsError(null);
    setApiError(null);
    setSuccessMessage(null);

    if (studiedAt === "") {
      setStudiedAtError("学習日時を入力してください。");
      return;
    }
    const studiedAtDate = new Date(studiedAt);
    if (Number.isNaN(studiedAtDate.getTime())) {
      setStudiedAtError("正しい学習日時を入力してください。");
      return;
    }

    const duration = durationSeconds === "" ? null : Number(durationSeconds);
    if (duration !== null && (!Number.isInteger(duration) || duration < 0)) {
      setDurationSecondsError("学習時間は0以上の整数で入力してください。");
      return;
    }

    try {
      const response = await createMutation.mutateAsync({
        learningItemId,
        data: {
          result,
          studiedAt: studiedAtDate.toISOString(),
          durationSeconds: duration,
          memo: memo.trim() === "" ? null : memo.trim(),
        },
      });

      if (response.status === 404) {
        setApiError("対象のLearningItemが見つかりません。");
        return;
      }

      if (response.status !== 201) {
        setApiError("入力内容を確認してください。");
        return;
      }

      setSuccessMessage("学習記録を登録しました。");
      queryClient.invalidateQueries({
        queryKey: getGetLearningItemByIdQueryKey(learningItemId),
      });
      queryClient.invalidateQueries({
        queryKey: getListStudyRecordsQueryKey(learningItemId),
      });
    } catch {
      setApiError("StudyRecordの登録に失敗しました。");
    }
  }

  return (
    <div className="max-w-2xl">
      <h1 className="mb-8 text-2xl font-semibold">New Study Record</h1>

      <form
        noValidate
        onSubmit={handleSubmit}
        className="space-y-6 rounded-xl border border-zinc-200 bg-white p-6"
      >
        <div>
          <label htmlFor="results" className="mb-2 block text-sm font-medium">
            Result
          </label>

          <select
            id="results"
            required
            value={result}
            onChange={(e) => setResult(e.target.value)}
            className="w-full rounded-lg border border-zinc-300 bg-white px-3 py-2 outline-none focus:border-zinc-900"
          >
            {results.map((result) => (
              <option key={result.value} value={result.value}>
                {result.label}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label htmlFor="studiedAt" className="mb-2 block text-sm font-medium">
            Studied At
          </label>
          <input
            id="studiedAt"
            type="datetime-local"
            required
            className={`w-full rounded-lg border bg-white px-3 py-2 outline-none ${
              studiedAtError
                ? "border-red-500 focus:border-red-600"
                : "border-zinc-300 focus:border-zinc-900"
            }`}
            value={studiedAt}
            onChange={(event) => {
              setStudiedAt(event.target.value);
              setStudiedAtError(null);
            }}
            aria-invalid={studiedAtError !== null}
            aria-describedby={studiedAtError ? "studiedAt-error" : undefined}
          />
          {studiedAtError && (
            <p
              id="studiedAt-error"
              role="alert"
              className="mt-2 text-sm text-red-600"
            >
              {studiedAtError}
            </p>
          )}
        </div>

        <div>
          <label
            htmlFor="durationSeconds"
            className="mb-2 block text-sm font-medium"
          >
            Duration (seconds)
          </label>
          <input
            id="durationSeconds"
            type="number"
            min={0}
            step={1}
            className={`w-full rounded-lg border bg-white px-3 py-2 outline-none ${
              durationSecondsError
                ? "border-red-500 focus:border-red-600"
                : "border-zinc-300 focus:border-zinc-900"
            }`}
            value={durationSeconds}
            onChange={(event) => {
              setDurationSeconds(event.target.value);
              setDurationSecondsError(null);
            }}
            aria-invalid={durationSecondsError !== null}
            aria-describedby={
              durationSecondsError ? "durationSeconds-error" : undefined
            }
          />
          {durationSecondsError && (
            <p
              id="durationSeconds-error"
              role="alert"
              className="mt-2 text-sm text-red-600"
            >
              {durationSecondsError}
            </p>
          )}
        </div>

        <div>
          <label htmlFor="memo" className="mb-2 block text-sm font-medium">
            Memo
          </label>

          <textarea
            id="memo"
            value={memo}
            onChange={(event) => setMemo(event.target.value)}
            rows={8}
            className="w-full rounded-lg border border-zinc-300 bg-white px-3 py-2 outline-none focus:border-zinc-900"
          />
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
          className="rounded-lg bg-zinc-900 px-5 py-2.5 text-sm font-medium text-white hover:bg-zinc-700 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {createMutation.isPending ? "Saving..." : "Create"}
        </button>
      </form>
      {successMessage && (
        <div
          role="status"
          className="mt-4 rounded-lg bg-green-50 p-4 text-sm text-green-700"
        >
          <p>{successMessage}</p>
        </div>
      )}
    </div>
  );
}

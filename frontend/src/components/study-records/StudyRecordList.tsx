import { useListStudyRecords } from "../../api/generated";

type StudyRecordListProps = {
  learningItemId: number;
};

export function StudyRecordList({ learningItemId }: StudyRecordListProps) {
  const {
    data: response,
    isPending,
    isError,
  } = useListStudyRecords(learningItemId);

  return (
    <section className="mt-8" aria-labelledby="study-records-heading">
      <h2 id="study-records-heading" className="mb-4 text-xl font-semibold">
        Study Records
      </h2>

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
              <p className="text-sm font-semibold text-zinc-900">
                {new Date(record.studiedAt).toLocaleString()}
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
                  <dd className="whitespace-pre-wrap">{record.memo ?? "—"}</dd>
                </div>
              </dl>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}

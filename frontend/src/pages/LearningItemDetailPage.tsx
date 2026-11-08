import { Link, useParams } from "react-router-dom";
import {
  useGetLearningItemById,
  getGetLearningItemByIdQueryKey,
} from "../api/generated";
import { StudyRecordCreateForm } from "../components/study-records/StudyRecordCreateForm";
import { StudyRecordList } from "../components/study-records/StudyRecordList";

export function LearningItemDetailPage() {
  const { id } = useParams();
  const itemId = Number(id);

  const {
    data: response,
    isPending,
    isError,
  } = useGetLearningItemById(itemId, {
    query: {
      queryKey: getGetLearningItemByIdQueryKey(itemId),
      enabled: Number.isInteger(itemId),
    },
  });

  if (!Number.isInteger(itemId)) {
    return (
      <p className="text-xl font-semibold">LearningItemが見つかりません。</p>
    );
  }

  if (isPending) {
    return <p className="text-sm text-zinc-500">Loading...</p>;
  }

  if (isError) {
    return (
      <p className="text-sm text-zinc-500">
        LearningItemの取得に失敗しました。
      </p>
    );
  }

  if (response.status === 404) {
    return (
      <div>
        <h1 className="text-xl font-semibold">
          LearningItemが見つかりません。
        </h1>

        <Link
          to="/learning-items"
          className="mt-4 inline-block text-sm underline"
        >
          一覧へ戻る
        </Link>
      </div>
    );
  }

  if (response.status !== 200) {
    return (
      <p className="text-xl font-semibold">
        LearningItemの取得に失敗しました。
      </p>
    );
  }

  const item = response.data;

  return (
    <article className="max-w-2xl">
      <Link
        to="/learning-items"
        className="mb-6 inline-block text-sm text-zinc-500 hover:text-zinc-900"
      >
        ← Back
      </Link>

      <h1 className="text-3xl font-semibold">{item.title}</h1>
      <p>現在の状態: {item.status}</p>

      <p className="mt-4 text-sm text-zinc-600">
        {item.source} / {item.category} / {item.problemIndex}
      </p>

      <div className="mt-6 space-y-1 text-xs text-zinc-400">
        <p>Created: {item.createdAt}</p>
        <p>Updated: {item.updatedAt}</p>
      </div>
      <StudyRecordCreateForm learningItemId={itemId} />
      <StudyRecordList learningItemId={itemId} />
    </article>
  );
}

import { useUpdateStudyRecord } from "./generated";
import type { UpdateStudyRecordRequest } from "./generated/models";

/** 生成型では表現されない「更新項目が1つ以上必要」というPATCHの契約を補う。 */
type RequireAtLeastOne<T> = {
  [Key in keyof T]-?: Required<Pick<T, Key>> & Omit<T, Key>;
}[keyof T];

export type StudyRecordUpdate = RequireAtLeastOne<UpdateStudyRecordRequest>;

export function hasStudyRecordUpdate(
  data: UpdateStudyRecordRequest,
): data is StudyRecordUpdate {
  const keys = ["result", "studiedAt", "durationSeconds", "memo"] as const;
  return keys.some(
    (key) => Object.hasOwn(data, key) && data[key] !== undefined,
  );
}

/** 生成Hookへの送信を包み、空の更新をコンパイル時に防ぐ。 */
export function useStudyRecordUpdate() {
  const mutation = useUpdateStudyRecord();

  return {
    isPending: mutation.isPending,
    update: (studyRecordId: number, data: StudyRecordUpdate) =>
      mutation.mutateAsync({ studyRecordId, data }),
  };
}

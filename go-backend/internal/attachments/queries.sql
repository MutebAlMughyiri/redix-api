-- name: AttachmentSent :many
-- The file a person already sent to the project with these bytes, which a retried upload answers (UC-029).
SELECT * FROM attachments WHERE project_id = sqlc.arg(project_id) AND uploaded_by_user_id = sqlc.arg(user_id) AND sha256 = sqlc.arg(sha256) LIMIT 1;

-- name: AddAttachment :exec
INSERT INTO attachments (id, project_id, storage_path, file_name, content_type, size_bytes, sha256, uploaded_by_user_id, uploaded_at)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);

-- name: AttachmentWithID :one
-- The attachment a signed link opens, whatever its project: the link names it, and its signature says it was made for it.
SELECT * FROM attachments WHERE id = ?;

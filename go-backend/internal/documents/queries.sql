-- name: DocumentsOf :many
-- A project's documents with their files: those in no group first, then each group in order, newest first within each.
SELECT documents.id, documents.name, documents.group_name, attachments.id AS attachment_id, attachments.file_name, attachments.content_type, documents.uploaded_at
FROM documents JOIN attachments ON attachments.id = documents.attachment_id
WHERE documents.project_id = ? ORDER BY documents.group_name COLLATE names, documents.uploaded_at DESC;

-- name: DocumentOf :one
SELECT * FROM documents WHERE id = sqlc.arg(id) AND project_id = sqlc.arg(project_id);

-- name: DocumentKept :one
SELECT EXISTS (SELECT 1 FROM documents WHERE project_id = sqlc.arg(project_id) AND attachment_id = sqlc.arg(attachment_id));

-- name: AddDocument :exec
INSERT INTO documents (id, project_id, attachment_id, name, group_name, uploaded_by_user_id, uploaded_at) VALUES (?, ?, ?, ?, ?, ?, ?);

-- name: RemoveDocument :exec
DELETE FROM documents WHERE id = ?;

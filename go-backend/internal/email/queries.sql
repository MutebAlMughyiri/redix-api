-- name: WordsOf :many
-- The subject and body words of one email, in one language.
SELECT translation_keys.key, translations.value
FROM translation_keys JOIN translations ON translations.translation_key_id = translation_keys.id
WHERE translations.language = sqlc.arg(language) AND translation_keys.key IN (sqlc.arg(subject_key), sqlc.arg(body_key));

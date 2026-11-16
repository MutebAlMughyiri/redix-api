-- name: WordsIn :many
-- Every word of one language, by its key.
SELECT translation_keys.key, translations.value
FROM translation_keys JOIN translations ON translations.translation_key_id = translation_keys.id
WHERE translations.language = sqlc.arg(language);

-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- name: WordsIn :many
-- Every word of one language, by its key.
SELECT translation_keys.key, translations.value
FROM translation_keys JOIN translations ON translations.translation_key_id = translation_keys.id
WHERE translations.language = sqlc.arg(language);

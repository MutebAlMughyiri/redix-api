-- name: WaitingByKind :many
-- What waits for approval in the project, by kind: how many and how much.
SELECT kind, count(*) AS entries, CAST(SUM(amount) AS INTEGER) AS total FROM entries WHERE project_id = ? AND status = 1 GROUP BY kind;

-- name: ItemTotalsOf :many
-- What each item of the project came to (ItemTotals, BR-02, BR-05): an expense item's approved expenses, a revenue item's approved revenue, the
-- capital item what the partners put in less what they took out.
SELECT moved.item_id, CAST(SUM(moved.amount) AS INTEGER) AS total FROM (
    SELECT expense_details.item_id, entries.amount FROM expense_details JOIN entries ON entries.id = expense_details.entry_id
    WHERE entries.project_id = sqlc.arg(project_id) AND entries.status = 2
    UNION ALL SELECT revenue_details.item_id, entries.amount FROM revenue_details JOIN entries ON entries.id = revenue_details.entry_id
    WHERE entries.project_id = sqlc.arg(project_id) AND entries.status = 2
    UNION ALL SELECT items.id, CASE WHEN entries.kind = 3 THEN -entries.amount ELSE entries.amount END
    FROM capital_movement_details JOIN entries ON entries.id = capital_movement_details.entry_id
        JOIN categories ON categories.project_id = entries.project_id JOIN items ON items.category_id = categories.id AND items.type = 6
    WHERE entries.project_id = sqlc.arg(project_id) AND entries.status = 2
) AS moved GROUP BY moved.item_id;

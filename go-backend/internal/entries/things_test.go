// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package entries

import (
	"context"
	"path/filepath"
	"strings"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
)

// shownNowhere are the tables whose rows never make a line of a project's history, each with why.
var shownNowhere = map[string]string{
	"Users":                  "a login's lines, which «سجلي» reads (UC-069 R6)",
	"SystemPermissions":      "the admin's, in no project",
	"Devices":                "never recorded, as the History net's exempt says",
	"EmailCodes":             "never recorded, as the History net's exempt says",
	"Notifications":          "never recorded, as the History net's exempt says",
	"NotificationEntries":    "never recorded, as the History net's exempt says",
	"UserDefaultProjects":    "never recorded, as the History net's exempt says",
	"History":                "History itself",
	"GooseDbVersion":         "the schema's own version",
	"SqliteSequence":         "SQLite's own counter",
	"EntryDecisions":         "recorded only through its entry's line",
	"PaymentMethodAccounts":  "recorded only through its method's or account's line",
	"ReconciliationBalances": "recorded only through its reconciliation's line",
	"ConfigValues":           "the configured values, which migrations alone change",
	"TranslationKeys":        "the words, which migrations alone change",
	"Translations":           "the words, which migrations alone change",
}

// UC-069 R2: every table is a thing of the project history or says why it shows nowhere, so a table added later cannot slip out of it.
func TestR2_EveryTableIsAThingOfTheProjectHistoryOrSaysWhyNot(t *testing.T) {
	database, err := db.Open(context.Background(), filepath.Join(t.TempDir(), "redix.db"))
	if err != nil {
		t.Fatal(err)
	}
	defer database.Close()
	rows, err := database.Read.Query("SELECT name FROM sqlite_schema WHERE type = 'table' AND name NOT LIKE 'lk\\_%' ESCAPE '\\'")
	if err != nil {
		t.Fatal(err)
	}
	defer rows.Close()
	for rows.Next() {
		var name string
		rows.Scan(&name)
		parts := strings.Split(name, "_")
		for i, part := range parts {
			parts[i] = strings.ToUpper(part[:1]) + part[1:]
		}
		table := strings.Join(parts, "")
		_, isThing := things[table]
		_, isNowhere := shownNowhere[table]
		if isThing == isNowhere {
			t.Errorf("%s is a thing: %v; shown nowhere: %v; it must be one of them", table, isThing, isNowhere)
		}
	}
}

// UC-069 D7: History keeps a permission as its place in access.All, so a new one goes last and every older line keeps naming its own.
func TestR7_ANewPermissionTakesNoOlderPermissionsNumber(t *testing.T) {
	if access.GenerateReports.Number() != 18 || access.SeeProjectHistory.Number() != len(access.All)-1 {
		t.Errorf("GenerateReports is %d, SeeProjectHistory %d of %d", access.GenerateReports.Number(), access.SeeProjectHistory.Number(), len(access.All))
	}
}

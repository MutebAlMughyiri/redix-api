// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package db

import (
	"slices"
	"strings"
	"testing"
)

func TestD7_NamesSortInArabicDictionaryOrderWithEnglishCaseIgnored(t *testing.T) {
	db := open(t)
	rows, err := db.Read.Query(`SELECT name FROM (SELECT 'سعيد' AS name UNION ALL SELECT 'أحمد' UNION ALL SELECT 'بدر' UNION ALL SELECT 'ابراهيم'
		UNION ALL SELECT 'إسماعيل' UNION ALL SELECT 'آمنة' UNION ALL SELECT 'Bader' UNION ALL SELECT 'ahmed' UNION ALL SELECT 'احمد') ORDER BY name COLLATE names`)
	if err != nil {
		t.Fatal(err)
	}
	defer func() { _ = rows.Close() }()
	var got []string
	for rows.Next() {
		var name string
		if err := rows.Scan(&name); err != nil {
			t.Fatal(err)
		}
		got = append(got, name)
	}
	want := []string{"ahmed", "Bader", "ابراهيم", "أحمد", "احمد", "إسماعيل", "آمنة", "بدر", "سعيد"}
	if !slices.Equal(got, want) {
		t.Errorf("got  %s\nwant %s", strings.Join(got, " "), strings.Join(want, " "))
	}
}

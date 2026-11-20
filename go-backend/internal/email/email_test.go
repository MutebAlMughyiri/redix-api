// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package email

import (
	"context"
	"os"
	"path/filepath"
	"strings"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
)

func TestR127_ACodeIsEmailedInTheAskersLanguage(t *testing.T) {
	database, err := db.Open(context.Background(), filepath.Join(t.TempDir(), "redix.db"))
	if err != nil {
		t.Fatal(err)
	}
	defer func() { _ = database.Close() }()
	folder := Folder{Path: filepath.Join(t.TempDir(), "Emails")}
	if err := Send(context.Background(), database, folder, "manager@example.com", "en", "email.resetSubject", "email.resetBody", "123456"); err != nil {
		t.Fatal(err)
	}
	files, _ := os.ReadDir(folder.Path)
	if len(files) != 1 {
		t.Fatalf("%d emails", len(files))
	}
	kept, _ := os.ReadFile(filepath.Join(folder.Path, files[0].Name()))
	text := string(kept)
	if !strings.HasPrefix(text, "To: manager@example.com\nSubject: ") || !strings.Contains(text, "123456") || strings.Contains(text, "{0}") {
		t.Errorf("%s", text)
	}
}

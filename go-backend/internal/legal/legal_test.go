// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package legal_test

import (
	"io/fs"
	"os"
	"path/filepath"
	"regexp"
	"slices"
	"strings"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
)

func TestQ6_ThePrivacyPolicyAndTheTermsAreServedToAnyoneInArabicAndEnglish(t *testing.T) {
	api := testapi.New(t)
	for path, words := range map[string][]string{
		"/privacy": {"سياسة الخصوصية", "Privacy policy", "Tech Details"},
		"/terms":   {"شروط استخدام", "Terms of use", "Tech Details"},
	} {
		answer := api.Send("GET", path, "")
		if answer.Code != 200 || answer.Header().Get("Content-Type") != "text/html; charset=utf-8" {
			t.Fatalf("%s: %d %s", path, answer.Code, answer.Header().Get("Content-Type"))
		}
		for _, word := range words {
			if !strings.Contains(strings.ToLower(answer.Body.String()), strings.ToLower(word)) {
				t.Errorf("%s does not say %q", path, word)
			}
		}
	}
}

// notice is the copyright notice every source file opens with (#143 R1), in the comment of its language.
var notice = []string{"Copyright 2026 Tech Details. All rights reserved.", "Use of this file is governed by the LICENSE file at the root of this repository."}

// generated is the line a tool writes into a file it made, such as sqlc's store; such a file is the tool's, and carries no notice.
var generated = regexp.MustCompile(`(?m)^// Code generated .* DO NOT EDIT\.$`)

func TestR6_EveryGoAndSqlFileOpensWithTheNoticeWordForWord(t *testing.T) {
	var missing []string
	err := filepath.WalkDir("../..", func(path string, entry fs.DirEntry, err error) error {
		if err != nil || entry.IsDir() {
			return err
		}
		comment := map[string]string{".go": "// ", ".sql": "-- "}[filepath.Ext(path)]
		if comment == "" {
			return nil
		}
		text, err := os.ReadFile(path)
		if err != nil {
			return err
		}
		if generated.Match(text) {
			return nil
		}
		lines := strings.Split(strings.ReplaceAll(string(text), "\r\n", "\n"), "\n")
		if len(lines) > 0 && lines[0] == "-- +goose Up" {
			lines = lines[1:]
		}
		want := []string{comment + notice[0], comment + notice[1]}
		if len(lines) < 2 || !slices.Equal(lines[:2], want) {
			missing = append(missing, path)
		}
		return nil
	})
	if err != nil {
		t.Fatal(err)
	}
	if len(missing) > 0 {
		t.Errorf("%d files open without the notice: %v", len(missing), missing)
	}
}

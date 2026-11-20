// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package stored_test

import (
	"bytes"
	"log/slog"
	"strings"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/stored"
)

// redix-api #76 row 282: a value kept is read; a value not kept reads as nothing and logs nothing; a value that cannot be read reads as
// nothing and is logged.
func TestRow282_AStoredValueIsReadOrReadsAsNothingAndOnlyOneUnreadableIsLogged(t *testing.T) {
	var logged bytes.Buffer
	before := slog.Default()
	slog.SetDefault(slog.New(slog.NewTextHandler(&logged, nil)))
	t.Cleanup(func() { slog.SetDefault(before) })

	var name string
	stored.Read([]byte(`"حساب المؤسسة"`), &name)
	if name != "حساب المؤسسة" || logged.Len() != 0 {
		t.Errorf("a value kept: %q, logged %q", name, logged.String())
	}
	var none string
	stored.Read(nil, &none)
	stored.Read([]byte{}, &none)
	if none != "" || logged.Len() != 0 {
		t.Errorf("a value not kept: %q, logged %q", none, logged.String())
	}
	var number int64
	stored.Read([]byte(`"not a number"`), &number)
	if number != 0 || !strings.Contains(logged.String(), "a stored JSON value could not be read") || !strings.Contains(logged.String(), "*int64") {
		t.Errorf("a value that cannot be read: %d, logged %q", number, logged.String())
	}
}

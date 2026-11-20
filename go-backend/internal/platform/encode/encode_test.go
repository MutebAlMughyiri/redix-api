// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package encode_test

import (
	jsonv1 "encoding/json"
	"strings"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/encode"
)

type line struct {
	Name    string            `json:"name"`
	Lines   []string          `json:"lines"`
	Values  map[string]string `json:"values"`
	Note    string            `json:"note,omitempty"`
	Count   int               `json:"count,omitempty"`
	Amounts []int             `json:"amounts,omitzero"`
}

// redix-api #125: a nil list is written [] and a nil map {}, as encoding/json writes an empty one, so the app never reads null for either.
func TestR125_ANilListOrMapIsWrittenEmpty(t *testing.T) {
	empty, _ := jsonv1.Marshal(line{Name: "<b>A & B</b>", Lines: []string{}, Values: map[string]string{}})
	written, err := encode.JSON(line{Name: "<b>A & B</b>"})
	if err != nil || string(written) != string(empty) || !strings.Contains(string(written), `"lines":[],"values":{}`) {
		t.Errorf("nil list and map: %s (%v), encoding/json writes the empty ones %s", written, err, empty)
	}
}

// redix-api #125, AC-73.1: a project's archive is laid out as encoding/json's MarshalIndent laid it out, its nil lists written empty.
func TestR125_AnIndentedValueIsLaidOutAsMarshalIndentLaidItOut(t *testing.T) {
	full := line{Name: "حساب", Lines: []string{"x", "y"}, Values: map[string]string{"b": "2", "a": "1"}, Count: 2}
	v1, _ := jsonv1.MarshalIndent(full, "", " ")
	if written, err := encode.Indented(full); err != nil || string(written) != string(v1) {
		t.Errorf("indented: %s (%v), MarshalIndent wrote %s", written, err, v1)
	}
	if written, _ := encode.Indented(line{}); !strings.Contains(string(written), `"lines": []`) {
		t.Errorf("a nil list indented: %s", written)
	}
}

// redix-api #125: everything else is written as encoding/json wrote it - keys sorted, <, > and & escaped, names as tagged, zero values
// omitted where tagged so.
func TestR125_EverythingElseIsWrittenAsEncodingJSONWroteIt(t *testing.T) {
	full := line{Name: "حساب <b>", Lines: []string{"x"}, Values: map[string]string{"b": "2", "a": "1"}, Note: "n", Count: 2, Amounts: []int{1}}
	v1, _ := jsonv1.Marshal(full)
	if written, _ := encode.JSON(full); string(written) != string(v1) {
		t.Errorf("a full value: %s, encoding/json wrote %s", written, v1)
	}
}

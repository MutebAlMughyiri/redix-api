// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package encode writes JSON the one way the API does: encoding/json's rules, but a nil list written [] and a nil map {}, so an empty list
// needs no []T{} for the app to read it as empty rather than null, which it refuses (redix-api #125, the owner 20 Nov 2026). It is
// encoding/json/v2, an experiment in Go 1.27, so every build sets GOEXPERIMENT=jsonv2 (redix-api #155 takes the flag away once Go publishes it).
package encode

import (
	jsonv1 "encoding/json"
	"encoding/json/jsontext"
	"encoding/json/v2"
)

// written is encoding/json's behaviour with nil lists and maps written empty.
var written = json.JoinOptions(jsonv1.DefaultOptionsV1(), json.FormatNilSliceAsNull(false), json.FormatNilMapAsNull(false))

// JSON is value as the API writes JSON: an answer, a History line's values, a list a query reads.
func JSON(value any) ([]byte, error) { return json.Marshal(value, written) }

// Indented is value as JSON a person may read too, one member a line indented by indent, as encoding/json's MarshalIndent laid it out: a
// project's archive, the OpenAPI document.
func Indented(value any, indent string) ([]byte, error) {
	return json.Marshal(value, written, jsontext.Multiline(true), jsontext.WithIndent(indent), jsontext.SpaceAfterColon(true))
}

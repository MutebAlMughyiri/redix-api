// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"encoding/json"
	"fmt"
	"strconv"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/encode"
)

// Enum is an enumeration kept as a number, as .NET's JsonStringEnumConverter wrote and read it under the camel-case policy: its names, in the
// order of their numbers. A type kept this way - an item's type, a column's data type - asks its Enum for both directions, so every such type
// travels the same way.
type Enum []string

// Write is value's name, as the API writes it.
func (e Enum) Write(value int64) ([]byte, error) {
	if value < 0 || value >= int64(len(e)) {
		return nil, fmt.Errorf("web: %d names none of %v", value, e)
	}
	return encode.JSON(e[value])
}

// Read is the value data names, as the converter read it: one of the names in any case, or a number naming one.
func (e Enum) Read(data []byte) (int64, error) {
	var text string
	if json.Unmarshal(data, &text) == nil {
		return e.Parse(text)
	}
	var number int64
	if err := json.Unmarshal(data, &number); err != nil || number < 0 || number >= int64(len(e)) {
		return 0, fmt.Errorf("web: %s names none of %v", data, e)
	}
	return number, nil
}

// Parse is the value text names in a URL, as Enum.TryParse with Enum.IsDefined read it: one of the names in any case, or its number.
func (e Enum) Parse(text string) (int64, error) {
	for value, name := range e {
		if strings.EqualFold(name, text) {
			return int64(value), nil
		}
	}
	if number, err := strconv.ParseInt(text, 10, 64); err == nil && number >= 0 && number < int64(len(e)) {
		return number, nil
	}
	return 0, fmt.Errorf("web: %q names none of %v", text, e)
}

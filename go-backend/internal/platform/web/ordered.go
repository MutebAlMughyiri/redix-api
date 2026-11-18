// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"bytes"
	"encoding/json"
	"fmt"
	"strings"
)

// Ordered is a JSON object of text values in the order it was written, as .NET's Dictionary<string, string> kept a request's keys: what an entry
// keeps of its item's columns, which the app reads back as the text .NET wrote.
type Ordered struct {
	Keys   []string
	Values map[string]string
}

// UnmarshalJSON reads the object, keeping the order of its keys.
func (o *Ordered) UnmarshalJSON(data []byte) error {
	o.Keys, o.Values = nil, map[string]string{}
	decoder := json.NewDecoder(bytes.NewReader(data))
	if token, err := decoder.Token(); err != nil || token != json.Delim('{') {
		return fmt.Errorf("web: %s is not an object of text", data)
	}
	for decoder.More() {
		key, err := decoder.Token()
		if err != nil {
			return err
		}
		var value string
		if err := decoder.Decode(&value); err != nil {
			return err
		}
		if _, seen := o.Values[key.(string)]; !seen {
			o.Keys = append(o.Keys, key.(string))
		}
		o.Values[key.(string)] = value
	}
	_, err := decoder.Token()
	return err
}

// KeptText is the object as SQL Server's json column gives it back, the form .NET read and answered (Entries.ColumnValues is json): its keys in
// order, no spaces, and every character as it is but a quote, a backslash and a control character - \b \f \n \r \t short, any other as
// \u00XX in upper case.
func (o Ordered) KeptText() string {
	var text strings.Builder
	text.WriteByte('{')
	for i, key := range o.Keys {
		if i > 0 {
			text.WriteByte(',')
		}
		writeKeptString(&text, key)
		text.WriteByte(':')
		writeKeptString(&text, o.Values[key])
	}
	text.WriteByte('}')
	return text.String()
}

func writeKeptString(text *strings.Builder, value string) {
	text.WriteByte('"')
	for _, letter := range value {
		switch letter {
		case '"':
			text.WriteString(`\"`)
		case '\\':
			text.WriteString(`\\`)
		case '\b':
			text.WriteString(`\b`)
		case '\f':
			text.WriteString(`\f`)
		case '\n':
			text.WriteString(`\n`)
		case '\r':
			text.WriteString(`\r`)
		case '\t':
			text.WriteString(`\t`)
		default:
			if letter < 0x20 {
				fmt.Fprintf(text, `\u%04X`, letter)
			} else {
				text.WriteRune(letter)
			}
		}
	}
	text.WriteByte('"')
}

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

// DotNetText is the object as System.Text.Json wrote it by default: its keys in order, no spaces, and every character but printable ASCII - and the
// ones HTML treats as markup - escaped as \uXXXX with upper-case digits.
func (o Ordered) DotNetText() string {
	var text strings.Builder
	text.WriteByte('{')
	for i, key := range o.Keys {
		if i > 0 {
			text.WriteByte(',')
		}
		writeDotNetString(&text, key)
		text.WriteByte(':')
		writeDotNetString(&text, o.Values[key])
	}
	text.WriteByte('}')
	return text.String()
}

func writeDotNetString(text *strings.Builder, value string) {
	text.WriteByte('"')
	for _, letter := range value {
		switch {
		case letter == '"':
			text.WriteString(`"`)
		case letter == '\\':
			text.WriteString(`\\`)
		case letter == '\n':
			text.WriteString(`\n`)
		case letter == '\r':
			text.WriteString(`\r`)
		case letter == '\t':
			text.WriteString(`\t`)
		case letter < 0x20 || letter > 0x7E || strings.ContainsRune("<>&'+`", letter):
			for _, unit := range utf16Of(letter) {
				fmt.Fprintf(text, `\u%04X`, unit)
			}
		default:
			text.WriteRune(letter)
		}
	}
	text.WriteByte('"')
}

func utf16Of(letter rune) []rune {
	if letter < 0x10000 {
		return []rune{letter}
	}
	letter -= 0x10000
	return []rune{0xD800 + letter>>10, 0xDC00 + letter&0x3FF}
}

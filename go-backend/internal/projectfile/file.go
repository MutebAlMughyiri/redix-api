// Package projectfile loads a project's file into REDIX (UC-071, FR-53, BR-43): a project that lived in Excel comes in at once, every part of it
// through the owner that takes it by hand, inside one change. The load holds no rule of its own: it reads the file, finds or adds each part, and
// asks the owners; one refusal anywhere saves nothing (ProjectFileEndpoints).
package projectfile

import (
	"bytes"
	"encoding/json"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
	"strings"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
)

// What a REDIX project file says it is, and the newest version of it this API reads.
const (
	formatName    = "redix.project"
	latestVersion = 1
)

// file is a project's file (ProjectFile, redix-api #130, #132): its report, what each account really held (D11), every list its cells choose from,
// and its categories with their items and tables, each table with its columns and rows. What is worked out is not in it.
type file struct {
	Report     fileReport     `json:"report"`
	Accounts   []fileAccount  `json:"accounts"`
	Lists      []fileList     `json:"lists"`
	Categories []fileCategory `json:"categories"`
}

// fileReport is the report a project file was written from (D9): its number, 0 for none, and its date and time in Saudi Arabia.
type fileReport struct {
	Number int64  `json:"number"`
	Date   string `json:"date"`
	Time   string `json:"time"`
}

// at is when the report was issued: its date and time read in the zone of the project the file loads into (redix-app #107); false when they are not
// a date and a time.
func (r fileReport) at(zone string) (moment.Moment, bool) {
	local, err := time.ParseInLocation("2006-01-02 15:04", r.Date+" "+r.Time, zones.Location(zone))
	if err != nil || len(r.Date) != 10 || len(r.Time) != 5 {
		return moment.Moment{}, false
	}
	return moment.Of(local), true
}

// fileAccount is an account of the file by its name, and what it really held at the file's report (D11), as written.
type fileAccount struct {
	Name          string          `json:"name"`
	ActualBalance json.RawMessage `json:"actualBalance"`
}

// fileList is a list of the file: its key, which a list column names, and its options, each with the id a row keeps and its label.
type fileList struct {
	Key     string `json:"key"`
	Options []struct {
		ID    string `json:"id"`
		Label string `json:"label"`
	} `json:"options"`
}

type fileCategory struct {
	Name  string     `json:"name"`
	Items []fileItem `json:"items"`
}

type fileItem struct {
	Name   string             `json:"name"`
	Type   structure.ItemType `json:"type"`
	Tables []fileTable        `json:"tables"`
}

// fileTable is a table of the file: its name, what its entries are, its columns and its rows; a contract's payments carry what was agreed, a
// number, or "" for an open value.
type fileTable struct {
	Name           string             `json:"name"`
	Kind           structure.ItemType `json:"kind"`
	Columns        []fileColumn       `json:"columns"`
	Rows           []fileRow          `json:"rows"`
	ContractAmount json.RawMessage    `json:"contractAmount"`
}

type fileColumn struct {
	Key  string             `json:"key"`
	Name string             `json:"name"`
	Type structure.DataType `json:"type"`
	List string             `json:"list"`
}

// fileRow is a row of a table: a value under each column's key, in the order the file wrote them.
type fileRow struct {
	keys   []string
	values map[string]json.RawMessage
}

// UnmarshalJSON reads the row's values in the order they were written.
func (r *fileRow) UnmarshalJSON(data []byte) error {
	decoder := json.NewDecoder(bytes.NewReader(data))
	if _, err := decoder.Token(); err != nil {
		return err
	}
	r.values = map[string]json.RawMessage{}
	for decoder.More() {
		key, err := decoder.Token()
		if err != nil {
			return err
		}
		var value json.RawMessage
		if err := decoder.Decode(&value); err != nil {
			return err
		}
		name, _ := key.(string)
		if _, seen := r.values[name]; !seen {
			r.keys = append(r.keys, name)
		}
		r.values[name] = value
	}
	return nil
}

// textOf is a value as the file wrote it: a text trimmed, a number as written, anything else empty.
func textOf(value json.RawMessage) string {
	var text string
	if json.Unmarshal(value, &text) == nil {
		return strings.TrimSpace(text)
	}
	var number json.Number
	decoder := json.NewDecoder(bytes.NewReader(value))
	decoder.UseNumber()
	if decoder.Decode(&number) == nil {
		return string(value)
	}
	return ""
}

// read reads text as a project file (ProjectFile.Read, AC-53.9): one that does not say it is a REDIX project file, or is not laid out as one, a
// report with no date and time among it, answers notAProjectFile, and one of a version newer than this API reads newerVersion, both before
// anything else is read.
func read(text []byte) (file, string) {
	var header struct {
		Format  string `json:"format"`
		Version int    `json:"version"`
	}
	if json.Unmarshal(text, &header) != nil || header.Format != formatName || header.Version < 1 {
		return file{}, "notAProjectFile"
	}
	if header.Version > latestVersion {
		return file{}, "newerVersion"
	}
	var read file
	if json.Unmarshal(text, &read) != nil {
		return file{}, "notAProjectFile"
	}
	if _, timed := read.Report.at(zones.Riyadh); read.Report.Number > 0 && !timed {
		return file{}, "notAProjectFile"
	}
	return read, ""
}

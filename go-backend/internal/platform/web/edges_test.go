package web

import (
	"encoding/json"
	"testing"
)

func TestAnEnumWritesOnlyItsOwnValuesAndReadsOnlyItsOwnNamesOrNumbers(t *testing.T) {
	enum := Enum{"draft", "approved"}
	if _, err := enum.Write(2); err == nil {
		t.Error("a value past the names was written")
	}
	for _, data := range []string{"2", "-1", "true"} {
		if _, err := enum.Read([]byte(data)); err == nil {
			t.Errorf("%s was read", data)
		}
	}
	if value, err := enum.Read([]byte("1")); err != nil || value != 1 {
		t.Errorf("1 read as %d (%v)", value, err)
	}
}

func TestOrderedValuesReadOnlyAnObject(t *testing.T) {
	var ordered Ordered
	if json.Unmarshal([]byte(`["a"]`), &ordered) == nil {
		t.Fatal("a list read as the columns' values")
	}
}

func TestCamelLeavesNoNameAsNone(t *testing.T) {
	if Camel("") != "" || Camel("ApproveProjects") != "approveProjects" {
		t.Fatal(Camel(""), Camel("ApproveProjects"))
	}
}

// A required field may sit in an embedded struct; it is still required.
func TestAFieldOfAnEmbeddedStructIsRequiredToo(t *testing.T) {
	type named struct {
		Name string `json:"name"`
	}
	type request struct {
		named
		Note string `json:"note,omitzero"`
	}
	if hasEvery(map[string]json.RawMessage{"note": json.RawMessage(`"x"`)}, &request{}) {
		t.Fatal("the embedded name was not required")
	}
	if !hasEvery(map[string]json.RawMessage{"name": json.RawMessage(`"x"`)}, &request{}) {
		t.Fatal("the embedded name was not found")
	}
}

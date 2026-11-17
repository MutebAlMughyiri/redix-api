package changes

import (
	"database/sql"
	"errors"
	"net/http"
	"testing"
)

func TestARefusalSaysItsCode(t *testing.T) {
	refused := Refusal{Code: "unknownItem"}
	if refused.Error() != "changes: refused unknownItem" {
		t.Fatal(refused.Error())
	}
}

// Unknown refuses only a row that is not there; a failing database stays the failure it is.
func TestUnknownRefusesOnlyARowThatIsNotThere(t *testing.T) {
	var refused Refusal
	if !errors.As(Unknown(sql.ErrNoRows, "unknownItem"), &refused) || refused.Status != http.StatusBadRequest || refused.Code != "unknownItem" {
		t.Fatal("a row not there was not refused")
	}
	failure := errors.New("disk I/O error")
	if Unknown(failure, "unknownItem") != failure || Unknown(nil, "unknownItem") != nil {
		t.Fatal("a failure, or none, became a refusal")
	}
}

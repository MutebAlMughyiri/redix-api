package attachments

import (
	"strconv"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/google/uuid"
)

func TestMuteb16Nov_ALinkOpensItsFileForSevenDaysAndAChangedOrForeignLinkOpensNothing(t *testing.T) {
	now := clock.NewFake(time.Date(2026, 11, 16, 10, 0, 0, 0, time.UTC))
	links, file := NewLinks([]byte("test-key-for-go-backend-only-32b"), now), uuid.New()
	link := links.sign(file.String() + "|" + strconv.FormatInt(now.Now().Add(lifetime).Unix(), 10))
	if opened := links.AttachmentOf(link); opened != file {
		t.Fatalf("a fresh link opened %v, want %v", opened, file)
	}
	changed := []byte(link)
	changed[3] ^= 1
	if opened := links.AttachmentOf(string(changed)); opened != uuid.Nil {
		t.Errorf("a changed link opened %v", opened)
	}
	if opened := NewLinks([]byte("another-key-another-key-another!"), now).AttachmentOf(link); opened != uuid.Nil {
		t.Errorf("a link signed with another key opened %v", opened)
	}
	now.Advance(lifetime - time.Second)
	if opened := links.AttachmentOf(link); opened != file {
		t.Errorf("a link a second before its week ends opened %v", opened)
	}
	now.Advance(time.Second)
	if opened := links.AttachmentOf(link); opened != uuid.Nil {
		t.Errorf("a link a week old opened %v", opened)
	}
}

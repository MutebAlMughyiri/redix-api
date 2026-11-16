package attachments

import (
	"context"
	"crypto/hmac"
	"crypto/sha256"
	"encoding/base64"
	"net"
	"net/http"
	"strconv"
	"strings"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/google/uuid"
)

// lifetime is how long a link opens its file.
const lifetime = 7 * 24 * time.Hour

// Links makes and reads the links that open one kept file with no sign-in until they expire, a week after they are made: what a workbook's
// «المرفق» cell opens, since Excel cannot sign in (FileLinks, Muteb, 16 Nov 2026). The file's id and the moment it expires are signed with a key
// of their own, derived from the API's signing key as Data Protection derived one per purpose, so a link cannot be made up or kept alive; the
// moment is checked on the API's one clock. Links .NET made stop opening at the switch, each within a week (#140 G6).
type Links struct {
	key   []byte
	clock clock.Clock
}

// NewLinks is the links signed from signingKey, timed on clock.
func NewLinks(signingKey []byte, clock clock.Clock) Links {
	derived := hmac.New(sha256.New, signingKey)
	derived.Write([]byte("FileLinks"))
	return Links{key: derived.Sum(nil), clock: clock}
}

// OfEntries is the link to the own file of each of entryIDs that has one, its oldest (redix-app #115), by entry, on the address r reached the API
// at, expiring a week from now.
func (l Links) OfEntries(ctx context.Context, q *store.Queries, projectID uuid.UUID, entryIDs []uuid.UUID, r *http.Request) (map[uuid.UUID]string, error) {
	files, err := q.EntryFilesOf(ctx, projectID)
	links := map[uuid.UUID]string{}
	wanted := map[uuid.UUID]bool{}
	for _, id := range entryIDs {
		wanted[id] = true
	}
	expiresAt := strconv.FormatInt(l.clock.Now().Add(lifetime).Unix(), 10)
	for _, file := range files {
		if _, made := links[file.EntryID]; wanted[file.EntryID] && !made {
			links[file.EntryID] = schemeOf(r) + "://" + r.Host + "/files/" + l.sign(file.AttachmentID.String()+"|"+expiresAt)
		}
	}
	return links, err
}

// AttachmentOf is the file link opens; uuid.Nil for one expired, changed or never made.
func (l Links) AttachmentOf(link string) uuid.UUID {
	payload, signature, found := strings.Cut(link, ".")
	given, err := base64.RawURLEncoding.DecodeString(signature)
	if !found || err != nil || !hmac.Equal(given, l.signatureOf(payload)) {
		return uuid.Nil
	}
	signed, err := base64.RawURLEncoding.DecodeString(payload)
	id, expiresAt, _ := strings.Cut(string(signed), "|")
	expires, expiresErr := strconv.ParseInt(expiresAt, 10, 64)
	attachment, idErr := uuid.Parse(id)
	if err != nil || expiresErr != nil || idErr != nil || l.clock.Now().Unix() >= expires {
		return uuid.Nil
	}
	return attachment
}

func (l Links) sign(text string) string {
	payload := base64.RawURLEncoding.EncodeToString([]byte(text))
	return payload + "." + base64.RawURLEncoding.EncodeToString(l.signatureOf(payload))
}

func (l Links) signatureOf(payload string) []byte {
	mac := hmac.New(sha256.New, l.key)
	mac.Write([]byte(payload))
	return mac.Sum(nil)
}

// schemeOf is the scheme the person reached the API on: behind Caddy, which speaks to the binary on this machine alone, the one Caddy saw.
func schemeOf(r *http.Request) string {
	if r.TLS != nil {
		return "https"
	}
	host, _, err := net.SplitHostPort(r.RemoteAddr)
	if ip := net.ParseIP(host); err == nil && ip != nil && ip.IsLoopback() && r.Header.Get("X-Forwarded-Proto") != "" {
		return r.Header.Get("X-Forwarded-Proto")
	}
	return "http"
}

// Package email sends what REDIX emails - a code to reset a password or to confirm an email - in the asker's language (redix-api #127). Sending
// waits on the network or a file, so it runs after the write that made the code has committed (R14).
package email

import (
	"context"
	"fmt"
	"os"
	"path/filepath"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/email/store"
	"github.com/google/uuid"
)

// Sender delivers one email.
type Sender interface {
	Deliver(ctx context.Context, to, subject, body string) error
}

// Folder keeps each email as a text file instead of sending it, while Azure Communication Services is not set up (FolderEmailSender).
type Folder struct {
	Path string
}

// Deliver writes the email to its own file in the folder.
func (f Folder) Deliver(_ context.Context, to, subject, body string) error {
	if err := os.MkdirAll(f.Path, 0o755); err != nil {
		return err
	}
	name := strings.ReplaceAll(uuid.Must(uuid.NewV7()).String(), "-", "") + ".txt"
	return os.WriteFile(filepath.Join(f.Path, name), []byte("To: "+to+"\nSubject: "+subject+"\n\n"+body+"\n"), 0o644)
}

// Send writes the email whose words are subjectKey and bodyKey in language, with code in the body's {0}, and delivers it to to (EmailSender.SendAsync).
func Send(ctx context.Context, database db.DB, sender Sender, to, language, subjectKey, bodyKey, code string) error {
	rows, err := store.New(database.Read).WordsOf(ctx, store.WordsOfParams{Language: language, SubjectKey: subjectKey, BodyKey: bodyKey})
	if err != nil {
		return err
	}
	words := map[string]string{}
	for _, row := range rows {
		words[row.Key] = row.Value
	}
	if words[subjectKey] == "" || words[bodyKey] == "" {
		return fmt.Errorf("email: no words %s and %s in %s", subjectKey, bodyKey, language)
	}
	return sender.Deliver(ctx, to, words[subjectKey], strings.ReplaceAll(words[bodyKey], "{0}", code))
}

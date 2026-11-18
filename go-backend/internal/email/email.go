// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package email sends what REDIX emails - a code to reset a password or to confirm an email - in the asker's language (redix-api #127). Sending
// waits on the network or a file, so it runs after the write that made the code has committed (R14).
package email

import (
	"context"
	"encoding/base64"
	"fmt"
	"mime"
	"net/smtp"
	"os"
	"path/filepath"
	"strings"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/google/uuid"
)

// Sender delivers one email.
type Sender interface {
	Deliver(ctx context.Context, to, subject, body string) error
}

// SenderOf is the sender the settings name: the mail server at smtpAddress when one is named, otherwise the folder, as on Muteb's PC.
func SenderOf(smtpAddress, from, folder string) Sender {
	if smtpAddress != "" {
		return Smtp{Address: smtpAddress, From: from}
	}
	return Folder{Path: folder}
}

// Smtp hands each email to the mail server at Address, from From: on the published server, Postfix on that same machine, which sends it on
// (Muteb, 17 Nov 2026, redix-api #127 and #145). It listens there alone, so it asks for no password and offers no TLS.
type Smtp struct {
	Address, From string
}

// Deliver sends the email as UTF-8 text, its subject encoded so an Arabic one arrives whole.
func (s Smtp) Deliver(_ context.Context, to, subject, body string) error {
	encoded := base64.StdEncoding.EncodeToString([]byte(body))
	var lines []string
	for len(encoded) > 76 {
		lines, encoded = append(lines, encoded[:76]), encoded[76:]
	}
	lines = append(lines, encoded)
	message := "From: " + s.From + "\r\nTo: " + to + "\r\nSubject: " + mime.QEncoding.Encode("utf-8", subject) + "\r\nDate: " +
		time.Now().UTC().Format(time.RFC1123Z) + "\r\nMIME-Version: 1.0\r\nContent-Type: text/plain; charset=utf-8\r\n" +
		"Content-Transfer-Encoding: base64\r\n\r\n" + strings.Join(lines, "\r\n") + "\r\n"
	return smtp.SendMail(s.Address, nil, s.From, []string{to}, []byte(message))
}

// Folder keeps each email as a text file instead of sending it, where no mail server is named (FolderEmailSender).
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
	rows, err := database.Queries().WordsOf(ctx, store.WordsOfParams{Language: language, SubjectKey: subjectKey, BodyKey: bodyKey})
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

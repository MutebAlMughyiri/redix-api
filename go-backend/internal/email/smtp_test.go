package email

import (
	"bufio"
	"context"
	"encoding/base64"
	"io"
	"mime"
	"net"
	"net/mail"
	"strings"
	"testing"
)

// mailServer is the least of an SMTP server, on a free port of this machine: it takes one email and hands back what it was given.
func mailServer(t *testing.T) (string, <-chan string) {
	t.Helper()
	listener, err := net.Listen("tcp", "127.0.0.1:0")
	if err != nil {
		t.Fatal(err)
	}
	t.Cleanup(func() { listener.Close() })
	received := make(chan string, 1)
	go func() {
		connection, err := listener.Accept()
		if err != nil {
			return
		}
		defer connection.Close()
		reader, say := bufio.NewReader(connection), func(line string) { io.WriteString(connection, line+"\r\n") }
		say("220 test")
		var data strings.Builder
		for {
			line, err := reader.ReadString('\n')
			if err != nil {
				return
			}
			switch command := strings.ToUpper(strings.TrimSpace(line)); {
			case strings.HasPrefix(command, "EHLO"), strings.HasPrefix(command, "HELO"):
				say("250 test")
			case command == "DATA":
				say("354 go on")
				for {
					line, err := reader.ReadString('\n')
					if err != nil || line == ".\r\n" {
						break
					}
					data.WriteString(line)
				}
				received <- data.String()
				say("250 kept")
			case command == "QUIT":
				say("221 bye")
				return
			default:
				say("250 ok")
			}
		}
	}()
	return listener.Addr().String(), received
}

// R127, decided by Muteb on 17 Nov 2026: on the published server each email is handed to the mail server on that machine, as UTF-8, so an Arabic
// code email arrives as it was written.
func TestR127_AnEmailIsHandedToTheMailServerWhole(t *testing.T) {
	address, received := mailServer(t)
	sender := SenderOf(address, "no-reply@redix.example", "unused")
	subject, body := "رمز إعادة تعيين كلمة المرور", "رمزك هو 123456 وهو صالح لمدة خمس عشرة دقيقة، ولا تشاركه مع أحد."
	if err := sender.Deliver(context.Background(), "manager@example.com", subject, body); err != nil {
		t.Fatal(err)
	}
	message, err := mail.ReadMessage(strings.NewReader(<-received))
	if err != nil {
		t.Fatal(err)
	}
	decodedSubject, _ := new(mime.WordDecoder).DecodeHeader(message.Header.Get("Subject"))
	raw, _ := io.ReadAll(message.Body)
	decodedBody, _ := base64.StdEncoding.DecodeString(strings.ReplaceAll(string(raw), "\r\n", ""))
	if message.Header.Get("From") != "no-reply@redix.example" || message.Header.Get("To") != "manager@example.com" || decodedSubject != subject ||
		string(decodedBody) != body || !strings.Contains(message.Header.Get("Content-Type"), "charset=utf-8") {
		t.Errorf("the mail server got %q from %q to %q: %q", decodedSubject, message.Header.Get("From"), message.Header.Get("To"), decodedBody)
	}
}

func TestR127_NoMailServerListeningIsAFailureToSend(t *testing.T) {
	listener, err := net.Listen("tcp", "127.0.0.1:0")
	if err != nil {
		t.Fatal(err)
	}
	address := listener.Addr().String()
	listener.Close()
	if err := SenderOf(address, "no-reply@redix.example", "").Deliver(context.Background(), "manager@example.com", "s", "b"); err == nil {
		t.Fatal("an email went to a mail server that is not there")
	}
}

func TestR127_WithNoMailServerNamedEachEmailIsKeptInTheFolder(t *testing.T) {
	if sender := SenderOf("", "", "Emails"); sender != (Folder{Path: "Emails"}) {
		t.Fatalf("with no mail server the sender is %#v", sender)
	}
}

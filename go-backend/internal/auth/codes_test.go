package auth_test

import (
	"testing"
	"time"
)

// withEmail gives the test user an email, as confirming one would.
func (a api) withEmail(address string) { a.Exec("UPDATE users SET email = ?", address) }

// lastCode is the six digits of the latest email sent to address.
func (a api) lastCode(address string) string {
	a.t.Helper()
	code := a.Emails.LastCodeTo(address)
	if code == "" {
		a.t.Fatalf("no code was emailed to %s", address)
	}
	return code
}

func (a api) emailCount() int { return a.Emails.Count() }

func TestAC02_8_AForgottenPasswordIsResetWithTheEmailedCode(t *testing.T) {
	a := newAPI(t, false)
	a.withEmail("manager@example.com")
	other := a.session()["accessToken"].(string)
	if code, _ := a.post("/auth/forgot-password", `{"login":"manager@example.com"}`, ""); code != 202 {
		t.Fatalf("forgot: %d", code)
	}
	typed := a.lastCode("manager@example.com")
	if code, answer := a.post("/auth/reset-with-code", `{"login":"Manager","code":" `+typed+` ","newPassword":"Reset-password-4"}`, ""); code != 204 {
		t.Fatalf("reset: %d %v", code, answer)
	}
	if code, _ := a.signIn("manager", "Reset-password-4"); code != 200 {
		t.Errorf("the new password: %d", code)
	}
	if code, _ := a.post("/auth/sign-out", ``, other); code != 401 {
		t.Errorf("a device from before the reset: %d", code)
	}
	if code, answer := a.post("/auth/reset-with-code", `{"login":"manager","code":"`+typed+`","newPassword":"Reset-password-5"}`, ""); code != 400 || answer["code"] != "codeWrong" {
		t.Errorf("the code again: %d %v", code, answer)
	}
}

func TestAC02_8_AskingForAnAccountThatDoesNotExistAnswersTheSame(t *testing.T) {
	a := newAPI(t, false)
	if code, _ := a.post("/auth/forgot-password", `{"login":"nobody"}`, ""); code != 202 || a.emailCount() != 0 {
		t.Errorf("%d, %d emails", code, a.emailCount())
	}
}

func TestAC02_8_ACodeHasFiveTriesAndFifteenMinutes(t *testing.T) {
	a := newAPI(t, false)
	a.withEmail("manager@example.com")
	a.post("/auth/forgot-password", `{"login":"manager"}`, "")
	typed := a.lastCode("manager@example.com")
	for i := 1; i <= 5; i++ {
		a.post("/auth/reset-with-code", `{"login":"manager","code":"000000","newPassword":"Reset-password-4"}`, "")
	}
	if code, answer := a.post("/auth/reset-with-code", `{"login":"manager","code":"`+typed+`","newPassword":"Reset-password-4"}`, ""); code != 400 || answer["code"] != "codeWrong" {
		t.Errorf("after five tries: %d %v", code, answer)
	}
	a.post("/auth/forgot-password", `{"login":"manager"}`, "")
	fresh := a.lastCode("manager@example.com")
	a.Clock.Advance(15 * time.Minute)
	if code, answer := a.post("/auth/reset-with-code", `{"login":"manager","code":"`+fresh+`","newPassword":"Reset-password-4"}`, ""); code != 400 || answer["code"] != "codeWrong" {
		t.Errorf("after 15 minutes: %d %v", code, answer)
	}
}

func TestAC02_8_OneLoginAsksFiveTimesAnHour(t *testing.T) {
	a := newAPI(t, false)
	for i := 1; i <= 5; i++ {
		if code, _ := a.post("/auth/forgot-password", `{"login":"manager"}`, ""); code != 202 {
			t.Fatalf("ask %d: %d", i, code)
		}
	}
	if code, _ := a.post("/auth/forgot-password", `{"login":" MANAGER "}`, ""); code != 429 {
		t.Errorf("a sixth ask: %d", code)
	}
}

func TestAC10_4_ANewEmailIsTheUsersOnceItsCodeIsTyped(t *testing.T) {
	a := newAPI(t, false)
	token := a.session()["accessToken"].(string)
	if code, _ := a.post("/me/email", `{"email":" new@example.com "}`, token); code != 202 {
		t.Fatalf("ask: %d", code)
	}
	if code, answer := a.post("/me/email/confirm", `{"code":"`+a.lastCode("new@example.com")+`"}`, token); code != 204 {
		t.Fatalf("confirm: %d %v", code, answer)
	}
	if session := a.session(); session["email"] != "new@example.com" {
		t.Errorf("email %v", session["email"])
	}
}

func TestAC10_4_AnEmailThatIsNotOneOrIsTakenIsRefused(t *testing.T) {
	a := newAPI(t, false)
	token := a.session()["accessToken"].(string)
	if code, answer := a.post("/me/email", `{"email":"not an email"}`, token); code != 400 || answer["code"] != "emailInvalid" {
		t.Errorf("invalid: %d %v", code, answer)
	}
	a.Exec(`INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at, email)
		VALUES ('0193a1b2-0000-7000-8000-000000000002', 'other', 'Other', '', '', 0, 0, '1900-01-01T00:00:00.0000000Z', '2026-11-01T10:00:00.0000000Z', 'taken@example.com')`)
	if code, answer := a.post("/me/email", `{"email":"TAKEN@example.com"}`, token); code != 409 || answer["code"] != "emailTaken" {
		t.Errorf("taken: %d %v", code, answer)
	}
}

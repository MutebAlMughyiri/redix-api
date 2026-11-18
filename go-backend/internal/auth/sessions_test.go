// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package auth_test

import (
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
)

const password = "Patrol-chosen-2"

// api is the API through testapi, on a fresh database holding "Manager", whose password is password.
type api struct {
	*testapi.API
	t *testing.T
}

func newAPI(t *testing.T, mustChange bool) api {
	t.Helper()
	a := api{API: testapi.New(t), t: t}
	a.Exec(`INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at, email)
		VALUES ('0193a1b2-0000-7000-8000-000000000001', 'Manager', 'مدير المشروع', '', ?, ?, 0, '1900-01-01T00:00:00.0000000Z', '2026-11-01T10:00:00.0000000Z', '')`,
		credentials.Hash(password), mustChange)
	return a
}

func (a api) post(path, body, bearer string) (int, map[string]any) {
	a.t.Helper()
	code, answer := a.Ask("POST", path, body, bearer)
	object, _ := answer.(map[string]any)
	if object == nil {
		object = map[string]any{}
	}
	return code, object
}

func (a api) signIn(username, typed string) (int, map[string]any) {
	return a.post("/auth/sign-in", `{"username":"`+username+`","password":"`+typed+`"}`, "")
}

func (a api) session() map[string]any {
	a.t.Helper()
	code, answer := a.signIn("manager", password)
	if code != 200 {
		a.t.Fatalf("sign-in: %d %v", code, answer)
	}
	return answer
}

func TestAC01_1_SigningInOpensASessionAndRecordsIt(t *testing.T) {
	a := newAPI(t, false)
	answer := a.session()
	if answer["username"] != "Manager" || answer["displayName"] != "مدير المشروع" || answer["mustChangePassword"] != false || answer["defaultProjectId"] != "" ||
		answer["accessTokenExpiresAt"] != "2026-11-16T10:15:00Z" || len(answer["refreshToken"].(string)) != 43 || answer["usesBiometrics"] != false {
		t.Errorf("%v", answer)
	}
	if permissions, ok := answer["permissions"].([]any); !ok || len(permissions) != 0 {
		t.Errorf("permissions %v", answer["permissions"])
	}
	if got := a.Count("SELECT count(*) FROM history WHERE action = 'SignIn'"); got != 1 {
		t.Errorf("history lines: %d", got)
	}
}

func TestAC01_1_AWrongUsernameAndAWrongPasswordAnswerTheSame(t *testing.T) {
	a := newAPI(t, false)
	for _, typed := range [][2]string{{"nobody", password}, {"manager", "Wrong-password-1"}} {
		if code, answer := a.signIn(typed[0], typed[1]); code != 401 || answer["code"] != "invalidCredentials" {
			t.Errorf("%v: %d %v", typed, code, answer)
		}
	}
}

func TestAC01_2_FiveWrongPasswordsLockTheAccountForFifteenMinutes(t *testing.T) {
	a := newAPI(t, false)
	for i := 1; i <= 5; i++ {
		if code, answer := a.signIn("manager", "Wrong-password-1"); code != 401 || answer["code"] != "invalidCredentials" {
			t.Fatalf("wrong %d: %d %v", i, code, answer)
		}
	}
	if code, answer := a.signIn("manager", password); code != 401 || answer["code"] != "accountLocked" {
		t.Errorf("locked: %d %v", code, answer)
	}
	a.Clock.Advance(15 * time.Minute)
	if code, _ := a.signIn("manager", password); code != 200 {
		t.Errorf("after 15 minutes: %d", code)
	}
	if got := a.Count("SELECT count(*) FROM history WHERE action = 'AccountLocked'"); got != 1 {
		t.Errorf("AccountLocked lines: %d", got)
	}
}

func TestR29_AUsernameSignsInWhateverItsCase(t *testing.T) {
	if code, _ := newAPI(t, false).signIn("MANAGER", password); code != 200 {
		t.Errorf("%d", code)
	}
}

func TestNFR09_ARefreshTokenRenewsOnceAndLastsTwelveHoursUnused(t *testing.T) {
	a := newAPI(t, false)
	first := a.session()["refreshToken"].(string)
	code, renewed := a.post("/auth/refresh", `{"refreshToken":"`+first+`"}`, "")
	if code != 200 || renewed["refreshToken"] == first {
		t.Fatalf("renewal: %d %v", code, renewed)
	}
	if code, answer := a.post("/auth/refresh", `{"refreshToken":"`+first+`"}`, ""); code != 401 || answer["code"] != "sessionExpired" {
		t.Errorf("the replaced token: %d %v", code, answer)
	}
	a.Clock.Advance(12 * time.Hour)
	if code, answer := a.post("/auth/refresh", `{"refreshToken":"`+renewed["refreshToken"].(string)+`"}`, ""); code != 401 || answer["code"] != "sessionExpired" {
		t.Errorf("after 12 hours: %d %v", code, answer)
	}
}

func TestUC011_APhoneThatUsesBiometricsStaysSignedInPastTwelveHours(t *testing.T) {
	a := newAPI(t, false)
	code, answer := a.post("/auth/biometrics", `{"on":true}`, a.session()["accessToken"].(string))
	if code != 200 || answer["usesBiometrics"] != true {
		t.Fatalf("%d %v", code, answer)
	}
	a.Clock.Advance(13 * time.Hour)
	if code, _ := a.post("/auth/refresh", `{"refreshToken":"`+answer["refreshToken"].(string)+`"}`, ""); code != 200 {
		t.Errorf("after 13 hours: %d", code)
	}
}

func TestUC003_ChangingThePasswordKeepsThisPhoneAndSignsOutTheOthers(t *testing.T) {
	a := newAPI(t, true)
	other := a.session()["accessToken"].(string)
	this := a.session()["accessToken"].(string)
	if code, _ := a.post("/auth/biometrics", `{"on":true}`, this); code != 403 {
		t.Errorf("a temporary password reached a working route: %d", code)
	}
	code, answer := a.post("/auth/change-password", `{"currentPassword":"`+password+`","newPassword":"Chosen-password-3"}`, this)
	if code != 200 || answer["mustChangePassword"] != false {
		t.Fatalf("%d %v", code, answer)
	}
	if code, _ := a.post("/auth/sign-out", ``, other); code != 401 {
		t.Errorf("the other phone: %d", code)
	}
	if code, _ := a.post("/auth/biometrics", `{"on":false}`, answer["accessToken"].(string)); code != 200 {
		t.Errorf("this phone's renewed session: %d", code)
	}
	if code, _ := a.signIn("manager", "Chosen-password-3"); code != 200 {
		t.Errorf("the new password: %d", code)
	}
}

func TestUC003_AWrongCurrentPasswordOrAWeakNewOneIsRefused(t *testing.T) {
	a := newAPI(t, true)
	token := a.session()["accessToken"].(string)
	for body, want := range map[string]string{
		`{"currentPassword":"Wrong-password-1","newPassword":"Chosen-password-3"}`: "invalidCredentials",
		`{"currentPassword":"` + password + `","newPassword":"short1"}`:            "weakPassword",
		`{"currentPassword":"` + password + `","newPassword":"onlyletters"}`:       "weakPassword",
		`{"currentPassword":"` + password + `","newPassword":"` + password + `"}`:  "weakPassword",
	} {
		if code, answer := a.post("/auth/change-password", body, token); code != 400 || answer["code"] != want {
			t.Errorf("%s: %d %v", body, code, answer)
		}
	}
}

func TestUC062_SigningOutEndsThisSessionOnly(t *testing.T) {
	a := newAPI(t, false)
	other := a.session()["accessToken"].(string)
	this := a.session()["accessToken"].(string)
	if code, _ := a.post("/auth/sign-out", ``, this); code != 204 {
		t.Fatalf("sign-out: %d", code)
	}
	if code, _ := a.post("/auth/biometrics", `{"on":false}`, this); code != 401 {
		t.Errorf("the signed-out session: %d", code)
	}
	if code, _ := a.post("/auth/biometrics", `{"on":false}`, other); code != 200 {
		t.Errorf("the other session: %d", code)
	}
}

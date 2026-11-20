// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web_test

import (
	"compress/gzip"
	"io"
	"net/http"
	"net/url"
	"regexp"
	"strings"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
)

// password is the password testapi.Person gives everyone.
const password = "Patrol-chosen-2"

// signedIn is a browser in which name has signed in, and their username.
func signedIn(t *testing.T, api *testapi.API, name string) (*testapi.Browser, string) {
	t.Helper()
	id, _ := api.Person(name)
	browser := api.Browser()
	answered := browser.Send("POST", "/sign-in", url.Values{"username": {id.String()}, "password": {password}})
	if answered.Code != http.StatusOK {
		t.Fatalf("sign-in: %d %s", answered.Code, answered.Body)
	}
	return browser, id.String()
}

func has(t *testing.T, body, want string) {
	t.Helper()
	if !strings.Contains(body, want) {
		t.Errorf("the answer lacks %q:\n%s", want, body)
	}
}

func lacks(t *testing.T, body, unwanted string) {
	t.Helper()
	if strings.Contains(body, unwanted) {
		t.Errorf("the answer has %q:\n%s", unwanted, body)
	}
}

func TestWeb_R1_TheRightPasswordOpensTheShellWithItsMotion(t *testing.T) {
	api := testapi.New(t)
	browser, _ := signedIn(t, api, "User A")
	answered := browser.Send("GET", "/", nil)
	has(t, answered.Body.String(), `class="shell in"`)
	has(t, answered.Body.String(), "User A")
	browser2 := api.Browser()
	id, _ := api.Person("User B")
	signing := browser2.Send("POST", "/sign-in", url.Values{"username": {id.String()}, "password": {password}})
	if signing.Header().Get("HX-Retarget") != "#shell" || signing.Header().Get("HX-Reswap") != "outerHTML" {
		t.Errorf("signing in answers the whole shell in place: %v", signing.Header())
	}
	has(t, signing.Body.String(), `class="shell in arriving"`)
	has(t, signing.Body.String(), `data-key="myAccount.signOut"`)
}

func TestWeb_R1_AWrongPasswordAnswersTheFormWithTheAppsWords(t *testing.T) {
	api := testapi.New(t)
	id, _ := api.Person("User A")
	answered := api.Browser().Send("POST", "/sign-in", url.Values{"username": {id.String()}, "password": {"wrong-1"}})
	if answered.Code != http.StatusUnprocessableEntity {
		t.Fatalf("a refusal answers 422, which htmx is told to swap: %d", answered.Code)
	}
	has(t, answered.Body.String(), "اسم المستخدم أو كلمة المرور غير صحيحة.")
	has(t, answered.Body.String(), `value="`+id.String()+`"`)
	unknown := api.Browser().Send("POST", "/sign-in", url.Values{"username": {"nobody"}, "password": {"wrong-1"}})
	has(t, unknown.Body.String(), "اسم المستخدم أو كلمة المرور غير صحيحة.")
}

func TestWeb_R1_TheFifthWrongPasswordLocksAsOnThePhone(t *testing.T) {
	api := testapi.New(t)
	id, _ := api.Person("User A")
	browser := api.Browser()
	for range 5 {
		browser.Send("POST", "/sign-in", url.Values{"username": {id.String()}, "password": {"wrong-1"}})
	}
	answered := browser.Send("POST", "/sign-in", url.Values{"username": {id.String()}, "password": {password}})
	if answered.Code != http.StatusUnprocessableEntity {
		t.Fatalf("a locked account refuses the right password: %d", answered.Code)
	}
	has(t, answered.Body.String(), "محاولات خاطئة كثيرة. حاول مرة أخرى لاحقاً.")
}

func TestWeb_R2_TheSignInFormHasWhatTheAppsHas(t *testing.T) {
	body := testapi.New(t).Browser().Send("GET", "/", nil).Body.String()
	for _, key := range []string{"signIn.username", "signIn.password", "signIn.submit", "signIn.registerProject", "auth.forgotPassword", "signIn.message",
		"appVersion.line", "myAccount.arabic", "myAccount.english"} {
		has(t, body, `data-key="`+key+`"`)
	}
	has(t, body, "الإصدار 1.0.0")
	has(t, body, `autocomplete="username"`)
	has(t, body, `autocomplete="current-password"`)
	has(t, body, "autofocus")
	lacks(t, body, `class="side"`)
}

func TestWeb_R3_TheFirstOpenWeighsUnder100KBWithHtmxTheOneScript(t *testing.T) {
	browser := testapi.New(t).Browser()
	page := browser.Send("GET", "/", nil, "Accept-Encoding", "gzip").Body.String()
	if count := strings.Count(page, "<script"); count != 1 || strings.Contains(page, "<img") {
		t.Errorf("htmx is the one script and the logo the one picture: %d scripts", count)
	}
	has(t, page, `<link rel="icon" href="data:image/svg+xml,`)
	weight := len(page)
	for _, address := range regexp.MustCompile(`/assets/[a-z.]+\.[0-9a-f]{8}\.(css|js)`).FindAllString(page, -1) {
		asset := browser.Send("GET", address, nil, "Accept-Encoding", "gzip")
		if asset.Header().Get("Content-Encoding") != "gzip" || !strings.Contains(asset.Header().Get("Cache-Control"), "immutable") {
			t.Errorf("%s is sent gzipped and kept a year: %v", address, asset.Header())
		}
		weight += asset.Body.Len()
	}
	if weight > 100*1024 || weight < 10*1024 {
		t.Errorf("the first open weighs %d bytes", weight)
	}
}

func TestWeb_R3_AnAssetUnzippedIsTheFileItself(t *testing.T) {
	browser := testapi.New(t).Browser()
	page := browser.Send("GET", "/", nil).Body.String()
	address := regexp.MustCompile(`/assets/redix\.[0-9a-f]{8}\.css`).FindString(page)
	zipped := browser.Send("GET", address, nil, "Accept-Encoding", "gzip")
	reader, err := gzip.NewReader(zipped.Body)
	if err != nil {
		t.Fatal(err)
	}
	unzipped, _ := io.ReadAll(reader)
	plain := browser.Send("GET", address, nil)
	if string(unzipped) != plain.Body.String() || !strings.HasPrefix(plain.Header().Get("Content-Type"), "text/css") {
		t.Errorf("the stylesheet differs gzipped, or is not CSS: %s", plain.Header().Get("Content-Type"))
	}
	if browser.Send("GET", "/assets/redix.00000000.css", nil).Code != http.StatusNotFound {
		t.Error("an address no file has answers 404")
	}
}

func TestWeb_R5_ATabAnswersItsContentAloneTheFrameStays(t *testing.T) {
	browser, _ := signedIn(t, testapi.New(t), "User A")
	answered := browser.Send("GET", "/tab/settings", nil)
	body := answered.Body.String()
	lacks(t, body, `id="shell"`)
	has(t, body, `<h2>الإعدادات</h2>`)
	has(t, body, `id="tabs" class="tabs" role="tablist" hx-swap-oob="true"`)
	has(t, body, `aria-current="true" data-key="sharedSettings.title" hx-swap-oob="true">الإعدادات</a>`)
	lacks(t, body, `class="tree"`)
	closed := browser.Send("DELETE", "/tab/settings", nil).Body.String()
	has(t, closed, "اختر من القائمة الجانبية لتبدأ.")
	lacks(t, closed, `class="tab-title"`)
	if browser.Send("GET", "/tab/entries", nil).Code != http.StatusNotFound {
		t.Error("an item whose feature is not built opens no tab")
	}
}

func TestWeb_R7_TheSideMenuFoldsWithAKeyboardCharacterAndAlt1(t *testing.T) {
	browser, _ := signedIn(t, testapi.New(t), "User A")
	body := browser.Send("GET", "/", nil).Body.String()
	has(t, body, `<label class="fold" for="view-side" data-tip="طيّ القائمة `+"\u2068(Alt+1)\u2069"+`">»</label>`)
	has(t, body, `<label class="unfold" for="view-side" data-tip="إظهار القائمة `+"\u2068(Alt+1)\u2069"+`">«</label>`)
	if strings.Count(body, "Digit1") != 1 || strings.Contains(body, "accesskey") {
		t.Error("Alt+1 has one handler, on the page's body")
	}
	browser.Send("GET", "/language/en", nil)
	english := browser.Send("GET", "/", nil).Body.String()
	has(t, english, `data-tip="Collapse `+"\u2068(Alt+1)\u2069"+`">«</label>`)
}

func TestWeb_R9_R18_TheViewMenuHidesTheBarsAndTheServerDrawsThemHidden(t *testing.T) {
	browser, _ := signedIn(t, testapi.New(t), "User A")
	if answered := browser.Send("POST", "/view", url.Values{"side": {"on"}, "width": {"300"}}); answered.Code != http.StatusNoContent {
		t.Fatalf("keeping the view draws nothing: %d", answered.Code)
	}
	body := browser.Send("GET", "/", nil).Body.String()
	has(t, body, `<input type="checkbox" id="view-top" name="top">`)
	has(t, body, `<input type="checkbox" id="view-status" name="status">`)
	has(t, body, `<input type="checkbox" id="view-side" name="side" checked>`)
	has(t, body, `style="--side-w: 300px;"`)
	browser.Send("POST", "/view", url.Values{"top": {"on"}, "width": {"9000"}})
	has(t, browser.Send("GET", "/", nil).Body.String(), `style="--side-w: 480px;"`)
}

func TestWeb_R11_TheLanguageIsInSettingsNotTheTopBar(t *testing.T) {
	browser, _ := signedIn(t, testapi.New(t), "User A")
	page := browser.Send("GET", "/", nil).Body.String()
	lacks(t, page, `href="/language/`)
	settings := browser.Send("GET", "/tab/settings", nil).Body.String()
	has(t, settings, `<h3>اللغة</h3>`)
	has(t, settings, `href="/language/en"`)
}

func TestWeb_R12_R20_TheSignInFormCarriesTheLanguageAndEnglishReadsLeftToRight(t *testing.T) {
	browser := testapi.New(t).Browser()
	arabic := browser.Send("GET", "/", nil).Body.String()
	has(t, arabic, `<html lang="ar" dir="rtl">`)
	has(t, arabic, `href="/language/en"`)
	chose := browser.Send("GET", "/language/en", nil)
	if chose.Code != http.StatusSeeOther || chose.Header().Get("Location") != "/" {
		t.Errorf("choosing a language opens the page again: %d %v", chose.Code, chose.Header())
	}
	english := browser.Send("GET", "/", nil).Body.String()
	has(t, english, `<html lang="en" dir="ltr">`)
	has(t, english, ">Sign in</button>")
	if browser.Send("GET", "/language/fr", nil).Code != http.StatusNotFound {
		t.Error("a language REDIX has no words for is refused")
	}
	asked := testapi.New(t).Browser().Send("GET", "/", nil, "Accept-Language", "en-GB,en;q=0.9")
	has(t, asked.Body.String(), `<html lang="en" dir="ltr">`)
}

func TestWeb_R13_ATemporaryPasswordIsChangedBeforeAnythingElse(t *testing.T) {
	api := testapi.New(t)
	id, _ := api.Person("User A")
	api.Exec(`UPDATE users SET must_change_password = 1 WHERE id = ?`, id)
	browser := api.Browser()
	signing := browser.Send("POST", "/sign-in", url.Values{"username": {id.String()}, "password": {password}}).Body.String()
	has(t, signing, `data-key="changePassword.current"`)
	lacks(t, signing, `class="side"`)
	if browser.Send("GET", "/tab/settings", nil).Header().Get("HX-Retarget") != "#shell" {
		t.Error("a tab answers the change-password form in the whole shell while the password is temporary")
	}
	mismatch := browser.Send("POST", "/change-password", url.Values{"current": {password}, "new": {"Chosen-new-3"}, "confirm": {"Chosen-new-4"}})
	if mismatch.Code != http.StatusUnprocessableEntity {
		t.Fatalf("two new passwords that differ are refused: %d", mismatch.Code)
	}
	has(t, mismatch.Body.String(), "كلمتا المرور الجديدتان غير متطابقتين.")
	weak := browser.Send("POST", "/change-password", url.Values{"current": {password}, "new": {"short"}, "confirm": {"short"}})
	has(t, weak.Body.String(), "استخدم 8 أحرف على الأقل")
	wrong := browser.Send("POST", "/change-password", url.Values{"current": {"wrong-1"}, "new": {"Chosen-new-3"}, "confirm": {"Chosen-new-3"}})
	has(t, wrong.Body.String(), "كلمة المرور الحالية غير صحيحة.")
	changed := browser.Send("POST", "/change-password", url.Values{"current": {password}, "new": {"Chosen-new-3"}, "confirm": {"Chosen-new-3"}})
	has(t, changed.Body.String(), `class="shell in arriving"`)
	has(t, browser.Send("GET", "/", nil).Body.String(), `class="shell in"`)
}

func TestWeb_R14_R15_WhatIsNotBuiltStandsGray(t *testing.T) {
	api := testapi.New(t)
	signedOut := api.Browser().Send("GET", "/", nil).Body.String()
	has(t, signedOut, `<span aria-disabled="true" data-key="signIn.registerProject">`)
	has(t, signedOut, `<span aria-disabled="true" data-key="auth.forgotPassword">`)
	browser, _ := signedIn(t, api, "User A")
	page := browser.Send("GET", "/", nil).Body.String()
	for _, key := range []string{"web.home", "entries.title", "approvals.title", "accounts.title", "partners.title", "contracts.title", "reports.title",
		"history.projectTitle"} {
		has(t, page, `<span class="parked" aria-disabled="true" data-key="`+key+`">`)
	}
	has(t, page, `hx-get="/tab/settings"`)
}

func TestWeb_R16_SignOutEndsThisBrowserAndClearsItsCookies(t *testing.T) {
	api := testapi.New(t)
	browser, _ := signedIn(t, api, "User A")
	open := `SELECT count(*) FROM devices WHERE is_revoked = 0`
	before := api.Count(open)
	answered := browser.Send("POST", "/sign-out", nil)
	has(t, answered.Body.String(), `data-key="signIn.username"`)
	if browser.Cookie("redix_access") != "" || browser.Cookie("redix_refresh") != "" {
		t.Error("signing out clears the session's cookies")
	}
	if after := api.Count(open); after != before-1 {
		t.Errorf("signing out closes this browser's device, and only it: %d open, then %d", before, after)
	}
	page := browser.Send("GET", "/", nil).Body.String()
	lacks(t, page, "انتهت الجلسة")
}

func TestWeb_R16_AnExpiredAccessTokenIsRenewedInsideTheRequest(t *testing.T) {
	api := testapi.New(t)
	browser, _ := signedIn(t, api, "User A")
	before := browser.Cookie("redix_access")
	api.Clock.Advance(20 * time.Minute)
	has(t, browser.Send("GET", "/", nil).Body.String(), `class="shell in"`)
	if browser.Cookie("redix_access") == before || browser.Cookie("redix_access") == "" {
		t.Error("the access token is renewed and kept")
	}
}

func TestWeb_R16_TwelveHoursUnusedEndTheSession(t *testing.T) {
	api := testapi.New(t)
	browser, _ := signedIn(t, api, "User A")
	api.Clock.Advance(13 * time.Hour)
	page := browser.Send("GET", "/", nil).Body.String()
	has(t, page, "انتهت الجلسة. سجّل الدخول مرة أخرى.")
	if browser.Cookie("redix_refresh") != "" {
		t.Error("an ended session's cookies are cleared")
	}
	if browser.Send("GET", "/tab/settings", nil).Header().Get("HX-Retarget") != "#shell" {
		t.Error("a tab asked with no session answers the sign-in form in the whole shell")
	}
}

func TestWeb_R17_TheSessionsCookiesAreHiddenFromScriptAndOtherSites(t *testing.T) {
	api := testapi.New(t)
	id, _ := api.Person("User A")
	signing := api.Browser().Send("POST", "/sign-in", url.Values{"username": {id.String()}, "password": {password}})
	for _, cookie := range signing.Result().Cookies() {
		if !cookie.HttpOnly || !cookie.Secure || cookie.SameSite != http.SameSiteStrictMode || cookie.Path != "/" {
			t.Errorf("%s is HttpOnly, Secure, SameSite=Strict on /: %+v", cookie.Name, cookie)
		}
	}
	elsewhere := api.Browser().Send("POST", "/sign-in", url.Values{"username": {id.String()}, "password": {password}}, "Origin", "https://elsewhere.example",
		"Sec-Fetch-Site", "cross-site")
	if elsewhere.Code != http.StatusForbidden {
		t.Errorf("a form another site posts is refused: %d", elsewhere.Code)
	}
}

func TestWeb_EveryPageAnswersAFailingDatabaseWith500(t *testing.T) {
	api := testapi.New(t)
	id, _ := api.Person("User A")
	for n := int64(1); ; n++ {
		answered, failed := api.Browser().SendFailing(n, "POST", "/sign-in", url.Values{"username": {id.String()}, "password": {password}})
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("signing in with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
	browser, _ := signedIn(t, api, "User B")
	for _, step := range []struct {
		method, path string
		form         url.Values
	}{
		{"GET", "/", nil}, {"GET", "/tab/settings", nil}, {"DELETE", "/tab/settings", nil},
		{"POST", "/change-password", url.Values{"current": {password}, "new": {"Chosen-new-3"}, "confirm": {"Chosen-new-3"}}}, {"POST", "/sign-out", nil},
	} {
		for n := int64(1); ; n++ {
			answered, failed := browser.SendFailing(n, step.method, step.path, step.form)
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s %s with the database failing at its call %d answers %d", step.method, step.path, n, answered.Code)
			}
		}
	}
}

func TestWeb_R5_ClosingATabNotOpenChangesNothing(t *testing.T) {
	browser, _ := signedIn(t, testapi.New(t), "User A")
	has(t, browser.Send("DELETE", "/tab/settings", nil).Body.String(), "اختر من القائمة الجانبية لتبدأ.")
}

func TestWeb_R13_ChangingAPasswordWithNoSessionAnswersTheSignInForm(t *testing.T) {
	answered := testapi.New(t).Browser().Send("POST", "/change-password", url.Values{"current": {password}, "new": {"Chosen-new-3"}, "confirm": {"Chosen-new-3"}})
	if answered.Header().Get("HX-Retarget") != "#shell" {
		t.Errorf("no session answers the whole shell: %v", answered.Header())
	}
	has(t, answered.Body.String(), `data-key="signIn.username"`)
}

// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web_test

import (
	"archive/zip"
	"bytes"
	"compress/gzip"
	"io"
	"net/http"
	"net/url"
	"os"
	"path/filepath"
	"regexp"
	"slices"
	"strconv"
	"strings"
	"testing"
	"time"

	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
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
	if signing.Header().Get("HX-Retarget") != "[id='web.shell']" || signing.Header().Get("HX-Reswap") != "outerHTML" {
		t.Errorf("signing in answers the whole shell in place: %v", signing.Header())
	}
	has(t, signing.Body.String(), `class="shell in arriving"`)
	has(t, signing.Body.String(), `id="myAccount.signOut"`)
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
		has(t, body, `id="`+key+`"`)
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
	has(t, body, `<h2 id="sharedSettings.title">الإعدادات</h2>`)
	has(t, body, `id="web.tabs" class="tabs" role="tablist" hx-swap-oob="true"`)
	has(t, body, `aria-current="true" hx-swap-oob="true">الإعدادات</a>`)
	lacks(t, body, `class="tree"`)
	has(t, browser.Send("DELETE", "/tab/settings", nil).Body.String(), `id="projects.title"`)
	closed := browser.Send("DELETE", "/tab/home", nil).Body.String()
	has(t, closed, "اختر من القائمة الجانبية لتبدأ.")
	lacks(t, closed, `class="tab-title"`)
	if browser.Send("GET", "/tab/nothing", nil).Code != http.StatusNotFound {
		t.Error("a tab no feature opens is not found")
	}
}

func TestWeb_R7_TheSideMenuFoldsWithAKeyboardCharacterAndAlt1(t *testing.T) {
	browser, _ := signedIn(t, testapi.New(t), "User A")
	body := browser.Send("GET", "/", nil).Body.String()
	has(t, body, `<label id="web.fold" class="fold" for="web.view.sideMenu" data-tip="طيّ القائمة `+"\u2068(Alt+1)\u2069"+`">»</label>`)
	has(t, body, `<label id="web.unfold" class="unfold" for="web.view.sideMenu" data-tip="إظهار القائمة `+"\u2068(Alt+1)\u2069"+`">«</label>`)
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
	has(t, body, `<input type="checkbox" id="web.view.topBar" name="top">`)
	has(t, body, `<input type="checkbox" id="web.view.statusBar" name="status">`)
	has(t, body, `<input type="checkbox" id="web.view.sideMenu" name="side" checked>`)
	has(t, body, `style="--side-w: 300px;"`)
	browser.Send("POST", "/view", url.Values{"top": {"on"}, "width": {"9000"}})
	has(t, browser.Send("GET", "/", nil).Body.String(), `style="--side-w: 480px;"`)
}

func TestWeb_R11_TheLanguageIsInSettingsNotTheTopBar(t *testing.T) {
	browser, _ := signedIn(t, testapi.New(t), "User A")
	page := browser.Send("GET", "/", nil).Body.String()
	lacks(t, page, `href="/language/`)
	settings := browser.Send("GET", "/tab/settings", nil).Body.String()
	has(t, settings, `<h3 id="myAccount.language">اللغة</h3>`)
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
	has(t, signing, `id="changePassword.current"`)
	lacks(t, signing, `class="side"`)
	if browser.Send("GET", "/tab/settings", nil).Header().Get("HX-Retarget") != "[id='web.shell']" {
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
	browser, _ := signedIn(t, api, "User A")
	page := browser.Send("GET", "/", nil).Body.String()
	for _, key := range []string{"documents"} {
		has(t, page, `<span id="web.item.`+key+`" class="parked" aria-disabled="true">`)
	}
	has(t, page, `hx-get="/tab/home"`)
	has(t, page, `hx-get="/tab/entries"`)
	has(t, page, `hx-get="/tab/settings"`)
}

func TestWeb_R16_SignOutEndsThisBrowserAndClearsItsCookies(t *testing.T) {
	api := testapi.New(t)
	browser, _ := signedIn(t, api, "User A")
	open := `SELECT count(*) FROM devices WHERE is_revoked = 0`
	before := api.Count(open)
	answered := browser.Send("POST", "/sign-out", nil)
	has(t, answered.Body.String(), `id="signIn.username"`)
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
	if browser.Send("GET", "/tab/settings", nil).Header().Get("HX-Retarget") != "[id='web.shell']" {
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
	if answered, _ := api.Browser().SendFailing(1, "POST", "/forgot-password", url.Values{"login": {"nobody"}}); answered.Code != http.StatusInternalServerError {
		t.Errorf("asking for a code with the database failing before the answer answers %d", answered.Code)
	}
	browser, _ := signedIn(t, api, "User B")
	for _, step := range []struct {
		method, path string
		form         url.Values
	}{
		{"GET", "/", nil}, {"GET", "/tab/settings", nil}, {"DELETE", "/tab/settings", nil}, {"GET", "/sign-in", nil}, {"GET", "/forgot-password", nil},
		{"POST", "/reset-with-code", url.Values{"login": {"nobody"}, "code": {"000000"}, "new": {"Reset-chosen-4"}, "confirm": {"Reset-chosen-4"}}},
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
	has(t, browser.Send("DELETE", "/tab/settings", nil).Body.String(), `id="projects.title"`)
}

func TestWeb_R13_ChangingAPasswordWithNoSessionAnswersTheSignInForm(t *testing.T) {
	answered := testapi.New(t).Browser().Send("POST", "/change-password", url.Values{"current": {password}, "new": {"Chosen-new-3"}, "confirm": {"Chosen-new-3"}})
	if answered.Header().Get("HX-Retarget") != "[id='web.shell']" {
		t.Errorf("no session answers the whole shell: %v", answered.Header())
	}
	has(t, answered.Body.String(), `id="signIn.username"`)
}

func TestWeb_EveryElementsIdIsTheOnlyOneOnItsPage(t *testing.T) {
	api := testapi.New(t)
	pages := map[string]string{"signed out": api.Browser().Send("GET", "/", nil).Body.String()}
	browser, _ := signedIn(t, api, "User A")
	browser.Send("GET", "/tab/settings", nil)
	pages["signed in"] = browser.Send("GET", "/", nil).Body.String()
	pages["forgot password, a code sent"] = pages["signed out"] + api.Browser().Send("POST", "/forgot-password", url.Values{"login": {"nobody"}}).Body.String()
	for name, page := range pages {
		seen := map[string]bool{}
		for _, id := range regexp.MustCompile(` id="([^"]+)"`).FindAllStringSubmatch(page, -1) {
			if seen[id[1]] {
				t.Errorf("%s: the id %q is on two elements", name, id[1])
			}
			seen[id[1]] = true
		}
		if len(seen) < 15 {
			t.Errorf("%s: only %d ids", name, len(seen))
		}
	}
}

func TestWeb_R19_AfterAWrongPasswordTheCursorWaitsInPassword(t *testing.T) {
	api := testapi.New(t)
	id, _ := api.Person("User A")
	browser := api.Browser()
	has(t, browser.Send("GET", "/", nil).Body.String(), `autocomplete="username" autofocus required`)
	refused := browser.Send("POST", "/sign-in", url.Values{"username": {id.String()}, "password": {"wrong-1"}}).Body.String()
	has(t, refused, `autocomplete="current-password" autofocus required`)
	lacks(t, refused, `autocomplete="username" autofocus`)
}

func TestWeb_R18_TheLookAndTheTabsAreKeptApartSoNeitherSaveUndoesTheOther(t *testing.T) {
	browser, _ := signedIn(t, testapi.New(t), "User A")
	cookies := func(answered interface{ Result() *http.Response }) []string {
		var names []string
		for _, cookie := range answered.Result().Cookies() {
			names = append(names, cookie.Name)
		}
		return names
	}
	if set := cookies(browser.Send("GET", "/tab/settings", nil)); !slices.Equal(set, []string{"redix_tabs"}) {
		t.Errorf("opening a tab keeps the tabs alone: %v", set)
	}
	if set := cookies(browser.Send("POST", "/view", url.Values{"side": {"on"}, "width": {"300"}})); !slices.Equal(set, []string{"redix_view"}) {
		t.Errorf("the View menu keeps the look alone: %v", set)
	}
	page := browser.Send("GET", "/", nil).Body.String()
	has(t, page, `style="--side-w: 300px;"`)
	has(t, page, `id="web.tab.settings" class="tab" role="tab" aria-selected="true"`)
}

func TestWeb_R2_TheVersionLineCarriesTheBuildKeptWholeInArabic(t *testing.T) {
	body := testapi.New(t).Browser().Send("GET", "/", nil).Body.String()
	has(t, body, `<span id="appVersion.line" class="end">الإصدار 1.0.0<bdi></bdi></span>`)
}

// withEmail gives name an account with address as its email, as confirming one would, and is their username.
func withEmail(t *testing.T, api *testapi.API, name, address string) string {
	t.Helper()
	id, _ := api.Person(name)
	api.Exec("UPDATE users SET email = ? WHERE username = ?", address, id.String())
	return id.String()
}

func TestWeb_AC02_6_ForgotPasswordOpensFromTheSignInAndCancelGoesBack(t *testing.T) {
	browser := testapi.New(t).Browser()
	has(t, browser.Send("GET", "/", nil).Body.String(), `<a id="auth.forgotPassword" href="#" hx-get="/forgot-password"`)
	form := browser.Send("GET", "/forgot-password", nil).Body.String()
	for _, key := range []string{"forgotPassword.title", "forgotPassword.login", "forgotPassword.send", "forgotPassword.message", "forgotPassword.cancel"} {
		has(t, form, `id="`+key+`"`)
	}
	has(t, form, "اسم المستخدم أو البريد")
	lacks(t, form, `id="forgotPassword.code"`)
	has(t, browser.Send("GET", "/sign-in", nil).Body.String(), `id="signIn.username"`)
}

func TestWeb_AC02_6_AForgottenPasswordIsResetWithTheEmailedCodeThenTheSignInSaysSo(t *testing.T) {
	api := testapi.New(t)
	username := withEmail(t, api, "User A", "a@example.com")
	browser := api.Browser()
	sent := browser.Send("POST", "/forgot-password", url.Values{"login": {" a@example.com "}})
	if sent.Code != http.StatusOK {
		t.Fatalf("send: %d", sent.Code)
	}
	for _, key := range []string{"forgotPassword.sent", "forgotPassword.code", "forgotPassword.new", "forgotPassword.confirm", "forgotPassword.reset"} {
		has(t, sent.Body.String(), `id="`+key+`"`)
	}
	has(t, sent.Body.String(), "إن كان للحساب بريد، أرسلنا إليه رمزًا صالحًا 15 دقيقة.")
	code := api.Emails.LastCodeTo("a@example.com")
	reset := browser.Send("POST", "/reset-with-code", url.Values{"login": {"a@example.com"}, "code": {code}, "new": {"Reset-chosen-4"}, "confirm": {"Reset-chosen-4"}})
	has(t, reset.Body.String(), `<p id="signIn.notice" class="notice" role="status">تغيّرت كلمة المرور. سجّل الدخول.</p>`)
	if signing := api.Browser().Send("POST", "/sign-in", url.Values{"username": {username}, "password": {"Reset-chosen-4"}}); signing.Code != http.StatusOK {
		t.Errorf("the new password signs in: %d", signing.Code)
	}
	again := browser.Send("POST", "/reset-with-code", url.Values{"login": {"a@example.com"}, "code": {code}, "new": {"Reset-chosen-5"}, "confirm": {"Reset-chosen-5"}})
	if again.Code != http.StatusUnprocessableEntity {
		t.Errorf("a used code is refused: %d", again.Code)
	}
	has(t, again.Body.String(), "الرمز غير صحيح أو انتهت صلاحيته.")
}

func TestWeb_AC02_7_AnUnknownLoginIsAnsweredAsAKnownOneAndNothingIsSent(t *testing.T) {
	api := testapi.New(t)
	answered := api.Browser().Send("POST", "/forgot-password", url.Values{"login": {"nobody"}})
	if answered.Code != http.StatusOK || api.Emails.Count() != 0 {
		t.Errorf("%d, %d emails", answered.Code, api.Emails.Count())
	}
	has(t, answered.Body.String(), `id="forgotPassword.sent"`)
}

func TestWeb_AC02_8_OneLoginAsksFiveTimesAnHourTheAppsAsksCountedWithTheWebs(t *testing.T) {
	api := testapi.New(t)
	for range 4 {
		api.Send("POST", "/auth/forgot-password", `{"login":"someone"}`)
	}
	browser := api.Browser()
	if fifth := browser.Send("POST", "/forgot-password", url.Values{"login": {"SOMEONE"}}); fifth.Code != http.StatusOK {
		t.Fatalf("the fifth ask: %d", fifth.Code)
	}
	sixth := browser.Send("POST", "/forgot-password", url.Values{"login": {"someone"}, "sent": {"true"}})
	if sixth.Code != http.StatusUnprocessableEntity {
		t.Fatalf("the sixth ask is refused: %d", sixth.Code)
	}
	has(t, sixth.Body.String(), "طلبات كثيرة. حاول بعد قليل.")
	has(t, sixth.Body.String(), `id="forgotPassword.code"`)
}

func TestWeb_AC02_6_AResetIsRefusedWithTheAppsWords(t *testing.T) {
	api := testapi.New(t)
	withEmail(t, api, "User A", "a@example.com")
	browser := api.Browser()
	browser.Send("POST", "/forgot-password", url.Values{"login": {"a@example.com"}})
	code := api.Emails.LastCodeTo("a@example.com")
	for _, step := range []struct {
		form url.Values
		says string
	}{
		{url.Values{"code": {code}, "new": {"Reset-chosen-4"}, "confirm": {"Reset-chosen-5"}}, "كلمتا المرور الجديدتان غير متطابقتين."},
		{url.Values{"code": {code}, "new": {"short"}, "confirm": {"short"}}, "استخدم 8 أحرف على الأقل فيها حرف ورقم، وليست اسم المستخدم."},
		{url.Values{"code": {"000000"}, "new": {"Reset-chosen-4"}, "confirm": {"Reset-chosen-4"}}, "الرمز غير صحيح أو انتهت صلاحيته."},
	} {
		step.form.Set("login", "a@example.com")
		answered := browser.Send("POST", "/reset-with-code", step.form)
		if answered.Code != http.StatusUnprocessableEntity {
			t.Errorf("%q: %d", step.says, answered.Code)
		}
		has(t, answered.Body.String(), step.says)
		has(t, answered.Body.String(), `value="a@example.com"`)
	}
}

// working is a project as its manager works in it on the web: approved, with an item, its table, a bank account and a payment method, and a browser
// the manager signed in on (redix-api #165).
type working struct {
	*testapi.API
	t                                             *testing.T
	browser                                       *testapi.Browser
	manager                                       uuid.UUID
	token, id, path, item, table, account, method string
}

func newWorking(t *testing.T) working {
	t.Helper()
	w := working{API: testapi.New(t), t: t}
	w.manager = uuid.Must(uuid.NewV7())
	admin := uuid.Must(uuid.NewV7())
	for _, id := range []uuid.UUID{w.manager, admin} {
		w.Exec(`INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at, email)
			VALUES (?, ?, 'User A', '', ?, 0, 0, '1900-01-01T00:00:00.0000000Z', '2026-11-01T10:00:00.0000000Z', '')`, id, id.String(), credentials.Hash(password))
	}
	w.Exec(`INSERT INTO system_permissions (id, user_id, permission) VALUES (?, ?, 'ApproveProjects')`, uuid.NewString(), admin)
	w.token = w.Token(w.manager, false)
	_, answer := w.Ask("POST", "/projects", `{"name":"مشروع A","startDate":"2026-11-01","units":["A1"]}`, w.token)
	w.id = testapi.Field(answer, "id").(string)
	w.path = "/projects/" + w.id
	w.Ask("POST", w.path+"/approve", ``, w.Token(admin, false))
	w.Ask("POST", w.path+"/accounts", `{"name":"حساب المؤسسة","type":"bank"}`, w.token)
	_, category := w.Ask("POST", w.path+"/categories", `{"name":"الحديد"}`, w.token)
	_, item := w.Ask("POST", w.path+"/categories/"+testapi.Field(category, "id").(string)+"/items", `{"name":"التسليح","type":"purchases"}`, w.token)
	w.item = testapi.Field(item, "id").(string)
	read := w.Database.Read
	for query, into := range map[string]*string{
		"SELECT id FROM subgroups WHERE item_id = '" + w.item + "'":                        &w.table,
		"SELECT id FROM accounts WHERE project_id = '" + w.id + "'":                        &w.account,
		"SELECT payment_methods.id FROM payment_methods WHERE project_id = '" + w.id + "'": &w.method,
	} {
		if err := read.QueryRow(query).Scan(into); err != nil {
			t.Fatal(query, err)
		}
	}
	w.browser = w.Browser()
	if signing := w.browser.Send("POST", "/sign-in", url.Values{"username": {w.manager.String()}, "password": {password}}); signing.Code != http.StatusOK {
		t.Fatalf("sign-in: %d", signing.Code)
	}
	return w
}

// sent records an entry of amount into the item's table and sends it for approval; it is the entry's id.
func (w working) sent(amount int) string {
	w.t.Helper()
	id := uuid.Must(uuid.NewV7()).String()
	body := `{"id":"` + id + `","itemId":"` + w.item + `","tableId":"` + w.table + `","date":"2026-11-10","amount":` + strconv.Itoa(amount) + `,"accountId":"` +
		w.account + `","paymentMethodId":"` + w.method + `","description":"","columnValues":{},"submit":true}`
	if code, answer := w.Ask("POST", w.path+"/entries", body, w.token); code != http.StatusCreated {
		w.t.Fatalf("recording: %d %v", code, answer)
	}
	return id
}

// home is the home the manager's browser shows after choosing the project.
func (w working) home() string {
	w.t.Helper()
	chosen := w.browser.Send("POST", "/project/"+w.id, nil)
	if chosen.Code != http.StatusOK {
		w.t.Fatalf("choosing the project: %d", chosen.Code)
	}
	return chosen.Body.String()
}

func TestWeb_R165_WithNoDefaultProjectTheHomeListsTheProjectsAndChoosingOneOpensItsHome(t *testing.T) {
	w := newWorking(t)
	first := w.browser.Send("GET", "/", nil).Body.String()
	has(t, first, `id="projects.card.`+w.id+`"`)
	has(t, first, `hx-post="/project/`+w.id+`"`)
	has(t, first, "اختر المشروع")
	lacks(t, first, `id="home.title"`)
	chosen := w.browser.Send("POST", "/project/"+w.id, nil)
	if chosen.Header().Get("HX-Retarget") != "[id='web.shell']" {
		t.Errorf("choosing a project answers the whole shell: %v", chosen.Header())
	}
	has(t, chosen.Body.String(), `<h2 id="home.title">مشروع A</h2>`)
	has(t, chosen.Body.String(), `<span id="web.sideProject" class="side-project">مشروع A</span>`)
	if w.browser.Cookie("redix_project") != w.id {
		t.Errorf("the project is kept: %q", w.browser.Cookie("redix_project"))
	}
	has(t, w.browser.Send("GET", "/", nil).Body.String(), `<h2 id="home.title">مشروع A</h2>`)
}

func TestWeb_R165_AC37_1_TheDefaultProjectOpensStraightIntoItsHome(t *testing.T) {
	w := newWorking(t)
	if code, _ := w.Ask("POST", w.path+"/default", ``, w.token); code >= 300 {
		t.Fatalf("default: %d", code)
	}
	has(t, w.browser.Send("GET", "/", nil).Body.String(), `<h2 id="home.title">مشروع A</h2>`)
}

func TestWeb_R165_UC049_TheSummaryAndTheNewestEntriesAreTheApps(t *testing.T) {
	w := newWorking(t)
	approved, waiting := w.sent(100), w.sent(50)
	if code, answer := w.Ask("POST", w.path+"/entries/approve", `{"entryIds":["`+approved+`"]}`, w.token); code != http.StatusOK {
		t.Fatalf("approve: %d %v", code, answer)
	}
	home := w.home()
	_, summary := w.Ask("GET", w.path+"/summary", ``, w.token)
	if testapi.Field(summary, "surplus") != -100.0 {
		t.Fatalf("the app's surplus: %v", testapi.Field(summary, "surplus"))
	}
	has(t, home, `<span id="summary.surplus" class="surplus">(100.00) <small id="summary.surplus.currency">ر.س</small></span>`)
	has(t, home, `<b id="summary.expenses">100.00</b>`)
	has(t, home, `<strong id="summary.pendingCount">بانتظار اعتمادك: 1</strong>`)
	has(t, home, `id="entries.row.`+approved+`"`)
	has(t, home, `id="entries.row.`+waiting+`"`)
	has(t, home, `class="state approved">معتمد</span>`)
	has(t, home, `class="state pendingApproval">بانتظار الاعتماد</span>`)
	has(t, home, `class="amount">50.00</td>`)
	has(t, home, `id="home.addEntry.`+w.id+`" class="primary small" type="button" disabled`)
}

func TestWeb_R165_AnEmptyProjectSaysSoAndShowsNoWaiting(t *testing.T) {
	home := newWorking(t).home()
	has(t, home, "لا قيود بعد.")
	lacks(t, home, `id="summary.pending"`)
	has(t, home, `<span id="summary.surplus" class="surplus">0.00 <small id="summary.surplus.currency">ر.س</small></span>`)
}

func TestWeb_R165_MyCustodyShowsWhatIHold(t *testing.T) {
	w := newWorking(t)
	if code, answer := w.Ask("POST", w.path+"/accounts", `{"name":"عهدة مستخدم أ","type":"cashCustody","custodianId":"`+w.manager.String()+`"}`, w.token); code >= 300 {
		t.Fatalf("custody: %d %v", code, answer)
	}
	var id string
	if err := w.Database.Read.QueryRow("SELECT id FROM accounts WHERE type = 1 AND project_id = ?", w.id).Scan(&id); err != nil {
		t.Fatal(err)
	}
	home := w.home()
	has(t, home, `id="home.custody.`+id+`"`)
	has(t, home, `<b id="home.custodyBalance.`+id+`">0.00 ر.س</b>`)
	lacks(t, home, `id="home.custody.`+w.account+`"`)
}

func TestWeb_R165_WhatWaitsShowsOnlyToWhoeverMayApprove(t *testing.T) {
	w := newWorking(t)
	w.sent(50)
	member, _ := w.Person("User B")
	if code, answer := w.Ask("POST", w.path+"/users/existing", `{"username":"`+member.String()+`"}`, w.token); code >= 300 {
		t.Fatalf("adding: %d %v", code, answer)
	}
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {member.String()}, "password": {password}})
	home := browser.Send("POST", "/project/"+w.id, nil).Body.String()
	has(t, home, `id="summary.card"`)
	lacks(t, home, `id="summary.pending"`)
	has(t, w.home(), `id="summary.pending"`)
}

func TestWeb_R165_AProjectOneMayNotOpenIsNotChosen(t *testing.T) {
	w := newWorking(t)
	if code := w.browser.Send("POST", "/project/"+uuid.NewString(), nil).Code; code != http.StatusNotFound {
		t.Errorf("a project not theirs: %d", code)
	}
	browser, _ := signedIn(t, w.API, "User B")
	if code := browser.Send("POST", "/project/"+w.id, nil).Code; code != http.StatusNotFound {
		t.Errorf("another's project: %d", code)
	}
}

func TestWeb_R165_HideDetailsIsKeptWithTheLook(t *testing.T) {
	w := newWorking(t)
	has(t, w.home(), `<input type="checkbox" id="web.view.summary" name="summary" checked hidden>`)
	w.browser.Send("POST", "/view", url.Values{"top": {"on"}, "status": {"on"}, "side": {"on"}, "width": {"248"}})
	page := w.browser.Send("GET", "/", nil).Body.String()
	has(t, page, `<input type="checkbox" id="web.view.summary" name="summary" hidden>`)
	has(t, page, `<label id="home.collapse" class="quiet-button" for="web.view.summary">`)
}

func TestWeb_R165_TheHomeAnswersAFailingDatabaseWith500(t *testing.T) {
	w := newWorking(t)
	w.sent(50)
	w.home()
	for _, step := range []struct{ method, path string }{{"GET", "/"}, {"GET", "/tab/home"}, {"POST", "/project/" + w.id}} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, step.method, step.path, nil)
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s %s with the database failing at its call %d answers %d", step.method, step.path, n, answered.Code)
			}
		}
	}
}

func TestWeb_R165_EveryIdOnTheHomeIsTheOnlyOne(t *testing.T) {
	w := newWorking(t)
	w.sent(50)
	w.home()
	page := w.browser.Send("GET", "/", nil).Body.String()
	seen := map[string]bool{}
	for _, id := range regexp.MustCompile(` id="([^"]+)"`).FindAllStringSubmatch(page, -1) {
		if seen[id[1]] {
			t.Errorf("the id %q is on two elements", id[1])
		}
		seen[id[1]] = true
	}
	if len(seen) < 40 {
		t.Errorf("only %d ids", len(seen))
	}
}

// registration is a whole Register new project form, as a person fills it in.
func registration(username string) url.Values {
	return url.Values{"name": {"مشروع د"}, "description": {""}, "startDate": {"2026-11-21"}, "unit": {"A1", "A2"}, "partner": {"شريك أ"},
		"displayName": {"مدير د"}, "username": {username}, "mobile": {"0500000000"}, "password": {"Chosen-passw0rd-1"}}
}

func TestWeb_R164_TheSignInsRegisterLinkOpensItsFormAndCancelGoesBack(t *testing.T) {
	browser := testapi.New(t).Browser()
	has(t, browser.Send("GET", "/", nil).Body.String(), `<a id="signIn.registerProject" href="#" hx-get="/register"`)
	form := browser.Send("GET", "/register", nil).Body.String()
	for _, key := range []string{"newProject.title", "newProject.hint", "newProject.name", "newProject.startDate", "newProject.unitName", "newProject.addUnit",
		"newProject.partnerName", "newProject.addPartner", "newProject.managerName", "newProject.username", "newProject.mobile", "newProject.password",
		"legal.agreement", "newProject.submit", "newProject.cancel"} {
		has(t, form, `id="`+key+`"`)
	}
	has(t, form, `value="2026-11-`)
	has(t, browser.Send("GET", "/sign-in", nil).Body.String(), `id="signIn.username"`)
}

func TestWeb_R164_UC066_ARegistrationWaitsForTheAdminAndTheSignInSaysSo(t *testing.T) {
	api := testapi.New(t)
	answered := api.Browser().Send("POST", "/register", registration("manager.d"))
	if answered.Code != http.StatusOK {
		t.Fatalf("register: %d %s", answered.Code, answered.Body)
	}
	has(t, answered.Body.String(), `<p id="signIn.notice" class="notice" role="status">أُرسل للاعتماد. سجّل الدخول لمتابعته.</p>`)
	if got := api.Count(`SELECT count(*) FROM projects WHERE name = 'مشروع د' AND status = 2`); got != 1 {
		t.Errorf("the project waits for the admin: %d", got)
	}
	if got := api.Count(`SELECT count(*) FROM units JOIN projects ON projects.id = units.project_id WHERE projects.name = 'مشروع د'`); got != 2 {
		t.Errorf("its units: %d", got)
	}
	signing := api.Browser().Send("POST", "/sign-in", url.Values{"username": {"manager.d"}, "password": {"Chosen-passw0rd-1"}})
	if signing.Code != http.StatusOK {
		t.Errorf("the manager signs in with their own password: %d", signing.Code)
	}
}

func TestWeb_R164_ARefusalAnswersTheFormWithTheAppsWordsAndKeepsWhatWasTyped(t *testing.T) {
	api := testapi.New(t)
	api.Browser().Send("POST", "/register", registration("manager.d"))
	for _, step := range []struct {
		change func(url.Values)
		says   string
	}{
		{func(form url.Values) { form.Set("name", "") }, "اكتب اسماً من 1 إلى 100 حرف."},
		{func(form url.Values) { form.Set("password", "short") }, "استخدم 8 أحرف على الأقل فيها حرف ورقم، وليست اسم المستخدم."},
		{func(form url.Values) { form["unit"] = []string{"A1", "A1"} }, "لا يصح أن تحمل وحدتان الاسم نفسه."},
		{func(form url.Values) { form.Set("username", "manager.d") }, "اسم المستخدم هذا مستخدم. إن كان لك، سجّل الدخول وسجّل المشروع من مشاريعك."},
	} {
		form := registration("manager.e")
		step.change(form)
		answered := api.Browser().Send("POST", "/register", form)
		if answered.Code != http.StatusUnprocessableEntity {
			t.Errorf("%q: %d", step.says, answered.Code)
		}
		has(t, answered.Body.String(), step.says)
		has(t, answered.Body.String(), `<input type="hidden" name="partner" value="شريك أ">`)
		lacks(t, answered.Body.String(), "Chosen-passw0rd-1")
	}
}

func TestWeb_R164_AddAndRemoveAnswerTheListAlone(t *testing.T) {
	browser := testapi.New(t).Browser()
	added := browser.Send("POST", "/register/units", url.Values{"unit": {"A1"}, "unitName": {" A2 "}}).Body.String()
	has(t, added, `<fieldset id="newProject.units" class="list-field">`)
	has(t, added, `<input type="hidden" name="unit" value="A1">`)
	has(t, added, `<input type="hidden" name="unit" value="A2">`)
	lacks(t, added, `id="newProject.name"`)
	removed := browser.Send("POST", "/register/units", url.Values{"unit": {"A1", "A2"}, "unitName": {"A3"}, "remove": {"0"}}).Body.String()
	lacks(t, removed, `value="A1"`)
	lacks(t, removed, `value="A3"`)
	has(t, removed, `<span id="newProject.unit.0" class="chip">`)
	blank := browser.Send("POST", "/register/partners", url.Values{"partnerName": {"  "}}).Body.String()
	lacks(t, blank, `class="chip"`)
	if code := browser.Send("POST", "/register/people", nil).Code; code != http.StatusNotFound {
		t.Errorf("a list the form has not: %d", code)
	}
}

func TestWeb_R164_RegisteringAnswersAFailingDatabaseWith500(t *testing.T) {
	api := testapi.New(t)
	for _, step := range []struct {
		path string
		form url.Values
	}{{"/register", nil}, {"/register/units", url.Values{"unitName": {"A1"}}}} {
		for n := int64(1); ; n++ {
			answered, failed := api.Browser().SendFailing(n, "POST", step.path, step.form)
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s with the database failing at its call %d answers %d", step.path, n, answered.Code)
			}
		}
	}
	for n := int64(1); ; n++ {
		answered, failed := api.Browser().SendFailing(n, "POST", "/register", registration("manager.f"))
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("registering with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
}

func TestWeb_R164_EveryIdOnTheRegisterFormIsTheOnlyOneAndNoneIsTheFrames(t *testing.T) {
	form := registration("manager.h")
	form.Set("name", "")
	fragment := testapi.New(t).Browser().Send("POST", "/register", form).Body.String()
	seen := map[string]bool{}
	for _, id := range regexp.MustCompile(` id="([^"]+)"`).FindAllStringSubmatch(fragment, -1) {
		if seen[id[1]] || strings.HasPrefix(id[1], "web.") || id[1] == "appVersion.line" {
			t.Errorf("the id %q is on two elements", id[1])
		}
		seen[id[1]] = true
	}
	if len(seen) < 25 {
		t.Errorf("only %d ids", len(seen))
	}
}

func TestWeb_R165_AC37_1_TheStarMakesAProjectTheDefaultAndAgainClearsIt(t *testing.T) {
	w := newWorking(t)
	starred := w.browser.Send("POST", "/project/"+w.id+"/default", nil)
	if starred.Code != http.StatusOK {
		t.Fatalf("starring: %d", starred.Code)
	}
	has(t, starred.Body.String(), `aria-pressed="true"`)
	has(t, starred.Body.String(), `<p id="web.projects.notice" class="notice" role="status">مشروع A هو مشروعك الافتراضي الآن، ويُفتح عند الدخول.</p>`)
	lacks(t, starred.Body.String(), `id="web.shell"`)
	again := w.Browser()
	again.Send("POST", "/sign-in", url.Values{"username": {w.manager.String()}, "password": {password}})
	has(t, again.Send("GET", "/", nil).Body.String(), `<h2 id="home.title">مشروع A</h2>`)
	cleared := w.browser.Send("POST", "/project/"+w.id+"/default", nil).Body.String()
	has(t, cleared, `aria-pressed="false"`)
	lacks(t, cleared, `id="web.projects.notice"`)
	if got := w.Count(`SELECT count(*) FROM user_default_projects`); got != 0 {
		t.Errorf("the default is cleared: %d left", got)
	}
	if code := w.browser.Send("POST", "/project/"+uuid.NewString()+"/default", nil).Code; code != http.StatusNotFound {
		t.Errorf("starring a project not theirs: %d", code)
	}
}

func TestWeb_R165_StarringAnswersAFailingDatabaseWith500(t *testing.T) {
	w := newWorking(t)
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "POST", "/project/"+w.id+"/default", nil)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("starring with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
}

// approved records an entry of amount and approves it; it is the entry's id.
func (w working) approved(amount int) string {
	w.t.Helper()
	id := w.sent(amount)
	if code, answer := w.Ask("POST", w.path+"/entries/approve", `{"entryIds":["`+id+`"]}`, w.token); code != http.StatusOK {
		w.t.Fatalf("approve: %d %v", code, answer)
	}
	return id
}

// listed is the Entries tab as the manager's browser asks it with query, after choosing the project.
func (w working) listed(query string) string {
	w.t.Helper()
	answered := w.browser.Send("GET", "/tab/entries"+query, nil)
	if answered.Code != http.StatusOK {
		w.t.Fatalf("the list: %d", answered.Code)
	}
	return answered.Body.String()
}

func TestWeb_R166_UC028_TheListShowsWhatTheCallerSeesAndTheFiltersNarrowIt(t *testing.T) {
	w := newWorking(t)
	approved, waiting := w.approved(100), w.sent(50)
	w.home()
	all := w.listed("")
	has(t, all, `id="entries.row.`+approved+`"`)
	has(t, all, `id="entries.row.`+waiting+`"`)
	lacks(t, all, `id="entries.clear"`)
	onlyApproved := w.listed("?statuses=approved")
	has(t, onlyApproved, `id="entries.row.`+approved+`"`)
	lacks(t, onlyApproved, `id="entries.row.`+waiting+`"`)
	has(t, onlyApproved, `type="checkbox" name="statuses" value="approved" checked>`)
	has(t, onlyApproved, `id="entries.clear"`)
	byAmount := w.listed("?text=50")
	has(t, byAmount, `id="entries.row.`+waiting+`"`)
	lacks(t, byAmount, `id="entries.row.`+approved+`"`)
	has(t, w.listed("?kinds=revenue"), `<p id="entries.empty" class="empty">لا قيود بعد.</p>`)
	has(t, w.listed("?mine=true"), `id="entries.row.`+approved+`"`)
	lacks(t, w.listed("?from=2026-11-11"), `id="entries.row.`)
	has(t, w.listed("?from=2026-11-10&to=2026-11-10"), `id="entries.row.`+waiting+`"`)
}

func TestWeb_R166_ShowMoreAddsAPageWithTheSameFilters(t *testing.T) {
	w := newWorking(t)
	for range 31 {
		w.sent(10)
	}
	w.home()
	first := w.listed("")
	if got := strings.Count(first, `id="entries.row.`); got != 30 {
		t.Errorf("the first page holds %d", got)
	}
	has(t, first, `hx-vals="{&#34;take&#34;:&#34;60&#34;}"`)
	has(t, first, `hx-include="[id='entries.filters']"`)
	second := w.listed("?take=60")
	if got := strings.Count(second, `id="entries.row.`); got != 31 {
		t.Errorf("the second page holds %d", got)
	}
	lacks(t, second, `id="entries.more"`)
}

func TestWeb_R166_TheListAnswersAFailingDatabaseWith500AndWithNoProjectTheProjects(t *testing.T) {
	w := newWorking(t)
	has(t, w.listed(""), `id="projects.title"`)
	w.home()
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "GET", "/tab/entries?statuses=approved", nil)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("the list with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
}

func TestWeb_R166_ARowOpensItsEntryInTheDetailsPanel(t *testing.T) {
	w := newWorking(t)
	id := w.approved(100)
	w.home()
	list := w.listed("")
	has(t, list, `hx-get="/details/entry/`+id+`" hx-target="[id='web.details']"`)
	has(t, list, `<aside id="web.details" class="details" aria-live="polite" hx-swap-oob="true"></aside>`)
	panel := w.browser.Send("GET", "/details/entry/"+id, nil)
	if panel.Code != http.StatusOK {
		t.Fatalf("the panel: %d", panel.Code)
	}
	has(t, panel.Body.String(), `<h3 id="entry.title">قيد `)
	has(t, panel.Body.String(), `<dd id="entry.amount" class="money">100.00</dd>`)
	has(t, panel.Body.String(), `class="state approved">معتمد</span>`)
	has(t, panel.Body.String(), `أدخله User A`)
	lacks(t, panel.Body.String(), `id="entry.reason"`)
	if code := w.browser.Send("GET", "/details/entry/"+uuid.NewString(), nil).Code; code != http.StatusNotFound {
		t.Errorf("an entry there is none of: %d", code)
	}
}

func TestWeb_R166_ARejectedEntrySaysWhy(t *testing.T) {
	w := newWorking(t)
	id := w.sent(40)
	if code, answer := w.Ask("POST", w.path+"/entries/"+id+"/reject", `{"reason":"الفاتورة غير واضحة"}`, w.token); code >= 300 {
		t.Fatalf("reject: %d %v", code, answer)
	}
	w.home()
	has(t, w.browser.Send("GET", "/details/entry/"+id, nil).Body.String(), `<dd id="entry.reason" class="refused">الفاتورة غير واضحة</dd>`)
}

func TestWeb_R166_AnEntryOfAnotherProjectIsNotShown(t *testing.T) {
	w := newWorking(t)
	id := w.sent(40)
	browser, _ := signedIn(t, w.API, "User B")
	if code := browser.Send("GET", "/details/entry/"+id, nil).Code; code != http.StatusNotFound {
		t.Errorf("another project's entry: %d", code)
	}
}

func TestWeb_R166_ThePanelAnswersAFailingDatabaseWith500(t *testing.T) {
	w := newWorking(t)
	id := w.sent(40)
	w.home()
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "GET", "/details/entry/"+id, nil)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("the panel with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
}

func TestWeb_R166_NewContentIsSettledAtOnceSoItsTriggersHearTheFirstKey(t *testing.T) {
	has(t, testapi.New(t).Browser().Send("GET", "/", nil).Body.String(), `&#34;defaultSettleDelay&#34;:0}`)
}

// drafted records an entry of amount into the item's table as a draft, not sent; it is the entry's id.
func (w working) drafted(amount int) string {
	w.t.Helper()
	id := uuid.Must(uuid.NewV7()).String()
	body := `{"id":"` + id + `","itemId":"` + w.item + `","tableId":"` + w.table + `","date":"2026-11-10","amount":` + strconv.Itoa(amount) + `,"accountId":"` +
		w.account + `","paymentMethodId":"` + w.method + `","description":"","columnValues":{},"submit":false}`
	if code, answer := w.Ask("POST", w.path+"/entries", body, w.token); code != http.StatusCreated {
		w.t.Fatalf("drafting: %d %v", code, answer)
	}
	return id
}

// statusOf is the entry's status as kept.
func (w working) statusOf(id string) int {
	w.t.Helper()
	var status int
	if err := w.Database.Read.QueryRow("SELECT status FROM entries WHERE id = ?", id).Scan(&status); err != nil {
		w.t.Fatal(err)
	}
	return status
}

func TestWeb_R166_FR19_TheManagerApprovesFromThePanelAndTheRowFollows(t *testing.T) {
	w := newWorking(t)
	id := w.sent(40)
	w.home()
	panel := w.browser.Send("GET", "/details/entry/"+id, nil).Body.String()
	has(t, panel, `hx-post="/entry/`+id+`/approve"`)
	lacks(t, panel, `id="entry.approve" class="primary small" type="button" disabled`)
	approved := w.browser.Send("POST", "/entry/"+id+"/approve", nil)
	if approved.Code != http.StatusOK {
		t.Fatalf("approving: %d", approved.Code)
	}
	has(t, approved.Body.String(), `class="state approved">معتمد</span>`)
	has(t, approved.Body.String(), `<tr id="entries.row.`+id+`" tabindex="0" aria-selected="false" hx-get="/details/entry/`+id+`" hx-target="[id='web.details']" hx-swap-oob="true">`)
	if w.statusOf(id) != 2 {
		t.Errorf("the entry is approved: %d", w.statusOf(id))
	}
	again := w.browser.Send("POST", "/entry/"+id+"/approve", nil).Body.String()
	has(t, again, `<p id="entry.refusal" class="refusal details-say" role="alert">قرر أحدهم هذا القيد من قبل.</p>`)
}

func TestWeb_R166_ADraftIsSentAndTheRemovedOneLeavesTheList(t *testing.T) {
	w := newWorking(t)
	draft, approved := w.drafted(30), w.approved(70)
	w.home()
	lacks(t, w.browser.Send("GET", "/details/entry/"+approved+"?asking=remove", nil).Body.String(), `id="entry.ask"`)
	if code, answer := w.Ask("POST", w.path+"/users/"+w.manager.String()+"/permissions/RemoveApprovedEntries", ``, w.token); code >= 300 {
		t.Fatalf("ticking: %d %v", code, answer)
	}
	has(t, w.browser.Send("GET", "/details/entry/"+draft, nil).Body.String(), `hx-post="/entry/`+draft+`/send"`)
	sent := w.browser.Send("POST", "/entry/"+draft+"/send", nil).Body.String()
	has(t, sent, `<p id="entry.notice" class="notice details-say" role="status">أُرسل للاعتماد.</p>`)
	has(t, sent, `class="state pendingApproval">بانتظار الاعتماد</span>`)
	asking := w.browser.Send("GET", "/details/entry/"+approved+"?asking=remove", nil).Body.String()
	has(t, asking, "حذف هذا القيد المعتمد؟")
	has(t, asking, `hx-post="/entry/`+approved+`/remove"`)
	removed := w.browser.Send("POST", "/entry/"+approved+"/remove", nil).Body.String()
	has(t, removed, `<h3 id="entry.title">حُذف.</h3>`)
	has(t, removed, `<template><tr id="entries.row.`+approved+`" hx-swap-oob="delete"></tr></template>`)
	if w.statusOf(approved) != 4 {
		t.Errorf("the approved entry is removed softly: %d", w.statusOf(approved))
	}
}

func TestWeb_R166_AnActionIsGatedAsTheAppsRouteIs(t *testing.T) {
	w := newWorking(t)
	id := w.sent(40)
	member, _ := w.Person("User B")
	w.Ask("POST", w.path+"/users/existing", `{"username":"`+member.String()+`"}`, w.token)
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {member.String()}, "password": {password}})
	browser.Send("POST", "/project/"+w.id, nil)
	has(t, browser.Send("GET", "/details/entry/"+id, nil).Body.String(), `id="entry.approve" class="primary small" type="button" disabled`)
	if code := browser.Send("POST", "/entry/"+id+"/approve", nil).Code; code != http.StatusForbidden {
		t.Errorf("approving without the permission: %d", code)
	}
	if w.statusOf(id) != 1 {
		t.Errorf("the entry still waits: %d", w.statusOf(id))
	}
	w.home()
	if code := w.browser.Send("POST", "/entry/"+id+"/archive", nil).Code; code != http.StatusNotFound {
		t.Errorf("an action there is none of: %d", code)
	}
	w.browser.Send("POST", "/entry/"+id+"/approve", nil)
	if code, answer := w.Ask("POST", w.path+"/close", ``, w.token); code >= 300 {
		t.Fatalf("closing: %d %v", code, answer)
	}
	has(t, w.browser.Send("POST", "/entry/"+id+"/remove", nil).Body.String(), "هذا المشروع منتهٍ، فلا يمكن تغييره.")
	if w.statusOf(id) != 2 {
		t.Errorf("a finished project's entry stays approved: %d", w.statusOf(id))
	}
}

func TestWeb_R166_AnActionAnswersAFailingDatabaseWith500(t *testing.T) {
	w := newWorking(t)
	id := w.sent(40)
	w.home()
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "POST", "/entry/"+id+"/approve", nil)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("approving with the database failing at its call %d answers %d", n, answered.Code)
		}
		if w.statusOf(id) != 1 {
			t.Errorf("an approval failing at its call %d keeps nothing: %d", n, w.statusOf(id))
		}
	}
}

// typedEntry is Add entry's form as a person fills it for the item's table, with amount, sent for approval when send.
func (w working) typedEntry(amount string, send bool) url.Values {
	submit := "draft"
	if send {
		submit = "send"
	}
	return url.Values{"table": {w.item + ":" + w.table}, "description": {"دفعة"}, "date": {"2026-11-10"}, "amount": {amount}, "method": {w.method},
		"account": {w.account}, "submit": {submit}}
}

// entriesKept is how many entries of the project are kept with status.
func (w working) entriesKept(status int) int {
	w.t.Helper()
	var count int
	if err := w.Database.Read.QueryRow("SELECT count(*) FROM entries WHERE project_id = ? AND status = ?", w.id, status).Scan(&count); err != nil {
		w.t.Fatal(err)
	}
	return count
}

func TestWeb_R166_UC022_AddEntryRecordsThroughTheAppsRuleAndShowsTheEntry(t *testing.T) {
	w := newWorking(t)
	w.home()
	has(t, w.browser.Send("GET", "/tab/entries", nil).Body.String(), `hx-get="/tab/addEntry"`)
	form := w.browser.Send("GET", "/tab/addEntry", nil).Body.String()
	has(t, form, `value="`+w.item+":"+w.table+`">التسليح</option>`)
	has(t, form, `id="payment.method.`+w.method+`"`)
	has(t, form, `id="payment.account.`+w.account+`"`)
	has(t, form, `id="addEntry.supplier"`)
	sent := w.browser.Send("POST", "/entry", w.typedEntry("1,200", true)).Body.String()
	has(t, sent, `<p id="addEntry.message" class="notice wide" role="status">أُرسل للاعتماد.</p>`)
	has(t, sent, `<aside id="web.details" class="details" aria-live="polite" hx-swap-oob="true">`)
	has(t, sent, `<dd id="entry.amount" class="money">1,200.00</dd>`)
	lacks(t, sent, `value="1,200"`)
	if w.entriesKept(1) != 1 {
		t.Errorf("one entry waits: %d", w.entriesKept(1))
	}
	has(t, w.browser.Send("POST", "/entry", w.typedEntry("75", false)).Body.String(), "حُفظ كمسودة.")
	if w.entriesKept(0) != 1 {
		t.Errorf("one draft is kept: %d", w.entriesKept(0))
	}
}

func TestWeb_R166_UC022_AddEntrySaysWhyNotAndKeepsWhatWasTyped(t *testing.T) {
	w := newWorking(t)
	w.home()
	for amount, word := range map[string]string{"abc": "أدخل مبلغاً أكبر من صفر", "": "أدخل مبلغاً أكبر من صفر"} {
		refused := w.browser.Send("POST", "/entry", w.typedEntry(amount, true)).Body.String()
		has(t, refused, word)
		has(t, refused, `<input id="addEntry.description" name="description" value="دفعة">`)
	}
	early := w.typedEntry("50", true)
	early.Set("date", "2026-10-01")
	has(t, w.browser.Send("POST", "/entry", early).Body.String(), `<p id="addEntry.message" class="refusal wide" role="alert">`)
	if w.entriesKept(0)+w.entriesKept(1) != 0 {
		t.Errorf("a refused entry is not kept: %d", w.entriesKept(0)+w.entriesKept(1))
	}
	if code, answer := w.Ask("POST", w.path+"/close", ``, w.token); code >= 300 {
		t.Fatalf("closing: %d %v", code, answer)
	}
	finished := w.browser.Send("POST", "/entry", w.typedEntry("50", true)).Body.String()
	has(t, finished, "هذا المشروع منتهٍ، فلا يمكن تغييره.")
	has(t, w.browser.Send("GET", "/tab/entries", nil).Body.String(), `id="entries.new" class="primary small" type="button" disabled`)
	if w.entriesKept(1) != 0 {
		t.Errorf("a finished project records nothing: %d", w.entriesKept(1))
	}
}

func TestWeb_R166_AddEntryAnswersAFailingDatabaseWith500(t *testing.T) {
	w := newWorking(t)
	w.home()
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "POST", "/entry", w.typedEntry("40", true))
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("recording with the database failing at its call %d answers %d", n, answered.Code)
		}
		if w.entriesKept(1) != 0 {
			t.Fatalf("an entry failing at its call %d keeps nothing: %d", n, w.entriesKept(1))
		}
	}
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "GET", "/tab/addEntry", nil)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("drawing Add entry with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
}

// amountOf is the entry's amount as kept, as the lists write it.
func (w working) amountOf(id string) string {
	w.t.Helper()
	var amount string
	if err := w.Database.Read.QueryRow("SELECT amount FROM entries WHERE id = ?", id).Scan(&amount); err != nil {
		w.t.Fatal(err)
	}
	return amount
}

func TestWeb_R166_UC033_ChangeFillsTheFormAndSavesThroughTheAppsRule(t *testing.T) {
	w := newWorking(t)
	draft := w.drafted(30)
	w.home()
	has(t, w.browser.Send("GET", "/details/entry/"+draft, nil).Body.String(), `hx-get="/tab/changeEntry?entry=`+draft+`"`)
	form := w.browser.Send("GET", "/tab/changeEntry?entry="+draft, nil).Body.String()
	has(t, form, `<h2 id="addEntry.title">تعديل</h2>`)
	has(t, form, `value="`+w.item+":"+w.table+`" selected>التسليح</option>`)
	has(t, form, `<input id="addEntry.amount" name="amount" inputmode="decimal" value="30">`)
	has(t, form, `hx-post="/entry/`+draft+`"`)
	changed := w.browser.Send("POST", "/entry/"+draft, w.typedEntry("45", true)).Body.String()
	has(t, changed, "أُرسل للاعتماد.")
	if w.statusOf(draft) != 1 || !strings.HasPrefix(w.amountOf(draft), "45") {
		t.Errorf("the draft is changed and sent: status %d, amount %s", w.statusOf(draft), w.amountOf(draft))
	}
}

func TestWeb_R166_UC033_AnApprovedEntryIsNotChanged(t *testing.T) {
	w := newWorking(t)
	approved := w.approved(70)
	w.home()
	has(t, w.browser.Send("GET", "/tab/changeEntry?entry="+approved, nil).Body.String(), "هذا القيد لم يعد هنا.")
	has(t, w.browser.Send("POST", "/entry/"+approved, w.typedEntry("45", true)).Body.String(), "فلم يُطبَّق تعديلك.")
	if !strings.HasPrefix(w.amountOf(approved), "70") {
		t.Errorf("an approved entry keeps its amount: %s", w.amountOf(approved))
	}
	if code := w.browser.Send("POST", "/entry/"+uuid.NewString(), w.typedEntry("45", true)).Code; code != http.StatusNotFound {
		t.Errorf("changing an entry there is none of: %d", code)
	}
}

func TestWeb_R166_ChangeAnswersAFailingDatabaseWith500(t *testing.T) {
	w := newWorking(t)
	draft := w.drafted(30)
	w.home()
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "GET", "/tab/changeEntry?entry="+draft, nil)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("drawing Change with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "POST", "/entry/"+draft, w.typedEntry("45", true))
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("changing with the database failing at its call %d answers %d", n, answered.Code)
		}
		if w.statusOf(draft) != 0 {
			t.Fatalf("a change failing at its call %d keeps nothing: %d", n, w.statusOf(draft))
		}
	}
}

func TestWeb_R166_UC033_FixFillsARejectedEntryWithItsColumns(t *testing.T) {
	w := newWorking(t)
	id := uuid.Must(uuid.NewV7()).String()
	body := `{"id":"` + id + `","itemId":"` + w.item + `","tableId":"` + w.table + `","date":"2026-11-10","amount":60,"accountId":"` + w.account +
		`","paymentMethodId":"` + w.method + `","description":"","columnValues":{"quantity":"3"},"submit":true}`
	if code, answer := w.Ask("POST", w.path+"/entries", body, w.token); code != http.StatusCreated {
		t.Fatalf("recording: %d %v", code, answer)
	}
	if code, answer := w.Ask("POST", w.path+"/entries/"+id+"/reject", `{"reason":"الفاتورة غير واضحة"}`, w.token); code >= 300 {
		t.Fatalf("rejecting: %d %v", code, answer)
	}
	w.home()
	form := w.browser.Send("GET", "/tab/changeEntry?entry="+id, nil).Body.String()
	has(t, form, `<h2 id="addEntry.title">تعديل وإعادة إرسال</h2>`)
	has(t, form, `<input id="addEntry.column.quantity" name="column.quantity" inputmode="decimal" value="3">`)
}

func TestWeb_R166_BR07_AContractPaymentShowsTheContractsFiguresAsTheAmountChanges(t *testing.T) {
	w := newWorking(t)
	_, supplier := w.Ask("POST", w.path+"/suppliers", `{"name":"مؤسسة الإتقان"}`, w.token)
	w.Ask("PUT", w.path+"/items/"+w.item+"/type", `{"type":"both"}`, w.token)
	if code, answer := w.Ask("POST", w.path+"/contracts", `{"supplierId":"`+testapi.Field(supplier, "id").(string)+`","itemId":"`+w.item+`","value":1000,"isOpenValue":false}`,
		w.token); code >= 300 {
		t.Fatalf("the contract: %d %v", code, answer)
	}
	var contract string
	if err := w.Database.Read.QueryRow("SELECT id FROM contracts").Scan(&contract); err != nil {
		t.Fatal(err)
	}
	w.home()
	table := w.item + ":" + contract
	form := w.browser.Send("GET", "/tab/addEntry?table="+table, nil).Body.String()
	has(t, form, `<dd id="addEntry.value" class="money">1,000.00</dd>`)
	has(t, form, `<dd id="addEntry.remaining" class="money">1,000.00</dd>`)
	has(t, form, `hx-get="/entry-figures"`)
	over := w.browser.Send("GET", "/entry-figures?table="+table+"&amount=1200", nil).Body.String()
	has(t, over, `id="addEntry.overrun"`)
	has(t, over, `<dd id="addEntry.remaining" class="money refused">(200.00)</dd>`)
	has(t, over, `<progress id="addEntry.progress" class="bar" max="100" value="100">`)
	outside := w.browser.Send("GET", "/entry-figures?table="+table+"&amount=1200&outside=true", nil).Body.String()
	has(t, outside, `<dd id="addEntry.remaining" class="money">1,000.00</dd>`)
	lacks(t, outside, `id="addEntry.overrun"`)
	half := w.browser.Send("GET", "/entry-figures?table="+table+"&amount=500", nil).Body.String()
	has(t, half, `<dd id="addEntry.remaining" class="money">500.00</dd>`)
	has(t, half, `value="50"`)
	typed := url.Values{"table": {table}, "date": {"2026-11-10"}, "amount": {"1200"}, "method": {w.method}, "account": {w.account}, "submit": {"send"}}
	has(t, w.browser.Send("POST", "/entry", typed).Body.String(), "حُفظت، والعقد متجاوَز.")
}

func TestWeb_R290_ForgotPasswordAndRegisterShareTheAppsPerAddressWindows(t *testing.T) {
	api := testapi.Limited(t, 2)
	api.Send("POST", "/auth/forgot-password", `{"login":"someone"}`)
	browser := api.Browser()
	if second := browser.Send("POST", "/forgot-password", url.Values{"login": {"another"}}); second.Code != http.StatusOK {
		t.Fatalf("the second ask from the address: %d", second.Code)
	}
	third := browser.Send("POST", "/forgot-password", url.Values{"login": {"a third"}})
	if third.Code != http.StatusUnprocessableEntity {
		t.Fatalf("the third ask from the address is refused: %d", third.Code)
	}
	has(t, third.Body.String(), "طلبات كثيرة. حاول بعد قليل.")
	for _, username := range []string{"manager.e", "manager.f"} {
		browser.Send("POST", "/register", registration(username))
	}
	refused := browser.Send("POST", "/register", registration("manager.g"))
	if refused.Code != http.StatusUnprocessableEntity {
		t.Fatalf("the third registration from the address is refused: %d", refused.Code)
	}
	has(t, refused.Body.String(), "محاولات كثيرة. حاول مرة أخرى لاحقاً.")
	if got := api.Count(`SELECT count(*) FROM users WHERE username = 'manager.g'`); got != 0 {
		t.Errorf("a refused registration keeps nothing: %d", got)
	}
}

func TestWeb_R167_UC032_ApprovalsListsWhatWaitsFilteredAndSearchedAsTheAppsDo(t *testing.T) {
	w := newWorking(t)
	first, second, decided := w.sent(40), w.sent(70), w.approved(90)
	w.home()
	list := w.browser.Send("GET", "/tab/approvals", nil).Body.String()
	has(t, list, `id="approvals.card.`+first+`"`)
	has(t, list, `id="approvals.card.`+second+`"`)
	lacks(t, list, `id="approvals.card.`+decided+`"`)
	has(t, list, "الكل · 2")
	has(t, w.browser.Send("GET", "/tab/approvals?filter=contractorPayments", nil).Body.String(), `<p id="approvals.empty" class="empty">لا شيء بانتظار الاعتماد.</p>`)
	searched := w.browser.Send("GET", "/tab/approvals?text=70", nil).Body.String()
	has(t, searched, `id="approvals.card.`+second+`"`)
	lacks(t, searched, `id="approvals.card.`+first+`"`)
}

func TestWeb_R167_FR19_ApproveSelectedApprovesTheTickedOnes(t *testing.T) {
	w := newWorking(t)
	first, second := w.sent(40), w.sent(70)
	w.home()
	w.browser.Send("GET", "/tab/approvals", nil)
	answered := w.browser.Send("POST", "/approvals/approve", url.Values{"entry": {first, uuid.NewString()}})
	if answered.Code != http.StatusOK {
		t.Fatalf("approving the ticked ones: %d", answered.Code)
	}
	lacks(t, answered.Body.String(), `id="approvals.card.`+first+`"`)
	has(t, answered.Body.String(), `id="approvals.card.`+second+`"`)
	if w.statusOf(first) != 2 || w.statusOf(second) != 1 {
		t.Errorf("the ticked one is approved, the other waits: %d %d", w.statusOf(first), w.statusOf(second))
	}
}

func TestWeb_R167_AC19_1_RejectAsksItsReasonAndTheRowLeavesApprovals(t *testing.T) {
	w := newWorking(t)
	id := w.sent(40)
	w.home()
	w.browser.Send("GET", "/tab/approvals", nil)
	has(t, w.browser.Send("GET", "/details/entry/"+id, nil).Body.String(), `hx-get="/details/entry/`+id+`?asking=reject"`)
	asking := w.browser.Send("GET", "/details/entry/"+id+"?asking=reject", nil).Body.String()
	has(t, asking, `id="reasonSheet.reason"`)
	has(t, asking, `value="صورة الفاتورة غير واضحة"`)
	none := w.browser.Send("POST", "/entry/"+id+"/reject", url.Values{"reason": {"  "}}).Body.String()
	has(t, none, "اذكر سبب الرفض.")
	has(t, none, `id="reasonSheet.reason"`)
	rejected := w.browser.Send("POST", "/entry/"+id+"/reject", url.Values{"quick": {"فاتورة مكررة"}}).Body.String()
	has(t, rejected, `<dd id="entry.reason" class="refused">فاتورة مكررة</dd>`)
	has(t, rejected, `<tr id="approvals.card.`+id+`" hx-swap-oob="delete"></tr>`)
	if w.statusOf(id) != 3 {
		t.Errorf("the entry is rejected: %d", w.statusOf(id))
	}
}

func TestWeb_R167_R7_ApprovalsIsLeftOutForWhoeverMayNotApprove(t *testing.T) {
	w := newWorking(t)
	id := w.sent(40)
	member, _ := w.Person("User B")
	w.Ask("POST", w.path+"/users/existing", `{"username":"`+member.String()+`"}`, w.token)
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {member.String()}, "password": {password}})
	shell := browser.Send("POST", "/project/"+w.id, nil).Body.String()
	lacks(t, shell, `id="web.item.approvals"`)
	opened := browser.Send("GET", "/tab/approvals", nil).Body.String()
	lacks(t, opened, `id="approvals.card.`+id+`"`)
	lacks(t, opened, `id="web.tab.approvals"`)
	if code := browser.Send("POST", "/approvals/approve", url.Values{"entry": {id}}).Code; code != http.StatusForbidden {
		t.Errorf("approving the ticked ones without the permission: %d", code)
	}
	if code := browser.Send("POST", "/entry/"+id+"/reject", url.Values{"reason": {"x"}}).Code; code != http.StatusForbidden {
		t.Errorf("rejecting without the permission: %d", code)
	}
	w.home()
	has(t, w.browser.Send("GET", "/", nil).Body.String(), `id="web.item.approvals"`)
}

func TestWeb_R167_ApprovalsAnswersAFailingDatabaseWith500(t *testing.T) {
	w := newWorking(t)
	id := w.sent(40)
	w.home()
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "GET", "/tab/approvals", nil)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("listing with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "POST", "/approvals/approve", url.Values{"entry": {id}})
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("approving with the database failing at its call %d answers %d", n, answered.Code)
		}
		if w.statusOf(id) != 1 {
			t.Fatalf("an approval failing at its call %d keeps nothing: %d", n, w.statusOf(id))
		}
	}
}

func TestWeb_R167_TheApprovalHistoryTellsEachDecisionNarrowedAsTheAppsIs(t *testing.T) {
	w := newWorking(t)
	w.approved(90)
	rejected := w.sent(40)
	if code, answer := w.Ask("POST", w.path+"/entries/"+rejected+"/reject", `{"reason":"الفاتورة غير واضحة"}`, w.token); code >= 300 {
		t.Fatalf("rejecting: %d %v", code, answer)
	}
	w.home()
	has(t, w.browser.Send("GET", "/tab/approvals", nil).Body.String(), `hx-get="/tab/approvalHistory"`)
	history := w.browser.Send("GET", "/tab/approvalHistory", nil).Body.String()
	has(t, history, `class="decision rejected">رفض: التسليح · 40.00، والسبب: الفاتورة غير واضحة</td>`)
	has(t, history, `class="decision approved">اعتماد: التسليح · 90.00</td>`)
	has(t, history, `class="who">اعتمده User A · 2026-11-16 13:00</td>`)
	has(t, history, `id="approvalHistory.person.`+w.manager.String()+`"`)
	searched := w.browser.Send("GET", "/tab/approvalHistory?text=الفاتورة", nil).Body.String()
	lacks(t, searched, "اعتماد: التسليح · 90.00")
	has(t, searched, "رفض: التسليح · 40.00")
	has(t, w.browser.Send("GET", "/tab/approvalHistory?kind=contractorPayment", nil).Body.String(), `<p id="approvalHistory.empty" class="empty">لا شيء بعد.</p>`)
	has(t, w.browser.Send("GET", "/tab/approvalHistory?by="+uuid.NewString(), nil).Body.String(), `id="approvalHistory.empty"`)
	lacks(t, history, `id="approvalHistory.more"`)
}

func TestWeb_R167_ShowMoreAddsTheNextPageOfTheApprovalHistory(t *testing.T) {
	w := newWorking(t)
	var ids []string
	for range 51 {
		ids = append(ids, `"`+w.sent(10)+`"`)
	}
	if code, answer := w.Ask("POST", w.path+"/entries/approve", `{"entryIds":[`+strings.Join(ids, ",")+`]}`, w.token); code != http.StatusOK {
		t.Fatalf("approving: %d %v", code, answer)
	}
	w.home()
	first := w.browser.Send("GET", "/tab/approvalHistory", nil).Body.String()
	if got := strings.Count(first, `id="approvalHistory.line.`); got != 50 {
		t.Fatalf("the first page holds 50 lines: %d", got)
	}
	at := strings.Index(first, `hx-get="/approval-history?`)
	if at < 0 {
		t.Fatal("a full page offers Show more")
	}
	next := first[at+len(`hx-get="`):]
	next = strings.ReplaceAll(next[:strings.Index(next, `"`)], "&amp;", "&")
	more := w.browser.Send("GET", next, nil).Body.String()
	if got := strings.Count(more, `id="approvalHistory.line.`); got != 1 {
		t.Errorf("the next page holds the one line left: %d", got)
	}
	has(t, more, `<button id="approvalHistory.more" hx-swap-oob="delete"></button>`)
}

func TestWeb_R167_TheApprovalHistoryIsLeftOutForWhoeverMayNotApprove(t *testing.T) {
	w := newWorking(t)
	w.approved(90)
	member, _ := w.Person("User B")
	w.Ask("POST", w.path+"/users/existing", `{"username":"`+member.String()+`"}`, w.token)
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {member.String()}, "password": {password}})
	browser.Send("POST", "/project/"+w.id, nil)
	lacks(t, browser.Send("GET", "/tab/approvalHistory", nil).Body.String(), `id="approvalHistory.title"`)
	if code := browser.Send("GET", "/approval-history?before=99", nil).Code; code != http.StatusNotFound {
		t.Errorf("a page of the history without the permission: %d", code)
	}
	w.home()
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "GET", "/tab/approvalHistory", nil)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("the history with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "GET", "/approval-history?before=99", nil)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("a page of the history with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
}

func TestWeb_R164_UC006_ASignedInPersonRegistersAProjectTheyManage(t *testing.T) {
	w := newWorking(t)
	w.home()
	has(t, w.browser.Send("GET", "/", nil).Body.String(), `<a id="projects.new" class="menu-link" href="#" hx-get="/tab/registerProject"`)
	form := w.browser.Send("GET", "/tab/registerProject", nil).Body.String()
	has(t, form, `value="`+w.id+`">مشروع A</option>`)
	lacks(t, form, `id="newProject.managerHeading"`)
	lacks(t, form, `id="newProject.password"`)
	typed := url.Values{"name": {"مشروع هـ"}, "startDate": {"2026-11-21"}, "unit": {"B1"}, "structureSource": {w.id}}
	created := w.browser.Send("POST", "/register", typed).Body.String()
	has(t, created, `<p id="newProject.notice" class="notice" role="status">مشروع هـ · بانتظار الاعتماد</p>`)
	has(t, created, `<div id="web.projects" hx-swap-oob="innerHTML">`)
	has(t, created, "مشروع هـ")
	if got := w.Count(`SELECT count(*) FROM projects WHERE name = 'مشروع هـ' AND status = 2`); got != 1 {
		t.Errorf("the project waits for the admin: %d", got)
	}
	if got := w.Count(`SELECT count(*) FROM project_structure_sources JOIN projects ON projects.id = project_structure_sources.project_id WHERE projects.name = 'مشروع هـ'`); got != 1 {
		t.Errorf("its structure comes from the project chosen: %d", got)
	}
	has(t, w.browser.Send("POST", "/register", url.Values{"name": {" "}, "startDate": {"2026-11-21"}}).Body.String(), "اكتب اسماً من 1 إلى 100 حرف.")
	has(t, w.browser.Send("POST", "/register", url.Values{"name": {"مشروع و"}, "startDate": {"2026-11-21"}, "structureSource": {uuid.NewString()}}).Body.String(),
		"لا يمكنك النسخ من ذلك المشروع.")
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "POST", "/register", url.Values{"name": {"مشروع ز"}, "startDate": {"2026-11-21"}})
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("registering with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
}

func TestWeb_R166_D1_AnEntryWithAFileSaysItHasOne(t *testing.T) {
	w := newWorking(t)
	code, uploaded := w.AskWithFile(w.path+"/attachments", "invoice-31.pdf", []byte("%PDF-1.4 invoice"), w.token, nil)
	if code >= 300 {
		t.Fatalf("uploading: %d %v", code, uploaded)
	}
	id := uuid.Must(uuid.NewV7()).String()
	body := `{"id":"` + id + `","itemId":"` + w.item + `","tableId":"` + w.table + `","date":"2026-11-10","amount":60,"accountId":"` + w.account +
		`","paymentMethodId":"` + w.method + `","description":"","columnValues":{},"attachmentId":"` + uploaded["id"].(string) + `","submit":true}`
	if code, answer := w.Ask("POST", w.path+"/entries", body, w.token); code != http.StatusCreated {
		t.Fatalf("recording: %d %v", code, answer)
	}
	w.home()
	panel := w.browser.Send("GET", "/details/entry/"+id, nil).Body.String()
	has(t, panel, `<dt id="entry.file.0.label">فاتورة</dt><dd id="entry.file.0" class="file">invoice-31.pdf</dd>`)
	lacks(t, w.browser.Send("GET", "/details/entry/"+w.sent(40), nil).Body.String(), `id="entry.file.0"`)
}

// accountNamed is the id of the project's account called name.
func (w working) accountNamed(name string) string {
	w.t.Helper()
	var id string
	if err := w.Database.Read.QueryRow("SELECT id FROM accounts WHERE project_id = ? AND name = ?", w.id, name).Scan(&id); err != nil {
		w.t.Fatal(err)
	}
	return id
}

func TestWeb_R168_UC015_AccountsListsTheAccountsByTypeWithTheLastCount(t *testing.T) {
	w := newWorking(t)
	w.home()
	list := w.browser.Send("GET", "/tab/accounts", nil).Body.String()
	has(t, list, `<th id="accounts.group.bank" colspan="2">الحسابات البنكية</th>`)
	has(t, list, `<span id="accounts.name.`+w.account+`">حساب المؤسسة</span>`)
	has(t, list, `<p id="reconciliation.never" class="hint">لا مطابقة بعد.</p>`)
	has(t, list, `id="reconciliation.surplus"`)
	panel := w.browser.Send("GET", "/details/account/"+w.account, nil).Body.String()
	has(t, panel, `<h3 id="account.title">حساب المؤسسة</h3>`)
	has(t, panel, `hx-post="/account/`+w.account+`/disable"`)
	if code := w.browser.Send("GET", "/details/account/"+uuid.NewString(), nil).Code; code != http.StatusNotFound {
		t.Errorf("an account not in the project: %d", code)
	}
}

func TestWeb_R168_BR15_DisableEnableAndDeleteAsTheAppsRoutes(t *testing.T) {
	w := newWorking(t)
	w.Ask("POST", w.path+"/accounts", `{"name":"حساب احتياطي","type":"bank"}`, w.token)
	spare := w.accountNamed("حساب احتياطي")
	w.sent(40)
	w.home()
	w.browser.Send("GET", "/tab/accounts", nil)
	disabled := w.browser.Send("POST", "/account/"+spare+"/disable", nil).Body.String()
	has(t, disabled, `hx-post="/account/`+spare+`/enable"`)
	has(t, disabled, `<tr id="accounts.row.`+spare+`" class="disabled"`)
	has(t, w.browser.Send("POST", "/account/"+spare+"/enable", nil).Body.String(), `hx-post="/account/`+spare+`/disable"`)
	used := w.browser.Send("GET", "/details/account/"+w.account, nil).Body.String()
	has(t, used, `<p id="accounts.deleteReason" class="hint details-say">مستخدم في قيود أو مطابقات أو تحويلات، فيمكن تعطيله فقط.</p>`)
	has(t, w.browser.Send("POST", "/account/"+w.account+"/delete", nil).Body.String(), `<p id="account.refusal" class="refusal details-say" role="alert">مستخدم في قيود`)
	has(t, w.browser.Send("GET", "/details/account/"+spare+"?asking=delete", nil).Body.String(), "حذف الحساب؟ لا يمكن التراجع عن الحذف.")
	deleted := w.browser.Send("POST", "/account/"+spare+"/delete", nil).Body.String()
	has(t, deleted, `<h3 id="account.title">تم حذف الحساب.</h3>`)
	has(t, deleted, `<template><tr id="accounts.row.`+spare+`" hx-swap-oob="delete"></tr></template>`)
	if got := w.Count("SELECT count(*) FROM accounts WHERE id = ?", spare); got != 0 {
		t.Errorf("the account is deleted: %d", got)
	}
	if code := w.browser.Send("POST", "/account/"+w.account+"/archive", nil).Code; code != http.StatusNotFound {
		t.Errorf("an action there is none of: %d", code)
	}
}

func TestWeb_R168_AnAccountsActionsAreGatedAsTheAppsRoutesAre(t *testing.T) {
	w := newWorking(t)
	member, _ := w.Person("User B")
	w.Ask("POST", w.path+"/users/existing", `{"username":"`+member.String()+`"}`, w.token)
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {member.String()}, "password": {password}})
	browser.Send("POST", "/project/"+w.id, nil)
	lacks(t, browser.Send("GET", "/details/account/"+w.account, nil).Body.String(), `hx-post="/account/`)
	if code := browser.Send("POST", "/account/"+w.account+"/disable", nil).Code; code != http.StatusForbidden {
		t.Errorf("disabling without ManageSettings: %d", code)
	}
	w.home()
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "POST", "/account/"+w.account+"/disable", nil)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("disabling with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "GET", "/tab/accounts", nil)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("listing with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
	if code, answer := w.Ask("POST", w.path+"/close", ``, w.token); code >= 300 {
		t.Fatalf("closing: %d %v", code, answer)
	}
	has(t, w.browser.Send("POST", "/account/"+w.account+"/disable", nil).Body.String(), "هذا المشروع منتهٍ، فلا يمكن تغييره.")
}

func TestWeb_R168_UC015_NewAccountAndEditAsTheAppsDialog(t *testing.T) {
	w := newWorking(t)
	w.home()
	has(t, w.browser.Send("GET", "/tab/accounts", nil).Body.String(), `hx-get="/details/account/new"`)
	form := w.browser.Send("GET", "/details/account/new", nil).Body.String()
	has(t, form, `id="accounts.type.cashCustody"`)
	has(t, form, `value="`+w.manager.String()+`">User A</option>`)
	custody := url.Values{"name": {"صندوق الموقع"}, "type": {"cashCustody"}, "custodian": {""}}
	has(t, w.browser.Send("POST", "/account", custody).Body.String(), "العهدة النقدية تحتاج إلى صاحب عهدة.")
	has(t, w.browser.Send("POST", "/account", url.Values{"name": {" "}, "type": {"bank"}}).Body.String(), "أدخل اسماً من 1 إلى 100 حرف.")
	custody.Set("custodian", w.manager.String())
	added := w.browser.Send("POST", "/account", custody).Body.String()
	has(t, added, `<h3 id="account.title">صندوق الموقع</h3>`)
	has(t, added, `<div id="web.content" hx-swap-oob="innerHTML">`)
	has(t, added, `<th id="accounts.group.cashCustody" colspan="2">العهد النقدية</th>`)
	editing := w.browser.Send("GET", "/details/account/"+w.account+"?asking=edit", nil).Body.String()
	has(t, editing, `value="حساب المؤسسة"`)
	lacks(t, editing, `id="accounts.type.bank"`)
	edited := w.browser.Send("POST", "/account/"+w.account+"/edit", url.Values{"name": {"الحساب الجاري"}, "custodian": {""}}).Body.String()
	has(t, edited, `<h3 id="account.title">الحساب الجاري</h3>`)
	if got := w.Count("SELECT count(*) FROM accounts WHERE id = ? AND name = 'الحساب الجاري'", w.account); got != 1 {
		t.Errorf("the account is renamed: %d", got)
	}
	member, _ := w.Person("User B")
	w.Ask("POST", w.path+"/users/existing", `{"username":"`+member.String()+`"}`, w.token)
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {member.String()}, "password": {password}})
	browser.Send("POST", "/project/"+w.id, nil)
	if code := browser.Send("GET", "/details/account/new", nil).Code; code != http.StatusNotFound {
		t.Errorf("New account without ManageSettings: %d", code)
	}
	if code := browser.Send("POST", "/account", url.Values{"name": {"x"}, "type": {"bank"}}).Code; code != http.StatusForbidden {
		t.Errorf("adding without ManageSettings: %d", code)
	}
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "POST", "/account", url.Values{"name": {"حساب آخر"}, "type": {"bank"}})
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("adding with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
}

func TestWeb_R168_TheAccountFormRefusesAndFailsAsTheAppsRoutes(t *testing.T) {
	w := newWorking(t)
	w.home()
	has(t, w.browser.Send("POST", "/account", url.Values{"name": {"صندوق"}, "type": {"cashCustody"}, "custodian": {uuid.NewString()}}).Body.String(), "هذا الشخص ليس في هذا المشروع.")
	if code := w.browser.Send("POST", "/account/"+uuid.NewString()+"/edit", url.Values{"name": {"x"}}).Code; code != http.StatusNotFound {
		t.Errorf("editing an account not in the project: %d", code)
	}
	for _, path := range []string{"/details/account/new", "/details/account/" + w.account, "/details/account/" + w.account + "?asking=edit"} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, "GET", path, nil)
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s with the database failing at its call %d answers %d", path, n, answered.Code)
			}
		}
	}
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "POST", "/account/"+w.account+"/edit", url.Values{"name": {"الحساب الجاري"}})
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("editing with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
	if code, answer := w.Ask("POST", w.path+"/close", ``, w.token); code >= 300 {
		t.Fatalf("closing: %d %v", code, answer)
	}
	has(t, w.browser.Send("POST", "/account/"+w.account+"/edit", url.Values{"name": {"بعد الإغلاق"}}).Body.String(), "هذا المشروع منتهٍ، فلا يمكن تغييره.")
	if got := w.Count("SELECT count(*) FROM accounts WHERE name = 'بعد الإغلاق'"); got != 0 {
		t.Errorf("a finished project keeps nothing: %d", got)
	}
}

// funded is the bank account holding amount more, by a revenue recorded into it and approved.
func (w working) funded(amount int) {
	w.t.Helper()
	_, category := w.Ask("POST", w.path+"/categories", `{"name":"الإيرادات"}`, w.token)
	code, item := w.Ask("POST", w.path+"/categories/"+testapi.Field(category, "id").(string)+"/items", `{"name":"البيع","type":"sale"}`, w.token)
	if code >= 300 {
		w.t.Fatalf("the revenue item: %d %v", code, item)
	}
	itemID := testapi.Field(item, "id").(string)
	var table string
	if err := w.Database.Read.QueryRow("SELECT id FROM subgroups WHERE item_id = ?", itemID).Scan(&table); err != nil {
		w.t.Fatal(err)
	}
	id := uuid.Must(uuid.NewV7()).String()
	body := `{"id":"` + id + `","itemId":"` + itemID + `","tableId":"` + table + `","date":"2026-11-10","amount":` + strconv.Itoa(amount) + `,"accountId":"` +
		w.account + `","description":"","columnValues":{},"submit":true}`
	if code, answer := w.Ask("POST", w.path+"/entries", body, w.token); code != http.StatusCreated {
		w.t.Fatalf("the revenue: %d %v", code, answer)
	}
	if code, answer := w.Ask("POST", w.path+"/entries/approve", `{"entryIds":["`+id+`"]}`, w.token); code != http.StatusOK {
		w.t.Fatalf("approving the revenue: %d %v", code, answer)
	}
}

func TestWeb_R168_FR23_TransferMovesMoneyBetweenTwoActiveAccounts(t *testing.T) {
	w := newWorking(t)
	w.home()
	has(t, w.browser.Send("GET", "/tab/accounts", nil).Body.String(), `id="accounts.transfer" class="quiet-button" type="button" disabled`)
	w.Ask("POST", w.path+"/accounts", `{"name":"حساب احتياطي","type":"bank"}`, w.token)
	spare := w.accountNamed("حساب احتياطي")
	w.funded(500)
	form := w.browser.Send("GET", "/details/transfer", nil).Body.String()
	has(t, form, `<small id="transfer.balanceNow" class="hint">رصيده الآن: 500.00</small>`)
	has(t, form, `value="`+spare+`">حساب احتياطي</option>`)
	typed := url.Values{"from": {w.account}, "to": {spare}, "amount": {"600"}, "date": {"2026-11-15"}, "notes": {""}}
	has(t, w.browser.Send("POST", "/transfer", typed).Body.String(), "المبلغ أكبر من رصيد الحساب الآن.")
	typed.Set("amount", "200")
	moved := w.browser.Send("POST", "/transfer", typed).Body.String()
	has(t, moved, `<div id="web.content" hx-swap-oob="innerHTML">`)
	has(t, moved, `<td id="accounts.balance.`+spare+`" class="amount">200.00</td>`)
	has(t, moved, `<td id="accounts.balance.`+w.account+`" class="amount">300.00</td>`)
	member, _ := w.Person("User B")
	w.Ask("POST", w.path+"/users/existing", `{"username":"`+member.String()+`"}`, w.token)
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {member.String()}, "password": {password}})
	browser.Send("POST", "/project/"+w.id, nil)
	if code := browser.Send("POST", "/transfer", typed).Code; code != http.StatusForbidden {
		t.Errorf("transferring without MoveMoney: %d", code)
	}
	if code := browser.Send("GET", "/details/transfer", nil).Code; code != http.StatusNotFound {
		t.Errorf("Transfer without MoveMoney: %d", code)
	}
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "GET", "/details/transfer", nil)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("Transfer with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
	typed.Set("amount", "10")
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "POST", "/transfer", typed)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("transferring with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
}

// TestWeb_R4_EveryElementATestReadsHasAnId reads every template of the web: each element a test clicks, types into or reads - a label, a field,
// a button, a link, an option, a heading, a line of text, a caption and its value, a table and its cells, a hint - carries an id, so the
// automation finds it at once by it (#80 D13; the owner, 22 Nov 2026). Only layout - a div, a table's body - and a hidden input go without one.
func TestWeb_R4_EveryElementATestReadsHasAnId(t *testing.T) {
	tags := `label|button|input|select|textarea|a|option|h1|h2|h3|h4|p|dt|dd|th|td|legend|small|span|b|strong|kbd|ul|li|table|progress|form|fieldset|section|aside|details|summary`
	element := regexp.MustCompile(`<(` + tags + `)\b([^>]*)>`)
	files, err := filepath.Glob("*.templ")
	if err != nil || len(files) == 0 {
		t.Fatalf("the templates: %v %v", files, err)
	}
	for _, file := range files {
		text, err := os.ReadFile(file)
		if err != nil {
			t.Fatal(err)
		}
		for _, found := range element.FindAllSubmatchIndex(text, -1) {
			attributes := string(text[found[4]:found[5]])
			if !strings.Contains(attributes, "id=") && !strings.Contains(attributes, `type="hidden"`) {
				t.Errorf("%s:%d: <%s> has no id", file, bytes.Count(text[:found[0]], []byte("\n"))+1, text[found[2]:found[3]])
			}
		}
	}
}

func TestWeb_R168_FR24_TheCountKeepsTheActualBalancesAndAccountsShowsThem(t *testing.T) {
	w := newWorking(t)
	w.home()
	has(t, w.browser.Send("GET", "/tab/accounts", nil).Body.String(), `hx-get="/tab/reconcile"`)
	page := w.browser.Send("GET", "/tab/reconcile", nil).Body.String()
	has(t, page, `id="reconcile.balance.`+w.account+`" name="balance.`+w.account+`"`)
	has(t, page, `<small id="reconcile.books.`+w.account+`" class="hint">في الدفاتر: 0.00</small>`)
	has(t, w.browser.Send("POST", "/count", url.Values{"balance." + w.account: {""}}).Body.String(), "اكتب رصيد كل حساب.")
	figures := w.browser.Send("GET", "/count-figures?balance."+w.account+"=120", nil).Body.String()
	has(t, figures, `<p id="reconcile.total">مجموع الأرصدة الفعلية: 120.00</p>`)
	has(t, figures, `<p id="reconcile.difference" class="refused">فرق المطابقة: 120.00</p>`)
	kept := w.browser.Send("POST", "/count", url.Values{"balance." + w.account: {"120"}}).Body.String()
	has(t, kept, `<h2 id="accounts.title">الحسابات</h2>`)
	has(t, kept, `<dd id="reconciliation.actualTotal" class="money">120.00</dd>`)
	lacks(t, kept, `id="web.tab.reconcile"`)
	has(t, w.browser.Send("GET", "/tab/reconcile", nil).Body.String(), `value="120"`)
	if got := w.Count("SELECT count(*) FROM reconciliations WHERE project_id = ?", w.id); got != 1 {
		t.Errorf("one check is kept: %d", got)
	}
}

func TestWeb_R168_TheCountIsLeftToWhoeverMayUpdateIt(t *testing.T) {
	w := newWorking(t)
	member, _ := w.Person("User B")
	w.Ask("POST", w.path+"/users/existing", `{"username":"`+member.String()+`"}`, w.token)
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {member.String()}, "password": {password}})
	browser.Send("POST", "/project/"+w.id, nil)
	has(t, browser.Send("GET", "/tab/reconcile", nil).Body.String(), `<p id="reconcile.message" class="hint">`)
	if code := browser.Send("POST", "/count", url.Values{"balance." + w.account: {"5"}}).Code; code != http.StatusForbidden {
		t.Errorf("counting without MoveMoney: %d", code)
	}
	if code := browser.Send("GET", "/count-figures", nil).Code; code != http.StatusNotFound {
		t.Errorf("the figures without MoveMoney: %d", code)
	}
	w.home()
	for _, path := range []string{"/tab/reconcile", "/count-figures?balance." + w.account + "=5"} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, "GET", path, nil)
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s with the database failing at its call %d answers %d", path, n, answered.Code)
			}
		}
	}
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "POST", "/count", url.Values{"balance." + w.account: {"5"}})
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("counting with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
}

func TestWeb_R168_TheAccountHistoryTellsEachLineAsTheAppsDoes(t *testing.T) {
	w := newWorking(t)
	w.funded(500)
	w.Ask("POST", w.path+"/accounts", `{"name":"حساب احتياطي","type":"bank"}`, w.token)
	spare := w.accountNamed("حساب احتياطي")
	w.Ask("POST", w.path+"/accounts/"+spare+"/disable", ``, w.token)
	w.home()
	has(t, w.browser.Send("GET", "/tab/accounts", nil).Body.String(), `hx-get="/tab/accountHistory"`)
	history := w.browser.Send("GET", "/tab/accountHistory", nil).Body.String()
	has(t, history, `class="decision accountDisabled">تعطيل حساب احتياطي</td>`)
	has(t, history, `class="decision accountCreated">إضافة الحساب حساب احتياطي</td>`)
	has(t, history, `class="decision entryIn">إيرادات 500.00 إلى حساب المؤسسة</td>`)
	has(t, history, `class="who">اعتمده User A · 2026-11-16 13:00</td>`)
	has(t, history, `id="accountHistory.account.`+spare+`"`)
	one := w.browser.Send("GET", "/tab/accountHistory?account="+spare, nil).Body.String()
	has(t, one, "تعطيل حساب احتياطي")
	lacks(t, one, "إيرادات 500.00")
	has(t, w.browser.Send("GET", "/tab/accountHistory?account="+uuid.NewString(), nil).Body.String(), `id="accountHistory.empty"`)
	searched := w.browser.Send("GET", "/tab/accountHistory?text=500", nil).Body.String()
	has(t, searched, "إيرادات 500.00")
	lacks(t, searched, "تعطيل حساب احتياطي")
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "GET", "/tab/accountHistory", nil)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("the account history with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "GET", "/account-history?before=999999", nil)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("a page of the account history with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
}

func TestWeb_R168_EveryKindOfAccountLineIsSaidAsTheAppsIs(t *testing.T) {
	w := newWorking(t)
	w.funded(500)
	w.Ask("POST", w.path+"/accounts", `{"name":"حساب احتياطي","type":"bank"}`, w.token)
	spare := w.accountNamed("حساب احتياطي")
	w.Ask("PUT", w.path+"/accounts/"+spare, `{"name":"الحساب الثاني"}`, w.token)
	w.Ask("PUT", w.path+"/accounts/"+spare, `{"name":"الحساب الثاني","custodianId":"`+w.manager.String()+`"}`, w.token)
	w.Ask("PUT", w.path+"/accounts/"+spare, `{"name":"الحساب الثاني"}`, w.token)
	if code, answer := w.Ask("POST", w.path+"/transfers", `{"fromAccountId":"`+w.account+`","toAccountId":"`+spare+`","amount":100,"date":"2026-11-15","notes":""}`, w.token); code >= 300 {
		t.Fatalf("transferring: %d %v", code, answer)
	}
	w.approved(40)
	w.home()
	if code := w.browser.Send("POST", "/count", url.Values{"balance." + w.account: {"360"}, "balance." + spare: {"100"}}).Code; code != http.StatusOK {
		t.Fatalf("counting: %d", code)
	}
	history := w.browser.Send("GET", "/tab/accountHistory", nil).Body.String()
	for _, line := range []string{
		`class="decision accountChanged">تعديل الحساب الثاني: الاسم كان حساب احتياطي</td>`,
		`class="decision custodianChanged">الحساب الثاني أصبح مع User A</td>`,
		`class="decision custodianChanged">الحساب الثاني أصبح مع لا أحد</td>`,
		`class="decision transfer">تحويل 100.00 من حساب المؤسسة إلى الحساب الثاني</td>`,
		`class="decision entryOut">مشتريات 40.00 من حساب المؤسسة</td>`,
		`class="decision check">مطابقة الأرصدة الفعلية: 460.00</td>`,
	} {
		has(t, history, line)
	}
}

// partner adds a partner called name, with login as their own login when it is not empty; it is the partner's id.
func (w working) partner(name, login string) string {
	w.t.Helper()
	body := `{"name":"` + name + `"}`
	if login != "" {
		body = `{"name":"` + name + `","userId":"` + login + `"}`
	}
	if code, answer := w.Ask("POST", w.path+"/partners", body, w.token); code >= 300 {
		w.t.Fatalf("the partner: %d %v", code, answer)
	}
	var id string
	if err := w.Database.Read.QueryRow("SELECT id FROM partners WHERE project_id = ? AND name = ?", w.id, name).Scan(&id); err != nil {
		w.t.Fatal(err)
	}
	return id
}

// capitalIn records amount the partner partnerID puts into the bank account, described as description, and approves it.
func (w working) capitalIn(partnerID string, amount int, description string) {
	w.t.Helper()
	var item string
	if err := w.Database.Read.QueryRow("SELECT items.id FROM items JOIN categories ON categories.id = items.category_id WHERE categories.project_id = ? AND items.type = 6", w.id).Scan(&item); err != nil {
		w.t.Fatal(err)
	}
	id := uuid.Must(uuid.NewV7()).String()
	body := `{"id":"` + id + `","itemId":"` + item + `","tableId":"` + partnerID + `","date":"2026-11-10","amount":` + strconv.Itoa(amount) + `,"accountId":"` + w.account +
		`","description":"` + description + `","columnValues":{},"submit":true}`
	if code, answer := w.Ask("POST", w.path+"/entries", body, w.token); code != http.StatusCreated {
		w.t.Fatalf("the movement: %d %v", code, answer)
	}
	if code, answer := w.Ask("POST", w.path+"/entries/approve", `{"entryIds":["`+id+`"]}`, w.token); code != http.StatusOK {
		w.t.Fatalf("approving the movement: %d %v", code, answer)
	}
}

func TestWeb_R169_UC040_PartnersListsTheCapitalAndEachPartnersShare(t *testing.T) {
	w := newWorking(t)
	first, second := w.partner("شريك أ", w.manager.String()), w.partner("شريك ب", "")
	w.capitalIn(first, 750, "دفعة أولى")
	w.capitalIn(second, 250, "")
	w.home()
	list := w.browser.Send("GET", "/tab/partners", nil).Body.String()
	has(t, list, `<dd id="partners.total" class="money">1,000.00</dd>`)
	has(t, list, `<span id="partners.name.`+first+`">شريك أ</span>`)
	has(t, list, `<small id="partners.details.`+first+`" class="hint">User A</small>`)
	has(t, list, `<small id="partners.details.`+second+`" class="hint">بدون حساب دخول</small>`)
	has(t, list, `<td id="partners.share.`+first+`">75.0000000000%</td>`)
	has(t, list, `<td id="partners.balance.`+second+`" class="amount">250.00</td>`)
	if strings.Index(list, `partners.row.`+first) > strings.Index(list, `partners.row.`+second) {
		t.Error("the largest balance comes first")
	}
	panel := w.browser.Send("GET", "/details/partner/"+first, nil).Body.String()
	has(t, panel, `<h3 id="partner.title">شريك أ</h3>`)
	has(t, panel, "دفعة أولى")
	has(t, panel, `class="money">750.00</b>`)
	has(t, w.browser.Send("GET", "/tab/partners", nil).Body.String(), `hx-get="/details/partner/`+second+`"`)
	if code := w.browser.Send("GET", "/details/partner/"+uuid.NewString(), nil).Code; code != http.StatusNotFound {
		t.Errorf("a partner not in the project: %d", code)
	}
	for _, path := range []string{"/tab/partners", "/details/partner/" + first} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, "GET", path, nil)
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s with the database failing at its call %d answers %d", path, n, answered.Code)
			}
		}
	}
}

func TestWeb_R169_UC016_NewPartnerEditAndDeleteAsTheAppsDialogs(t *testing.T) {
	w := newWorking(t)
	holding := w.partner("شريك أ", "")
	w.capitalIn(holding, 300, "")
	w.home()
	has(t, w.browser.Send("GET", "/tab/partners", nil).Body.String(), `hx-get="/details/partner/new"`)
	form := w.browser.Send("GET", "/details/partner/new", nil).Body.String()
	has(t, form, `<option id="partners.login.option.`+w.manager.String()+`" value="`+w.manager.String()+`">User A</option>`)
	has(t, w.browser.Send("POST", "/partner", url.Values{"name": {" "}}).Body.String(), "أدخل اسماً من 1 إلى 100 حرف.")
	added := w.browser.Send("POST", "/partner", url.Values{"name": {"شريك ج"}, "login": {w.manager.String()}}).Body.String()
	has(t, added, `<h3 id="partner.title">شريك ج</h3>`)
	has(t, added, `<div id="web.content" hx-swap-oob="innerHTML">`)
	has(t, w.browser.Send("POST", "/partner", url.Values{"name": {"شريك د"}, "login": {w.manager.String()}}).Body.String(), "حساب الدخول هذا لشريك آخر في هذا المشروع.")
	newcomer := w.partnerNamed("شريك ج")
	has(t, w.browser.Send("GET", "/details/partner/"+newcomer+"?asking=edit", nil).Body.String(), `value="شريك ج"`)
	has(t, w.browser.Send("POST", "/partner/"+newcomer+"/edit", url.Values{"name": {"شريك ج الجديد"}, "login": {""}}).Body.String(), `<h3 id="partner.title">شريك ج الجديد</h3>`)
	held := w.browser.Send("GET", "/details/partner/"+holding, nil).Body.String()
	has(t, held, `id="partners.delete" class="quiet-button" type="button" disabled`)
	has(t, held, "لا يُحذف الشريك إلا ورأس ماله صفر ولا ينتظر له قيد.")
	has(t, w.browser.Send("GET", "/details/partner/"+newcomer+"?asking=delete", nil).Body.String(), "حذف الشريك؟")
	deleted := w.browser.Send("POST", "/partner/"+newcomer+"/delete", nil).Body.String()
	has(t, deleted, `<h3 id="partner.title">تم حذف الشريك.</h3>`)
	lacks(t, deleted, `id="partners.row.`+newcomer+`"`)
	has(t, w.browser.Send("POST", "/partner/"+holding+"/delete", nil).Body.String(), `<p id="partner.refusal" class="refusal details-say" role="alert">لا يُحذف الشريك`)
	if code := w.browser.Send("POST", "/partner/"+holding+"/archive", nil).Code; code != http.StatusNotFound {
		t.Errorf("an action there is none of: %d", code)
	}
	if code := w.browser.Send("POST", "/partner/"+uuid.NewString()+"/edit", url.Values{"name": {"x"}}).Code; code != http.StatusNotFound {
		t.Errorf("a partner not in the project: %d", code)
	}
}

// partnerNamed is the id of the project's partner called name.
func (w working) partnerNamed(name string) string {
	w.t.Helper()
	var id string
	if err := w.Database.Read.QueryRow("SELECT id FROM partners WHERE project_id = ? AND name = ?", w.id, name).Scan(&id); err != nil {
		w.t.Fatal(err)
	}
	return id
}

func TestWeb_R169_APartnersChangesAreGatedAsTheAppsRoutesAre(t *testing.T) {
	w := newWorking(t)
	partner := w.partner("شريك أ", "")
	member, _ := w.Person("User B")
	w.Ask("POST", w.path+"/users/existing", `{"username":"`+member.String()+`"}`, w.token)
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {member.String()}, "password": {password}})
	browser.Send("POST", "/project/"+w.id, nil)
	if code := browser.Send("GET", "/details/partner/new", nil).Code; code != http.StatusNotFound {
		t.Errorf("New partner without ManageSettings: %d", code)
	}
	if code := browser.Send("POST", "/partner", url.Values{"name": {"x"}}).Code; code != http.StatusForbidden {
		t.Errorf("adding without ManageSettings: %d", code)
	}
	w.home()
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "POST", "/partner", url.Values{"name": {"شريك ب"}})
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("adding with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
	for _, action := range []string{"edit", "delete"} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, "POST", "/partner/"+partner+"/"+action, url.Values{"name": {"شريك أ"}})
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s with the database failing at its call %d answers %d", action, n, answered.Code)
			}
		}
	}
	partner = w.partner("شريك ب", "")
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "GET", "/details/partner/"+partner+"?asking=edit", nil)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("Edit with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
	if code, answer := w.Ask("POST", w.path+"/close", ``, w.token); code >= 300 {
		t.Fatalf("closing: %d %v", code, answer)
	}
	has(t, w.browser.Send("POST", "/partner/"+partner+"/edit", url.Values{"name": {"بعد الإغلاق"}}).Body.String(), "هذا المشروع منتهٍ، فلا يمكن تغييره.")
}

func TestWeb_R169_ThePartnerHistoryTellsEachLineAsTheAppsDoes(t *testing.T) {
	w := newWorking(t)
	first, second := w.partner("شريك أ", ""), w.partner("شريك ب", "")
	w.Ask("PUT", w.path+"/partners/"+first, `{"name":"الشريك الأول"}`, w.token)
	w.Ask("PUT", w.path+"/partners/"+first, `{"name":"الشريك الأول","userId":"`+w.manager.String()+`"}`, w.token)
	w.capitalIn(first, 600, "")
	w.Ask("DELETE", w.path+"/partners/"+second, ``, w.token)
	w.home()
	has(t, w.browser.Send("GET", "/tab/partners", nil).Body.String(), `hx-get="/tab/partnerHistory"`)
	history := w.browser.Send("GET", "/tab/partnerHistory", nil).Body.String()
	for _, line := range []string{
		`class="decision partnerCreated">إضافة الشريك شريك ب</td>`,
		`class="decision partnerChanged">تعديل الشريك الشريك الأول: الاسم كان شريك أ</td>`,
		`class="decision partnerAccount">حساب الشريك الشريك الأول: User A</td>`,
		`class="decision capitalIn">رأس مال 600.00 من الشريك الأول إلى حساب المؤسسة</td>`,
		`class="decision partnerDeleted">حذف الشريك شريك ب</td>`,
		`class="who">اعتمده User A · 2026-11-16 13:00</td>`,
	} {
		has(t, history, line)
	}
	one := w.browser.Send("GET", "/tab/partnerHistory?partner="+first, nil).Body.String()
	has(t, one, "رأس مال 600.00")
	lacks(t, one, "شريك ب")
	has(t, w.browser.Send("GET", "/tab/partnerHistory?partner="+uuid.NewString(), nil).Body.String(), `id="partnerHistory.empty"`)
	for _, path := range []string{"/tab/partnerHistory", "/partner-history?before=999999"} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, "GET", path, nil)
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s with the database failing at its call %d answers %d", path, n, answered.Code)
			}
		}
	}
}

// supplier adds the supplier body types to the working project through the app's route; it is the supplier's id.
func (w working) supplier(body string) string {
	w.t.Helper()
	code, answer := w.Ask("POST", w.path+"/suppliers", body, w.token)
	if code >= 300 {
		w.t.Fatalf("the supplier: %d %v", code, answer)
	}
	return testapi.Field(answer, "id").(string)
}

func TestWeb_R170_UC017_SuppliersListsTheRegisterAsTheAppsScreen(t *testing.T) {
	w := newWorking(t)
	steel := w.supplier(`{"name":"مؤسسة الحديد","vatNumber":"300111","mobile":"0500000001","defaultItemId":"` + w.item + `"}`)
	gone := w.supplier(`{"name":"مؤسسة قديمة"}`)
	w.Ask("POST", w.path+"/suppliers/"+gone+"/disable", ``, w.token)
	member, memberToken := w.Person("User B")
	w.Ask("POST", w.path+"/users/existing", `{"username":"`+member.String()+`"}`, w.token)
	w.Ask("POST", w.path+"/users/"+member.String()+"/permissions/AddSupplier", ``, w.token)
	_, waiting := w.Ask("POST", w.path+"/suppliers", `{"name":"مورد من الموقع"}`, memberToken)
	w.home()
	has(t, w.browser.Send("GET", "/", nil).Body.String(), `id="web.item.suppliers"`)
	list := w.browser.Send("GET", "/tab/suppliers", nil).Body.String()
	has(t, list, `<small id="suppliers.details.`+steel+`" class="hint">300111 · التسليح</small>`)
	has(t, list, `<tr id="suppliers.row.`+gone+`" class="disabled"`)
	has(t, list, `<small id="suppliers.details.`+gone+`" class="hint">معطّل</small>`)
	has(t, list, "مورد نقدي عام")
	has(t, list, `<small id="suppliers.details.`+testapi.Field(waiting, "id").(string)+`" class="hint">بانتظار الاعتماد</small>`)
	has(t, list, `<button id="suppliers.add" class="primary small" type="button" title="`)
	found := w.browser.Send("GET", "/tab/suppliers?text=0500000001", nil).Body.String()
	has(t, found, "مؤسسة الحديد")
	lacks(t, found, "مؤسسة قديمة")
	has(t, w.browser.Send("GET", "/tab/suppliers?text=لا+أحد", nil).Body.String(), `<p id="suppliers.empty" class="empty">لا يوجد موردون.</p>`)
	panel := w.browser.Send("GET", "/details/supplier/"+steel, nil).Body.String()
	for _, part := range []string{`<h3 id="supplier.title">مؤسسة الحديد</h3>`, `<dd id="supplier.vatNumber">300111</dd>`, `<dd id="supplier.mobile"><bdi>0500000001</bdi></dd>`,
		`<dd id="supplier.defaultItem">التسليح</dd>`, `id="suppliers.edit"`, `id="suppliers.disable"`} {
		has(t, panel, part)
	}
	has(t, w.browser.Send("GET", "/details/supplier/"+gone, nil).Body.String(), `<dd id="supplier.defaultItem">بدون بند افتراضي</dd>`)
	if code := w.browser.Send("GET", "/details/supplier/"+uuid.NewString(), nil).Code; code != http.StatusNotFound {
		t.Errorf("a supplier not in the register: %d", code)
	}
}

func TestWeb_R170_UC017_NewSupplierEditDisableAndEnableAsTheAppsDialog(t *testing.T) {
	w := newWorking(t)
	w.home()
	form := w.browser.Send("GET", "/details/supplier/new", nil).Body.String()
	has(t, form, `<option id="suppliers.defaultItem.option.`+w.item+`" value="`+w.item+`">التسليح</option>`)
	has(t, w.browser.Send("POST", "/supplier", url.Values{"name": {" "}}).Body.String(), "أدخل اسماً من 1 إلى 100 حرف.")
	has(t, w.browser.Send("POST", "/supplier", url.Values{"name": {"مؤسسة أ"}, "vatNumber": {"30a"}}).Body.String(), "الرقم الضريبي رقم من 20 خانة على الأكثر.")
	added := w.browser.Send("POST", "/supplier", url.Values{"name": {"مؤسسة أ"}, "vatNumber": {"300222"}}).Body.String()
	has(t, added, `<h3 id="supplier.title">مؤسسة أ</h3>`)
	has(t, added, `<div id="web.content" hx-swap-oob="innerHTML">`)
	taken := w.browser.Send("POST", "/supplier", url.Values{"name": {"مؤسسة ب"}, "vatNumber": {"300222"}}).Body.String()
	has(t, taken, "هذا الرقم الضريبي لمورد آخر.")
	has(t, taken, `<small id="suppliers.holder" class="hint">المورد الموجود: مؤسسة أ</small>`)
	var first string
	if err := w.Database.Read.QueryRow("SELECT id FROM suppliers WHERE name = ?", "مؤسسة أ").Scan(&first); err != nil {
		t.Fatal(err)
	}
	has(t, w.browser.Send("GET", "/details/supplier/"+first+"?asking=edit", nil).Body.String(), `value="300222"`)
	edited := w.browser.Send("POST", "/supplier/"+first+"/edit", url.Values{"name": {"مؤسسة أ الجديدة"}, "vatNumber": {"300222"}, "defaultItem": {w.item}}).Body.String()
	has(t, edited, `<h3 id="supplier.title">مؤسسة أ الجديدة</h3>`)
	has(t, edited, `<dd id="supplier.defaultItem">التسليح</dd>`)
	has(t, w.browser.Send("POST", "/supplier/"+first+"/edit", url.Values{"name": {"مؤسسة أ"}, "mobile": {"x"}}).Body.String(), `<p id="supplier.refusal" class="refusal" role="alert">اكتب رقم الجوال`)
	disabled := w.browser.Send("POST", "/supplier/"+first+"/disable", nil).Body.String()
	has(t, disabled, `<p id="supplier.details" class="hint details-foot">معطّل · 300222 · التسليح</p>`)
	has(t, disabled, `id="suppliers.enable"`)
	has(t, w.browser.Send("POST", "/supplier/"+first+"/enable", nil).Body.String(), `id="suppliers.disable"`)
	var cash string
	if err := w.Database.Read.QueryRow("SELECT id FROM suppliers WHERE is_general_cash").Scan(&cash); err != nil {
		t.Fatal(err)
	}
	has(t, w.browser.Send("POST", "/supplier/"+cash+"/disable", nil).Body.String(), `<p id="supplier.refusal" class="refusal details-say" role="alert">يبقى المورد النقدي العام`)
	w.Ask("POST", w.path+"/items/"+w.item+"/disable", ``, w.token)
	kept := w.browser.Send("GET", "/details/supplier/"+first+"?asking=edit", nil).Body.String()
	has(t, kept, `<option id="suppliers.defaultItem.option.`+w.item+`" value="`+w.item+`" selected>التسليح</option>`)
	for _, path := range []string{"/supplier/" + first + "/archive", "/supplier/" + uuid.NewString() + "/edit"} {
		if code := w.browser.Send("POST", path, url.Values{"name": {"x"}}).Code; code != http.StatusNotFound {
			t.Errorf("%s: %d", path, code)
		}
	}
}

func TestWeb_R170_ASuppliersChangesAreGatedAsTheAppsRoutesAre(t *testing.T) {
	w := newWorking(t)
	supplier := w.supplier(`{"name":"مؤسسة أ"}`)
	member, _ := w.Person("User B")
	w.Ask("POST", w.path+"/users/existing", `{"username":"`+member.String()+`"}`, w.token)
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {member.String()}, "password": {password}})
	browser.Send("POST", "/project/"+w.id, nil)
	list := browser.Send("GET", "/tab/suppliers", nil).Body.String()
	has(t, list, `<button id="suppliers.add" class="primary small" type="button" disabled`)
	lacks(t, browser.Send("GET", "/details/supplier/"+supplier, nil).Body.String(), `id="suppliers.edit"`)
	if code := browser.Send("GET", "/details/supplier/new", nil).Code; code != http.StatusNotFound {
		t.Errorf("New supplier without ManageSettings: %d", code)
	}
	if code := browser.Send("POST", "/supplier", url.Values{"name": {"x"}}).Code; code != http.StatusForbidden {
		t.Errorf("adding without ManageSettings: %d", code)
	}
	w.home()
	for _, ask := range []struct{ method, path string }{{"POST", "/supplier"}, {"POST", "/supplier/" + supplier + "/edit"}, {"POST", "/supplier/" + supplier + "/disable"},
		{"GET", "/details/supplier/" + supplier}, {"GET", "/details/supplier/" + supplier + "?asking=edit"}, {"GET", "/details/supplier/new"}} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, ask.method, ask.path, url.Values{"name": {"مؤسسة ج"}})
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s %s with the database failing at its call %d answers %d", ask.method, ask.path, n, answered.Code)
			}
		}
	}
	if code, answer := w.Ask("POST", w.path+"/close", ``, w.token); code >= 300 {
		t.Fatalf("closing: %d %v", code, answer)
	}
	has(t, w.browser.Send("POST", "/supplier/"+supplier+"/edit", url.Values{"name": {"بعد الإغلاق"}}).Body.String(), "هذا المشروع منتهٍ، فلا يمكن تغييره.")
	lacks(t, w.browser.Send("GET", "/details/supplier/"+supplier, nil).Body.String(), `id="suppliers.edit"`)
}

// contract makes the working item take contractor payments and gives contractor a contract on it of terms; it is the contract's id.
func (w working) contract(contractor, terms string) string {
	w.t.Helper()
	w.Ask("PUT", w.path+"/items/"+w.item+"/type", `{"type":"both"}`, w.token)
	if code, answer := w.Ask("POST", w.path+"/contracts", `{"supplierId":"`+contractor+`","itemId":"`+w.item+`",`+terms+`}`, w.token); code >= 300 {
		w.t.Fatalf("the contract: %d %v", code, answer)
	}
	var id string
	if err := w.Database.Read.QueryRow("SELECT id FROM contracts WHERE supplier_id = ?", contractor).Scan(&id); err != nil {
		w.t.Fatal(err)
	}
	return id
}

// paid records a payment of amount to contractor inside contract, approved or only sent; it is the entry's id.
func (w working) paid(contract, contractor string, amount int, approve bool) string {
	w.t.Helper()
	id := uuid.Must(uuid.NewV7()).String()
	body := `{"id":"` + id + `","itemId":"` + w.item + `","tableId":"` + contract + `","date":"2026-11-10","amount":` + strconv.Itoa(amount) + `,"accountId":"` +
		w.account + `","paymentMethodId":"` + w.method + `","supplierId":"` + contractor + `","description":"","columnValues":{},"submit":true}`
	if code, answer := w.Ask("POST", w.path+"/entries", body, w.token); code != http.StatusCreated {
		w.t.Fatalf("the payment: %d %v", code, answer)
	}
	if approve {
		if code, answer := w.Ask("POST", w.path+"/entries/approve", `{"entryIds":["`+id+`"]}`, w.token); code != http.StatusOK {
			w.t.Fatalf("approving the payment: %d %v", code, answer)
		}
	}
	return id
}

func TestWeb_R170_UC020_ContractsListsEachContractsValuePaidAndRemaining(t *testing.T) {
	w := newWorking(t)
	w.funded(100000)
	builder, other := w.supplier(`{"name":"مقاول أ"}`), w.supplier(`{"name":"مقاول ب"}`)
	running := w.contract(builder, `"value":1000,"isOpenValue":false,"unitPrice":10,"quantity":100,"notes":"أعمال الأساسات"`)
	open := w.contract(other, `"value":0,"isOpenValue":true`)
	w.paid(running, builder, 1200, true)
	w.paid(running, builder, 300, false)
	w.home()
	has(t, w.browser.Send("GET", "/", nil).Body.String(), `hx-get="/tab/contracts"`)
	list := w.browser.Send("GET", "/tab/contracts", nil).Body.String()
	for _, part := range []string{
		`<td id="contracts.valueOf.` + running + `" class="amount">1,000.00</td>`,
		`<td id="contracts.paid.` + running + `" class="amount">1,200.00</td>`,
		`<td id="contracts.remaining.` + running + `" class="amount minus">(200.00)</td>`,
		`<span id="contracts.standingWord.` + running + `" class="chip warn">تجاوز العقد</span>`,
		`<td id="contracts.valueOf.` + open + `" class="amount">مفتوح</td>`,
		`<td id="contracts.remaining.` + open + `" class="amount">—</td>`,
		`<span id="contracts.standingWord.` + open + `" class="chip">عقد مفتوح القيمة</span>`,
		`الكل · 2`, `متجاوز · 1`, `مفتوح القيمة · 1`,
	} {
		has(t, list, part)
	}
	overrun := w.browser.Send("GET", "/tab/contracts?filter=overrun", nil).Body.String()
	has(t, overrun, `id="contracts.row.`+running+`"`)
	lacks(t, overrun, `id="contracts.row.`+open+`"`)
	found := w.browser.Send("GET", "/tab/contracts?text=الأساسات", nil).Body.String()
	lacks(t, found, `id="contracts.row.`+open+`"`)
	has(t, w.browser.Send("GET", "/tab/contracts?text=لا+شيء", nil).Body.String(), `<p id="contracts.empty" class="empty">لا توجد عقود.</p>`)
	panel := w.browser.Send("GET", "/details/contract/"+running, nil).Body.String()
	for _, part := range []string{`<h3 id="contract.title">مقاول أ</h3>`, `<dd id="contract.terms">10 × 100</dd>`, `<dd id="contract.notes">أعمال الأساسات</dd>`,
		`<p id="contract.pending" class="hint details-say">بانتظار الاعتماد 300.00 · (500.00) بعد الاعتماد</p>`,
		`hx-get="/tab/itemReport?contract=` + running + `"`} {
		has(t, panel, part)
	}
	has(t, w.browser.Send("GET", "/details/contract/"+open, nil).Body.String(), `<p id="contract.standing" class="hint details-foot">عقد مفتوح القيمة</p>`)
	third := w.supplier(`{"name":"مقاول ج"}`)
	steady := w.contract(third, `"value":5000,"isOpenValue":false,"unitPrice":25`)
	w.paid(steady, third, 1000, true)
	steadyPanel := w.browser.Send("GET", "/details/contract/"+steady, nil).Body.String()
	has(t, steadyPanel, `<p id="contract.standing" class="hint details-foot">جارٍ</p>`)
	has(t, steadyPanel, `<dd id="contract.terms">25</dd>`)
	if code := w.browser.Send("GET", "/details/contract/"+uuid.NewString(), nil).Code; code != http.StatusNotFound {
		t.Errorf("a contract not in the project: %d", code)
	}
}

func TestWeb_R170_UC020_NewContractEditDisableAndEnableAsTheAppsDialog(t *testing.T) {
	w := newWorking(t)
	builder := w.supplier(`{"name":"مقاول أ"}`)
	w.Ask("PUT", w.path+"/items/"+w.item+"/type", `{"type":"both"}`, w.token)
	w.home()
	form := w.browser.Send("GET", "/details/contract/new", nil).Body.String()
	has(t, form, `<option id="contracts.item.option.`+w.item+`" value="`+w.item+`">التسليح</option>`)
	has(t, form, `id="contracts.contractor.option.`+builder+`"`)
	lacks(t, w.browser.Send("GET", "/details/contract/new?contractorText=لا+أحد&value=", nil).Body.String(), `id="contracts.contractor.option.`+builder+`"`)
	for _, typed := range []string{"12a", "0"} {
		has(t, w.browser.Send("POST", "/contract", url.Values{"contractor": {builder}, "item": {w.item}, "value": {typed}}).Body.String(), "أدخل قيمة عقد أكبر من صفر")
	}
	has(t, w.browser.Send("POST", "/contract", url.Values{"contractor": {builder}, "item": {w.item}, "value": {"100"}, "quantity": {"x"}}).Body.String(),
		"أدخل الكمية بالأرقام")
	has(t, w.browser.Send("POST", "/contract", url.Values{"item": {w.item}, "value": {"100"}}).Body.String(), "اختر مقاولاً من الموردين.")
	added := w.browser.Send("POST", "/contract", url.Values{"contractor": {builder}, "item": {w.item}, "value": {"5,000"}, "unitPrice": {"50"}, "quantity": {"100"}}).Body.String()
	has(t, added, `<h3 id="contract.title">مقاول أ</h3>`)
	has(t, added, `<dd id="contract.value" class="money">5,000.00</dd>`)
	has(t, added, `<div id="web.content" hx-swap-oob="innerHTML">`)
	var contract string
	if err := w.Database.Read.QueryRow("SELECT id FROM contracts").Scan(&contract); err != nil {
		t.Fatal(err)
	}
	edit := w.browser.Send("GET", "/details/contract/"+contract+"?asking=edit", nil).Body.String()
	has(t, edit, `<input id="contracts.value" name="value" inputmode="decimal" value="5000">`)
	has(t, edit, `<option id="contracts.contractor.option.none" value="">مقاول أ</option>`)
	opened := w.browser.Send("POST", "/contract/"+contract+"/edit", url.Values{"openValue": {"true"}, "value": {""}, "notes": {"مفتوح الآن"}}).Body.String()
	has(t, opened, `<dd id="contract.value" class="money">مفتوح</dd>`)
	has(t, opened, `<dd id="contract.item">التسليح</dd>`)
	w.paid(contract, builder, 100, false)
	has(t, w.browser.Send("GET", "/details/contract/"+contract+"?asking=edit", nil).Body.String(), `<p id="contracts.parties" class="hint">مقاول أ · التسليح</p>`)
	other := w.supplier(`{"name":"مقاول ب"}`)
	has(t, w.browser.Send("POST", "/contract/"+contract+"/edit", url.Values{"contractor": {other}, "openValue": {"true"}, "value": {""}}).Body.String(),
		"لهذا العقد دفعات، فلا يتغير مقاوله ولا بنده.")
	disabled := w.browser.Send("POST", "/contract/"+contract+"/disable", nil).Body.String()
	has(t, disabled, `<p id="contract.standing" class="hint details-foot">معطّل</p>`)
	has(t, disabled, `id="contracts.enable"`)
	has(t, w.browser.Send("POST", "/contract/"+contract+"/enable", nil).Body.String(), `id="contracts.disable"`)
	for _, path := range []string{"/contract/" + contract + "/archive", "/contract/" + uuid.NewString() + "/edit"} {
		if code := w.browser.Send("POST", path, url.Values{"value": {"1"}}).Code; code != http.StatusNotFound {
			t.Errorf("%s: %d", path, code)
		}
	}
}

func TestWeb_R170_AContractsChangesAreGatedAsTheAppsRoutesAre(t *testing.T) {
	w := newWorking(t)
	builder := w.supplier(`{"name":"مقاول أ"}`)
	contract := w.contract(builder, `"value":1000,"isOpenValue":false`)
	member, _ := w.Person("User B")
	w.Ask("POST", w.path+"/users/existing", `{"username":"`+member.String()+`"}`, w.token)
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {member.String()}, "password": {password}})
	browser.Send("POST", "/project/"+w.id, nil)
	has(t, browser.Send("GET", "/tab/contracts", nil).Body.String(), `<button id="contracts.add" class="primary small" type="button" disabled`)
	lacks(t, browser.Send("GET", "/details/contract/"+contract, nil).Body.String(), `id="contracts.edit"`)
	if code := browser.Send("GET", "/details/contract/new", nil).Code; code != http.StatusNotFound {
		t.Errorf("New contract without ManageSettings: %d", code)
	}
	if code := browser.Send("POST", "/contract", url.Values{"value": {"1"}}).Code; code != http.StatusForbidden {
		t.Errorf("adding without ManageSettings: %d", code)
	}
	w.home()
	for _, ask := range []struct{ method, path string }{{"POST", "/contract"}, {"POST", "/contract/" + contract + "/edit"}, {"POST", "/contract/" + contract + "/disable"},
		{"GET", "/details/contract/" + contract}, {"GET", "/details/contract/" + contract + "?asking=edit"}, {"GET", "/details/contract/new"}} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, ask.method, ask.path, url.Values{"contractor": {builder}, "item": {w.item}, "value": {"200"}})
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s %s with the database failing at its call %d answers %d", ask.method, ask.path, n, answered.Code)
			}
		}
	}
	if code, answer := w.Ask("POST", w.path+"/close", ``, w.token); code >= 300 {
		t.Fatalf("closing: %d %v", code, answer)
	}
	has(t, w.browser.Send("POST", "/contract/"+contract+"/edit", url.Values{"value": {"2000"}}).Body.String(), "هذا المشروع منتهٍ، فلا يمكن تغييره.")
	lacks(t, w.browser.Send("GET", "/details/contract/"+contract, nil).Body.String(), `id="contracts.edit"`)
}

func TestWeb_R171_UC053_ReportsListsThePeriodicReportsAndIssuesTheNextAskedFirst(t *testing.T) {
	w := newWorking(t)
	w.funded(1000)
	w.approved(300)
	waiting := w.sent(200)
	w.home()
	has(t, w.browser.Send("GET", "/", nil).Body.String(), `hx-get="/tab/reports"`)
	list := w.browser.Send("GET", "/tab/reports", nil).Body.String()
	for _, part := range []string{`>إصدار رقم 1</button>`, `<p id="reports.noneIssued" class="empty">لم يُصدر تقرير بعد.</p>`, `hx-get="/tab/expenses"`,
		`hx-get="/tab/revenue"`, `<button id="reports.vat" class="quiet-button" type="button" disabled>`} {
		has(t, list, part)
	}
	asking := w.browser.Send("GET", "/tab/reports?asking=issue", nil).Body.String()
	has(t, asking, `<b id="reports.askQuestion">إصدار التقرير رقم 1؟</b>`)
	has(t, asking, `(1)`)
	issued := w.browser.Send("POST", "/reports/issue", nil).Body.String()
	has(t, issued, `<p id="reports.notice" class="notice" role="status">صدر التقرير.</p>`)
	has(t, issued, `<td id="reports.periodic.1.title">تقرير رقم 1</td>`)
	has(t, issued, `>إصدار رقم 2</button>`)
	for _, path := range []string{"/tab/reports", "/tab/reports?asking=issue"} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, "GET", path, nil)
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s with the database failing at its call %d answers %d", path, n, answered.Code)
			}
		}
	}
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "POST", "/reports/issue", nil)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("issuing with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
	w.Ask("POST", w.path+"/entries/approve", `{"entryIds":["`+waiting+`"]}`, w.token)
	if code, answer := w.Ask("POST", w.path+"/close", ``, w.token); code >= 300 {
		t.Fatalf("closing: %d %v", code, answer)
	}
	has(t, w.browser.Send("POST", "/reports/issue", nil).Body.String(), "هذا المشروع منتهٍ، فلا يمكن تغييره.")
}

func TestWeb_R171_UC053_ThePeriodicReportsAreForWhoeverSeesEveryTable(t *testing.T) {
	w := newWorking(t)
	member, _ := w.Person("User B")
	w.Ask("POST", w.path+"/users/existing", `{"username":"`+member.String()+`"}`, w.token)
	w.Ask("POST", w.path+"/users/"+member.String()+"/permissions/SeeReports", ``, w.token)
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {member.String()}, "password": {password}})
	browser.Send("POST", "/project/"+w.id, nil)
	seeing := browser.Send("GET", "/tab/reports", nil).Body.String()
	has(t, seeing, `<p id="reports.noneIssued" class="empty">`)
	has(t, seeing, `<button id="reports.issue" class="primary small" type="button" disabled`)
	if code := browser.Send("POST", "/reports/issue", nil).Code; code != http.StatusForbidden {
		t.Errorf("issuing without GenerateReports: %d", code)
	}
	if code, answer := w.Ask("POST", w.path+"/users/"+member.String()+"/tables", `{"seen":false,"tableIds":["`+w.table+`"]}`, w.token); code >= 300 {
		t.Fatalf("hiding the table: %d %v", code, answer)
	}
	has(t, browser.Send("GET", "/tab/reports", nil).Body.String(), `<p id="reports.periodicNotForYou" class="empty">التقرير الدوري لمن يرى كل الجداول</p>`)
}

func TestWeb_R171_UC050_EachSidesItemReportsAndAnItemsOrAContractsReport(t *testing.T) {
	w := newWorking(t)
	w.funded(5000)
	purchase := uuid.NewString()
	if code, answer := w.Ask("POST", w.path+"/entries", `{"id":"`+purchase+`","itemId":"`+w.item+`","tableId":"`+w.table+`","date":"2026-11-10","amount":300,"accountId":"`+
		w.account+`","paymentMethodId":"`+w.method+`","description":"حديد 16 مم","columnValues":{},"submit":true}`, w.token); code != http.StatusCreated {
		t.Fatalf("the purchase: %d %v", code, answer)
	}
	w.Ask("POST", w.path+"/entries/approve", `{"entryIds":["`+purchase+`"]}`, w.token)
	builder := w.supplier(`{"name":"مقاول أ"}`)
	contract := w.contract(builder, `"value":1000,"isOpenValue":false`)
	w.paid(contract, builder, 1200, true)
	w.home()
	expenses := w.browser.Send("GET", "/tab/expenses", nil).Body.String()
	has(t, expenses, `<b id="itemReports.total">1,500.00</b>`)
	has(t, expenses, `>الحديد</span>`)
	has(t, expenses, `1,500.00 · 100%`)
	has(t, expenses, `hx-get="/tab/itemReport?item=`+w.item+`"`)
	lacks(t, w.browser.Send("GET", "/tab/expenses?text=لا+شيء", nil).Body.String(), `>الحديد</span>`)
	has(t, w.browser.Send("GET", "/tab/expenses?text=التسليح", nil).Body.String(), `class="tile" open`)
	has(t, w.browser.Send("GET", "/tab/revenue", nil).Body.String(), `>البيع</td>`)
	report := w.browser.Send("GET", "/tab/itemReport?item="+w.item, nil).Body.String()
	for _, part := range []string{`<h2 id="itemReport.title">التسليح</h2>`, `<b id="itemReport.total">1,500.00</b>`, `>عام</span>`, `حديد 16 مم`, `>300.00</td>`,
		`<h3 id="itemReport.contractor.` + contract + `" class="section-title">مقاول أ</h3>`,
		`قيمة العقد: 1,000.00 · المدفوع: 1,200.00 · المتبقي: (200.00)`, `مدفوع أكثر من العقد`, `اجمالي الكل: 1,200.00`} {
		has(t, report, part)
	}
	found := w.browser.Send("GET", "/tab/itemReport?item="+w.item+"&text=16", nil).Body.String()
	has(t, found, `حديد 16 مم`)
	lacks(t, found, `itemReport.contractCard.`+contract)
	byContract := w.browser.Send("GET", "/tab/itemReport?contract="+contract, nil).Body.String()
	has(t, byContract, `itemReport.contractCard.`+contract)
	lacks(t, byContract, `حديد 16 مم`)
	has(t, w.browser.Send("GET", "/details/contract/"+contract, nil).Body.String(), `hx-get="/tab/itemReport?contract=`+contract+`"`)
	has(t, w.browser.Send("GET", "/tab/itemReport?item="+uuid.NewString(), nil).Body.String(), `<p id="itemReport.notFound" class="empty">تعذر تنفيذ ذلك.</p>`)
	for _, path := range []string{"/tab/expenses", "/tab/itemReport?item=" + w.item, "/tab/itemReport?contract=" + contract} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, "GET", path, nil)
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s with the database failing at its call %d answers %d", path, n, answered.Code)
			}
		}
	}
}

func TestWeb_R171_UC050_AReportRowSaysItsValuesByColumnAndANamedTableByName(t *testing.T) {
	w := newWorking(t)
	w.funded(5000)
	w.Ask("POST", w.path+"/items/"+w.item+"/columns", `{"name":"المقاس","dataType":"text","isRequired":false,"showInReports":true,"choices":[]}`, w.token)
	w.Ask("POST", w.path+"/items/"+w.item+"/columns", `{"name":"الخصم","dataType":"amount","isRequired":false,"showInReports":true,"choices":[]}`, w.token)
	w.Ask("POST", w.path+"/items/"+w.item+"/subgroups", `{"name":"الدور الأول"}`, w.token)
	var size, discount, floor string
	for query, into := range map[string]*string{
		"SELECT key FROM item_columns WHERE item_id = '" + w.item + "' AND name = 'المقاس'":  &size,
		"SELECT key FROM item_columns WHERE item_id = '" + w.item + "' AND name = 'الخصم'":   &discount,
		"SELECT id FROM subgroups WHERE item_id = '" + w.item + "' AND name = 'الدور الأول'": &floor,
	} {
		if err := w.Database.Read.QueryRow(query).Scan(into); err != nil {
			t.Fatal(query, err)
		}
	}
	purchase := uuid.NewString()
	if code, answer := w.Ask("POST", w.path+"/entries", `{"id":"`+purchase+`","itemId":"`+w.item+`","tableId":"`+floor+`","date":"2026-11-10","amount":400,"accountId":"`+
		w.account+`","paymentMethodId":"`+w.method+`","description":"","columnValues":{"`+size+`":"16 مم","`+discount+`":"1250"},"submit":true}`, w.token); code != http.StatusCreated {
		t.Fatalf("the purchase: %d %v", code, answer)
	}
	w.Ask("POST", w.path+"/entries/approve", `{"entryIds":["`+purchase+`"]}`, w.token)
	w.home()
	report := w.browser.Send("GET", "/tab/itemReport?item="+w.item, nil).Body.String()
	has(t, report, `>الدور الأول</span>`)
	has(t, report, `التاريخ: 2026-11-10 · الحساب: حساب المؤسسة · طريقة الدفع: شبكة · المقاس: 16 مم · الخصم: 1,250.00</small>`)
	stranger, _ := signedIn(t, w.API, "User C")
	if code := stranger.Send("POST", "/reports/issue", nil).Code; code != http.StatusNotFound {
		t.Errorf("issuing with no project chosen: %d", code)
	}
}

func TestWeb_R171_UC053_APeriodicReportShowsEachSectionAsIssued(t *testing.T) {
	w := newWorking(t)
	w.funded(1000)
	w.approved(300)
	w.sent(200)
	partner := w.partner("شريك أ", "")
	w.capitalIn(partner, 500, "")
	if code, answer := w.Ask("POST", w.path+"/periodic-reports", ``, w.token); code >= 300 {
		t.Fatalf("issuing: %d %v", code, answer)
	}
	w.approved(50)
	w.home()
	has(t, w.browser.Send("GET", "/tab/reports", nil).Body.String(), `hx-get="/tab/periodicReport?number=1"`)
	report := w.browser.Send("GET", "/tab/periodicReport?number=1", nil).Body.String()
	for _, part := range []string{
		`<h2 id="periodicReport.title">تقرير رقم 1 · `,
		`<dt id="periodicReport.line.accounts.0.label" class="line">حساب المؤسسة</dt>`,
		`<dd id="periodicReport.line.expenses.previous" class="money">0.00</dd>`,
		`<dd id="periodicReport.line.expenses.current" class="money">300.00</dd>`,
		`<dd id="periodicReport.difference" class="money strong">300.00</dd>`,
		`<dt id="periodicReport.line.surplus.pendingExpenses.label" class="line">مصروفات بانتظار الاعتماد (1)</dt>`,
		`<dt id="periodicReport.line.partners.0.label" class="line">شريك أ · 100.0000000000%</dt>`,
		`<dd id="periodicReport.line.partners.total" class="money strong">500.00</dd>`,
	} {
		has(t, report, part)
	}
	has(t, w.browser.Send("GET", "/tab/periodicReport?number=9", nil).Body.String(), `<p id="periodicReport.notFound" class="empty">`)
	for n := int64(1); ; n++ {
		answered, failed := w.browser.SendFailing(n, "GET", "/tab/periodicReport?number=1", nil)
		if !failed {
			break
		}
		if answered.Code != http.StatusInternalServerError {
			t.Errorf("the report with the database failing at its call %d answers %d", n, answered.Code)
		}
	}
	member, _ := w.Person("User B")
	w.Ask("POST", w.path+"/users/existing", `{"username":"`+member.String()+`"}`, w.token)
	w.Ask("POST", w.path+"/users/"+member.String()+"/permissions/SeeReports", ``, w.token)
	w.Ask("POST", w.path+"/users/"+member.String()+"/tables", `{"seen":false,"tableIds":["`+w.table+`"]}`, w.token)
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {member.String()}, "password": {password}})
	browser.Send("POST", "/project/"+w.id, nil)
	has(t, browser.Send("GET", "/tab/periodicReport?number=1", nil).Body.String(), `<p id="periodicReport.notForYou" class="empty">`)
}

func TestWeb_R171_UC054_EachReportsPdfAsTheAppsRouteHandsIt(t *testing.T) {
	w := newWorking(t)
	w.funded(5000)
	w.approved(300)
	builder := w.supplier(`{"name":"مقاول أ"}`)
	contract := w.contract(builder, `"value":1000,"isOpenValue":false`)
	if code, answer := w.Ask("POST", w.path+"/periodic-reports", ``, w.token); code >= 300 {
		t.Fatalf("issuing: %d %v", code, answer)
	}
	w.home()
	has(t, w.browser.Send("GET", "/tab/periodicReport?number=1", nil).Body.String(), `<a id="periodicReport.file.pdf" class="quiet-button" href="/report-file/periodic/1/pdf">`)
	has(t, w.browser.Send("GET", "/tab/itemReport?item="+w.item, nil).Body.String(), `href="/report-file/item/`+w.item+`/pdf"`)
	has(t, w.browser.Send("GET", "/tab/itemReport?contract="+contract, nil).Body.String(), `href="/report-file/contract/`+contract+`/pdf"`)
	has(t, w.browser.Send("GET", "/details/contract/"+contract, nil).Body.String(), `href="/report-file/contract/`+contract+`/pdf"`)
	for path, name := range map[string]string{"/report-file/periodic/1/pdf": "report-1.pdf", "/report-file/item/" + w.item + "/pdf": "item.pdf",
		"/report-file/contract/" + contract + "/pdf": "contract.pdf"} {
		answered := w.browser.Send("GET", path, nil)
		if answered.Code != http.StatusOK || answered.Header().Get("Content-Type") != "application/pdf" || !strings.HasPrefix(answered.Body.String(), "%PDF") {
			t.Errorf("%s: %d %s", path, answered.Code, answered.Header().Get("Content-Type"))
		}
		has(t, answered.Header().Get("Content-Disposition"), name)
	}
	for _, path := range []string{"/report-file/periodic/9/pdf", "/report-file/item/" + uuid.NewString() + "/pdf", "/report-file/contract/" + uuid.NewString() + "/pdf"} {
		if code := w.browser.Send("GET", path, nil).Code; code != http.StatusNotFound {
			t.Errorf("%s: %d", path, code)
		}
	}
	for _, path := range []string{"/report-file/periodic/1/pdf", "/report-file/item/" + w.item + "/pdf"} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, "GET", path, nil)
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s with the database failing at its call %d answers %d", path, n, answered.Code)
			}
		}
	}
	member, _ := w.Person("User B")
	w.Ask("POST", w.path+"/users/existing", `{"username":"`+member.String()+`"}`, w.token)
	w.Ask("POST", w.path+"/users/"+member.String()+"/permissions/SeeReports", ``, w.token)
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {member.String()}, "password": {password}})
	browser.Send("POST", "/project/"+w.id, nil)
	has(t, browser.Send("GET", "/tab/periodicReport?number=1", nil).Body.String(),
		`<button id="periodicReport.file.pdf" class="quiet-button" type="button" disabled title="يحتاج صلاحية «يُصدر التقارير».">`)
	if code := browser.Send("GET", "/report-file/item/"+w.item+"/pdf", nil).Code; code != http.StatusForbidden {
		t.Errorf("a PDF without GenerateReports: %d", code)
	}
	w.Ask("POST", w.path+"/users/"+member.String()+"/permissions/GenerateReports", ``, w.token)
	w.Ask("POST", w.path+"/users/"+member.String()+"/tables", `{"seen":false,"tableIds":["`+w.table+`"]}`, w.token)
	if code := browser.Send("GET", "/report-file/periodic/1/pdf", nil).Code; code != http.StatusForbidden {
		t.Errorf("the periodic PDF without every table: %d", code)
	}
	stranger, _ := signedIn(t, w.API, "User C")
	if code := stranger.Send("GET", "/report-file/periodic/1/pdf", nil).Code; code != http.StatusNotFound {
		t.Errorf("a PDF with no project chosen: %d", code)
	}
}

func TestWeb_R171_UC055_EachReportsWorkbookAndItsReceiptLinkOpenFromTheWeb(t *testing.T) {
	w := newWorking(t)
	w.funded(5000)
	code, uploaded := w.AskWithFile(w.path+"/attachments", "invoice-31.pdf", []byte("%PDF-1.4 invoice"), w.token, nil)
	if code >= 300 {
		t.Fatalf("uploading: %d %v", code, uploaded)
	}
	id := uuid.Must(uuid.NewV7()).String()
	body := `{"id":"` + id + `","itemId":"` + w.item + `","tableId":"` + w.table + `","date":"2026-11-10","amount":60,"accountId":"` + w.account +
		`","paymentMethodId":"` + w.method + `","description":"","columnValues":{},"attachmentId":"` + uploaded["id"].(string) + `","submit":true}`
	if code, answer := w.Ask("POST", w.path+"/entries", body, w.token); code != http.StatusCreated {
		t.Fatalf("recording: %d %v", code, answer)
	}
	w.Ask("POST", w.path+"/entries/approve", `{"entryIds":["`+id+`"]}`, w.token)
	builder := w.supplier(`{"name":"مقاول أ"}`)
	contract := w.contract(builder, `"value":1000,"isOpenValue":false`)
	if code, answer := w.Ask("POST", w.path+"/periodic-reports", ``, w.token); code >= 300 {
		t.Fatalf("issuing: %d %v", code, answer)
	}
	w.home()
	has(t, w.browser.Send("GET", "/tab/itemReport?item="+w.item, nil).Body.String(), `<a id="itemReport.file.excel" class="quiet-button" href="/report-file/item/`+w.item+`/xlsx">`)
	for path, name := range map[string]string{"/report-file/periodic/1/xlsx": "report-1.xlsx", "/report-file/item/" + w.item + "/xlsx": "item.xlsx",
		"/report-file/contract/" + contract + "/xlsx": "contract.xlsx"} {
		answered := w.browser.Send("GET", path, nil)
		if answered.Code != http.StatusOK || !strings.HasPrefix(answered.Body.String(), "PK") {
			t.Errorf("%s: %d %s", path, answered.Code, answered.Header().Get("Content-Type"))
		}
		has(t, answered.Header().Get("Content-Disposition"), name)
	}
	if code := w.browser.Send("GET", "/report-file/item/"+w.item+"/docx", nil).Code; code != http.StatusNotFound {
		t.Errorf("a format there is none of: %d", code)
	}
	workbook := w.browser.Send("GET", "/report-file/item/"+w.item+"/xlsx", nil).Body.Bytes()
	opened, err := zip.NewReader(bytes.NewReader(workbook), int64(len(workbook)))
	if err != nil {
		t.Fatal(err)
	}
	link := ""
	for _, file := range opened.File {
		if strings.HasPrefix(file.Name, "xl/worksheets/_rels/") {
			reader, _ := file.Open()
			content, _ := io.ReadAll(reader)
			_ = reader.Close()
			if at := strings.Index(string(content), "/files/"); at >= 0 {
				link = string(content[at:])
				link = link[:strings.IndexByte(link, '"')]
			}
		}
	}
	if link == "" {
		t.Fatal("the workbook links no receipt")
	}
	receipt := w.Browser().Send("GET", link, nil)
	if receipt.Code != http.StatusOK || receipt.Body.String() != "%PDF-1.4 invoice" {
		t.Errorf("the receipt's link opens %d %q", receipt.Code, receipt.Body.String())
	}
	if code := w.Browser().Send("GET", "/files/not-a-link", nil).Code; code != http.StatusNotFound {
		t.Errorf("a link never made: %d", code)
	}
}

func TestWeb_R172_UC069_TheProjectHistoryTellsEachLineAsTheAppsDoes(t *testing.T) {
	w := newWorking(t)
	w.Ask("PUT", w.path+"/items/"+w.item, `{"name":"حديد التسليح"}`, w.token)
	w.supplier(`{"name":"مؤسسة أ"}`)
	w.approved(300)
	w.home()
	has(t, w.browser.Send("GET", "/", nil).Body.String(), `hx-get="/tab/projectHistory"`)
	history := w.browser.Send("GET", "/tab/projectHistory", nil).Body.String()
	for _, part := range []string{`<h2 id="projectHistory.title">سجل المشروع</h2>`, `id="projectHistory.kindChoice"`, `id="projectHistory.personChoice"`,
		`الاسم كان التسليح`, `مؤسسة أ`, `300.00`} {
		has(t, history, part)
	}
	suppliers := w.browser.Send("GET", "/tab/projectHistory?area=suppliers", nil).Body.String()
	has(t, suppliers, `مؤسسة أ`)
	lacks(t, suppliers, `الاسم كان التسليح`)
	has(t, suppliers, `<option id="projectHistory.kind.suppliers" value="suppliers" selected>`)
	has(t, w.browser.Send("GET", "/tab/projectHistory?text=لا+شيء", nil).Body.String(), `<p id="projectHistory.empty" class="empty">لا شيء بعد.</p>`)
	for _, path := range []string{"/tab/projectHistory", "/project-history?before=999999"} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, "GET", path, nil)
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s with the database failing at its call %d answers %d", path, n, answered.Code)
			}
		}
	}
	member, _ := w.Person("User B")
	w.Ask("POST", w.path+"/users/existing", `{"username":"`+member.String()+`"}`, w.token)
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {member.String()}, "password": {password}})
	browser.Send("POST", "/project/"+w.id, nil)
	lacks(t, browser.Send("GET", "/", nil).Body.String(), `id="web.item.projectHistory"`)
	if code := browser.Send("GET", "/project-history", nil).Code; code != http.StatusNotFound {
		t.Errorf("the project history without SeeProjectHistory: %d", code)
	}
}

func TestWeb_R172_UC069_MyHistoryTellsEveryLineOfMyOwnLogin(t *testing.T) {
	w := newWorking(t)
	w.home()
	has(t, w.browser.Send("GET", "/", nil).Body.String(), `<button id="web.myHistory" class="quiet-button" type="button" hx-get="/tab/myHistory"`)
	mine := w.browser.Send("GET", "/tab/myHistory", nil).Body.String()
	has(t, mine, `<h2 id="myHistory.title">سجلي</h2>`)
	has(t, mine, `تسجيل الدخول`)
	has(t, mine, `id="myHistory.kindChoice"`)
	lacks(t, mine, `id="myHistory.personChoice"`)
	has(t, w.browser.Send("GET", "/tab/myHistory?area=password", nil).Body.String(), `<p id="myHistory.empty" class="empty">`)
	stranger, _ := signedIn(t, w.API, "User C")
	has(t, stranger.Send("GET", "/tab/myHistory", nil).Body.String(), `تسجيل الدخول`)
	if code := stranger.Send("GET", "/my-history?before=999999", nil).Code; code != http.StatusOK {
		t.Errorf("the next page of My history with no project chosen: %d", code)
	}
	for _, path := range []string{"/tab/myHistory", "/my-history?before=999999"} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, "GET", path, nil)
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s with the database failing at its call %d answers %d", path, n, answered.Code)
			}
		}
	}
}

// Findings row 293 (the owner, 23 Nov 2026): one details panel, following the tab in front - every selection is kept for its tab, and a tab coming
// from /tab/ selects and shows its own again; the browser's behaviour is proved by redix-web-automation's EntriesTest.
func TestWeb_D293_TheDetailsPanelFollowsTheTabInFront(t *testing.T) {
	browser, _ := signedIn(t, testapi.New(t), "User A")
	page := browser.Send("GET", "/", nil).Body.String()
	has(t, page, `hx-on::after-settle="`)
	has(t, page, `window.selected[front.id] = row.getAttribute(&#39;hx-get&#39;)`)
	has(t, page, `path.startsWith(&#39;/tab/&#39;)`)
	lacks(t, page, "openInTab")
}

// The side menu's line takes its drag for itself: pressing it starts none of the browser's own mouse actions, which dragged the side menu's link
// beside it and cancelled the line's drag after its first step (redix-web-automation WorkInTheShellTest, 23 Nov 2026).
func TestWeb_R10_TheSideLinesPressStartsNoDragOfItsOwn(t *testing.T) {
	browser, _ := signedIn(t, testapi.New(t), "User A")
	has(t, browser.Send("GET", "/", nil).Body.String(), `hx-on:pointerdown="event.preventDefault(); this.setPointerCapture(event.pointerId);`)
}

// member adds the person called name to the working project as the app's Add an existing user does, with roleName; it is their id.
func (w working) member(name, roleName string) uuid.UUID {
	w.t.Helper()
	person, _ := w.Person(name)
	if code, answer := w.Ask("POST", w.path+"/users/existing", `{"username":"`+person.String()+`"}`, w.token); code >= 300 {
		w.t.Fatalf("adding %s: %d %v", name, code, answer)
	}
	if roleName != "" {
		w.Ask("PUT", w.path+"/users/"+person.String()+"/role-name", `{"roleName":"`+roleName+`"}`, w.token)
	}
	return person
}

func TestWeb_R173_UC008_UsersListsEveryoneAsTheAppsScreen(t *testing.T) {
	w := newWorking(t)
	builder := w.member("User B", "مهندس الموقع")
	gone := w.member("User C", "")
	w.Ask("POST", w.path+"/users/"+gone.String()+"/disable", ``, w.token)
	w.home()
	has(t, w.browser.Send("GET", "/", nil).Body.String(), `hx-get="/tab/users"`)
	list := w.browser.Send("GET", "/tab/users", nil).Body.String()
	has(t, list, `<small id="users.details.`+builder.String()+`" class="hint">`+builder.String()+` · مهندس الموقع</small>`)
	has(t, list, `<small id="users.permissionsLine.`+builder.String()+`" class="hint">الصلاحيات: 1</small>`)
	has(t, list, `<span id="users.status.`+gone.String()+`" class="chip">معطّل</span>`)
	has(t, list, `<button id="users.new" class="primary small" type="button" hx-get="/tab/newUser"`)
	found := w.browser.Send("GET", "/tab/users?text=مهندس", nil).Body.String()
	has(t, found, `id="users.row.`+builder.String()+`"`)
	lacks(t, found, `id="users.row.`+gone.String()+`"`)
	has(t, w.browser.Send("GET", "/tab/users?text=لا+أحد", nil).Body.String(), `<p id="users.empty" class="empty">لا يوجد مستخدم مطابق.</p>`)
	panel := w.browser.Send("GET", "/details/user/"+builder.String(), nil).Body.String()
	has(t, panel, `<h3 id="user.title">User B</h3>`)
	has(t, panel, `<dd id="user.roleName">مهندس الموقع</dd>`)
	has(t, panel, `id="users.disable"`)
	has(t, w.browser.Send("GET", "/details/user/"+gone.String(), nil).Body.String(), `id="users.enable"`)
	if code := w.browser.Send("GET", "/details/user/"+uuid.NewString(), nil).Code; code != http.StatusNotFound {
		t.Errorf("someone not in the project: %d", code)
	}
}

func TestWeb_R173_UC007_EditResetDisableEnableAndAddExistingAsTheAppsDialogs(t *testing.T) {
	w := newWorking(t)
	builder := w.member("User B", "")
	newcomer, _ := w.Person("User D")
	w.home()
	has(t, w.browser.Send("GET", "/details/user/existing", nil).Body.String(), `id="users.existingUsername"`)
	has(t, w.browser.Send("POST", "/user", url.Values{"username": {"nobody-here"}}).Body.String(), "لا يوجد مستخدم بهذا الاسم.")
	added := w.browser.Send("POST", "/user", url.Values{"username": {newcomer.String()}}).Body.String()
	has(t, added, `<h3 id="user.title">User D</h3>`)
	has(t, added, `<div id="web.content" hx-swap-oob="innerHTML">`)
	has(t, w.browser.Send("GET", "/details/user/"+builder.String()+"?asking=edit", nil).Body.String(), `<input id="users.editName" name="name" required value="User B" autofocus>`)
	has(t, w.browser.Send("POST", "/user/"+builder.String()+"/edit", url.Values{"name": {"User B"}, "mobile": {"abc"}}).Body.String(),
		`<p id="user.refusal" class="refusal" role="alert">اكتب رقم الجوال`)
	edited := w.browser.Send("POST", "/user/"+builder.String()+"/edit", url.Values{"name": {"المهندس ب"}, "mobile": {""}, "email": {"b@example.com"}, "roleName": {"مشرف"}}).Body.String()
	has(t, edited, `<h3 id="user.title">المهندس ب</h3>`)
	has(t, edited, `<dd id="user.roleName">مشرف</dd>`)
	has(t, w.browser.Send("POST", "/user/"+builder.String()+"/edit", url.Values{"name": {"المهندس ب"}, "email": {"b@example.com"}, "roleName": {strings.Repeat("م", 51)}}).Body.String(),
		"المسمى الوظيفي 50 حرفاً على الأكثر.")
	has(t, w.browser.Send("GET", "/details/user/"+builder.String()+"?asking=reset", nil).Body.String(), `id="users.resetPasswordField"`)
	has(t, w.browser.Send("POST", "/user/"+builder.String()+"/reset", url.Values{"password": {"short"}}).Body.String(), "استخدم 8 أحرف على الأقل")
	has(t, w.browser.Send("POST", "/user/"+builder.String()+"/reset", url.Values{"password": {"Temporary-77"}}).Body.String(), `<h3 id="user.title">المهندس ب</h3>`)
	has(t, w.browser.Send("GET", "/details/user/"+builder.String()+"?asking=disable", nil).Body.String(), "تعطيل هذا المستخدم؟")
	disabled := w.browser.Send("POST", "/user/"+builder.String()+"/disable", nil).Body.String()
	has(t, disabled, `id="users.enable"`)
	has(t, disabled, `<span id="users.status.`+builder.String()+`" class="chip">معطّل</span>`)
	has(t, w.browser.Send("POST", "/user/"+builder.String()+"/enable", nil).Body.String(), `id="users.disable"`)
	has(t, w.browser.Send("POST", "/user/"+w.manager.String()+"/disable", nil).Body.String(), "يحتاج المشروع إلى شخص واحد على الأقل يدير مستخدميه.")
	for _, path := range []string{"/user/" + builder.String() + "/archive", "/user/" + uuid.NewString() + "/edit"} {
		if code := w.browser.Send("POST", path, url.Values{"name": {"x"}}).Code; code != http.StatusNotFound {
			t.Errorf("%s: %d", path, code)
		}
	}
}

func TestWeb_R173_UsersAreForWhoeverManagesThem(t *testing.T) {
	w := newWorking(t)
	builder := w.member("User B", "")
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {builder.String()}, "password": {password}})
	browser.Send("POST", "/project/"+w.id, nil)
	lacks(t, browser.Send("GET", "/", nil).Body.String(), `id="web.item.users"`)
	if code := browser.Send("GET", "/details/user/"+builder.String(), nil).Code; code != http.StatusNotFound {
		t.Errorf("a person's panel without ManageUsers: %d", code)
	}
	if code := browser.Send("POST", "/user", url.Values{"username": {"x"}}).Code; code != http.StatusForbidden {
		t.Errorf("adding without ManageUsers: %d", code)
	}
	w.home()
	for _, ask := range []struct{ method, path string }{{"GET", "/tab/users"}, {"GET", "/details/user/" + builder.String()}, {"POST", "/user/" + builder.String() + "/edit"},
		{"POST", "/user/" + builder.String() + "/disable"}, {"POST", "/user"}} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, ask.method, ask.path, url.Values{"name": {"User B2"}, "username": {builder.String()}})
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s %s with the database failing at its call %d answers %d", ask.method, ask.path, n, answered.Code)
			}
		}
	}
}

func TestWeb_R173_UC008_NewUserAsksWhatTheAppsScreenAsks(t *testing.T) {
	w := newWorking(t)
	partner := w.partner("الشريك A", "")
	w.home()
	has(t, w.browser.Send("GET", "/tab/users", nil).Body.String(), `hx-get="/tab/newUser"`)
	form := w.browser.Send("GET", "/tab/newUser", nil).Body.String()
	has(t, form, `<h2 id="newUser.title">مستخدم جديد</h2>`)
	has(t, form, `id="newUser.name.hint"`)
	has(t, form, `id="newUser.password.hint"`)
	has(t, form, `<input id="newUser.readProject.input" type="checkbox" checked disabled name="permission" value="ReadProject">`)
	has(t, form, `data-excludes="newUser.seeChosenCapital.input"`)
	has(t, form, `<p id="newUser.permissions.capitalGroup" class="section-title">`)
	has(t, form, `<p id="newUser.permissions.reportsGroup" class="section-title">`)
	has(t, form, `<input id="permissions.partner.`+partner+`.input" type="checkbox" name="partner" value="`+partner+`">`)
	has(t, form, `<input id="tables.table.`+w.table+`.input" type="checkbox" checked data-table name="seen" value="`+w.table+`">`)
	has(t, form, `<input id="tables.item.`+w.item+`.input" type="checkbox" checked data-item>`)
	lacks(t, form, `value="ApproveProjects"`)
}

func TestWeb_R173_UC008_NewUserAddsThePersonWithWhatIsTicked(t *testing.T) {
	w := newWorking(t)
	partner := w.partner("الشريك A", "")
	w.home()
	typed := func(username string, permissions ...string) url.Values {
		return url.Values{"name": {"User E"}, "username": {username}, "mobile": {""}, "email": {username + "@example.com"}, "password": {"Temporary-77"},
			"roleName": {"محاسب"}, "permission": permissions, "partner": {partner}}
	}
	refused := w.browser.Send("POST", "/users/new", typed("user.e", "SeeAllCapital", "SeeChosenCapital")).Body.String()
	has(t, refused, `<p id="newUser.message" class="refusal" role="alert">لا تُمنح هذه الصلاحية داخل مشروع.</p>`)
	has(t, refused, `<input id="newUser.username" name="username" required autocomplete="off" value="user.e">`)
	has(t, refused, `<input id="newUser.password" name="password" type="password" autocomplete="new-password" required>`)
	has(t, refused, `<input id="newUser.seeAllCapital.input" type="checkbox" checked data-excludes="newUser.seeChosenCapital.input" name="permission" value="SeeAllCapital">`)
	added := w.browser.Send("POST", "/users/new", typed("user.e", "RecordEntries", "SeeChosenCapital")).Body.String()
	has(t, added, `<h3 id="user.title">User E</h3>`)
	has(t, added, `<dd id="user.roleName">محاسب</dd>`)
	has(t, added, `<input id="newUser.name" name="name" required value="" autofocus>`)
	read := w.Database.Read
	var person string
	if err := read.QueryRow("SELECT id FROM users WHERE username = 'user.e'").Scan(&person); err != nil {
		t.Fatal(err)
	}
	var permissions, chosen, unseen int
	for query, into := range map[string]*int{
		"SELECT count(*) FROM project_permissions WHERE user_id = ? AND permission IN ('ReadProject', 'RecordEntries', 'SeeChosenCapital')": &permissions,
		"SELECT count(*) FROM chosen_partners WHERE user_id = ? AND partner_id = '" + partner + "'":                                         &chosen,
		"SELECT count(*) FROM unseen_groups WHERE user_id = ? AND subgroup_id = '" + w.table + "'":                                          &unseen,
	} {
		if err := read.QueryRow(query, person).Scan(into); err != nil {
			t.Fatal(query, err)
		}
	}
	if permissions != 3 || chosen != 1 || unseen != 1 {
		t.Errorf("the new person holds %d of the three permissions, %d chosen partner, %d unseen table", permissions, chosen, unseen)
	}
	seen := typed("user.f", "RecordEntries")
	seen.Set("seen", w.table)
	w.browser.Send("POST", "/users/new", seen)
	if err := read.QueryRow("SELECT count(*) FROM unseen_groups JOIN users ON users.id = unseen_groups.user_id WHERE users.username = 'user.f' AND subgroup_id = ?", w.table).Scan(&unseen); err != nil || unseen != 0 {
		t.Errorf("a table left ticked is seen: %d unseen, %v", unseen, err)
	}
	has(t, w.browser.Send("POST", "/users/new", typed("user.e", "RecordEntries")).Body.String(), "اسم المستخدم هذا مستخدم. اختر غيره.")
}

func TestWeb_R173_NewUserIsForWhoeverManagesTheUsers(t *testing.T) {
	w := newWorking(t)
	builder := w.member("User B", "")
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {builder.String()}, "password": {password}})
	browser.Send("POST", "/project/"+w.id, nil)
	if code := browser.Send("POST", "/users/new", url.Values{"username": {"x"}}).Code; code != http.StatusForbidden {
		t.Errorf("New user without ManageUsers: %d", code)
	}
	w.home()
	for _, ask := range []struct{ method, path string }{{"GET", "/tab/newUser"}, {"POST", "/users/new"}} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, ask.method, ask.path, url.Values{"name": {"User G"}, "username": {"user.g"}, "email": {"g@example.com"},
				"password": {"Temporary-77"}})
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s %s with the database failing at its call %d answers %d", ask.method, ask.path, n, answered.Code)
			}
		}
	}
}

// tick is how a person's Permissions page draws the tick whose id is id, ticked or not, the rest of its tag cut.
func tick(id string, ticked bool) string {
	if ticked {
		return `<input id="` + id + `.input" type="checkbox" checked `
	}
	return `<input id="` + id + `.input" type="checkbox" hx-post=`
}

func TestWeb_R173_UC008_PermissionsShowWhatThePersonHoldsAsTheAppsScreen(t *testing.T) {
	w := newWorking(t)
	builder := w.member("User B", "")
	partner := w.partner("الشريك A", "")
	w.Ask("POST", w.path+"/users/"+builder.String()+"/permissions/RecordEntries", ``, w.token)
	w.home()
	has(t, w.browser.Send("GET", "/details/user/"+builder.String(), nil).Body.String(), `hx-get="/tab/userPermissions?user=`+builder.String()+`"`)
	page := w.browser.Send("GET", "/tab/userPermissions?user="+builder.String(), nil).Body.String()
	has(t, page, `<h3 id="userPermissions.name" class="section-title">User B</h3>`)
	has(t, page, `id="userPermissions.hint"`)
	has(t, page, tick("userPermissions.readProject", true))
	has(t, page, tick("userPermissions.recordEntries", true))
	has(t, page, tick("userPermissions.manageUsers", false))
	has(t, page, `hx-post="/user/`+builder.String()+`/permission/RecordEntries"`)
	has(t, page, `hx-post="/user/`+builder.String()+`/partner/`+partner+`"`)
	has(t, page, `hx-post="/user/`+builder.String()+`/tables?table=`+w.table+`"`)
	has(t, page, tick("tables.table."+w.table, true))
	lacks(t, page, `value="ApproveProjects"`)
	lacks(t, w.browser.Send("GET", "/tab/userPermissions?user="+uuid.NewString(), nil).Body.String(), `id="userPermissions.name"`)
}

func TestWeb_R173_UC008_EachTickIsKeptAtOnceAsTheAppsScreenKeepsIt(t *testing.T) {
	w := newWorking(t)
	builder := w.member("User B", "")
	partner := w.partner("الشريك A", "")
	w.Ask("POST", w.path+"/items/"+w.item+"/subgroups", `{"name":"الدور الأول"}`, w.token)
	var floor string
	if err := w.Database.Read.QueryRow("SELECT id FROM subgroups WHERE item_id = ? AND name = 'الدور الأول'", w.item).Scan(&floor); err != nil {
		t.Fatal(err)
	}
	w.home()
	person := "/user/" + builder.String()
	held := url.Values{"held": {"on"}}
	given := w.browser.Send("POST", person+"/permission/RecordEntries", held).Body.String()
	has(t, given, tick("userPermissions.recordEntries", true))
	has(t, given, `<p id="user.permissionsLine" class="hint details-foot">الصلاحيات: 2</p>`)
	w.browser.Send("POST", person+"/permission/SeeAllCapital", held)
	chosen := w.browser.Send("POST", person+"/permission/SeeChosenCapital", held).Body.String()
	has(t, chosen, tick("userPermissions.seeChosenCapital", true))
	has(t, chosen, tick("userPermissions.seeAllCapital", false))
	has(t, w.browser.Send("POST", person+"/partner/"+partner, held).Body.String(), `<input id="permissions.partner.`+partner+`.input" type="checkbox" checked `)
	has(t, w.browser.Send("POST", person+"/partner/"+partner, nil).Body.String(), `<input id="permissions.partner.`+partner+`.input" type="checkbox" hx-post=`)
	between := w.browser.Send("POST", person+"/tables?table="+w.table, nil).Body.String()
	has(t, between, tick("tables.table."+w.table, false))
	has(t, between, tick("tables.table."+floor, true))
	has(t, between, `<input id="tables.item.`+w.item+`.input" type="checkbox" data-partial hx-post=`)
	both := w.browser.Send("POST", person+"/tables?table="+w.table+"&table="+floor, held).Body.String()
	has(t, both, `<input id="tables.item.`+w.item+`.input" type="checkbox" checked hx-post=`)
	taken := w.browser.Send("POST", person+"/permission/RecordEntries", nil).Body.String()
	has(t, taken, tick("userPermissions.recordEntries", false))
	has(t, w.browser.Send("POST", "/user/"+w.manager.String()+"/permission/ManageUsers", nil).Body.String(),
		`<p id="userPermissions.message" class="refusal" role="alert">يحتاج المشروع إلى شخص واحد على الأقل يدير مستخدميه.</p>`)
	has(t, w.browser.Send("POST", person+"/permission/ApproveProjects", held).Body.String(), "لا تُمنح هذه الصلاحية داخل مشروع.")
	out := w.browser.Send("POST", person+"/permission/ReadProject", nil).Body.String()
	lacks(t, out, `id="userPermissions.name"`)
	has(t, out, `<aside id="web.details" class="details" aria-live="polite" hx-swap-oob="true"></aside>`)
	for _, path := range []string{"/user/" + uuid.NewString() + "/permission/RecordEntries", "/user/x/permission/RecordEntries", person + "/partner/x",
		"/user/" + w.manager.String() + "/tables?table=x"} {
		if code := w.browser.Send("POST", path, held).Code; code != http.StatusNotFound {
			t.Errorf("%s: %d", path, code)
		}
	}
}

func TestWeb_R173_PermissionsAreForWhoeverManagesTheUsers(t *testing.T) {
	w := newWorking(t)
	builder := w.member("User B", "")
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {builder.String()}, "password": {password}})
	browser.Send("POST", "/project/"+w.id, nil)
	if code := browser.Send("POST", "/user/"+builder.String()+"/permission/ManageUsers", url.Values{"held": {"on"}}).Code; code != http.StatusForbidden {
		t.Errorf("a tick without ManageUsers: %d", code)
	}
	w.home()
	for _, ask := range []struct{ method, path string }{{"GET", "/tab/userPermissions?user=" + builder.String()},
		{"POST", "/user/" + builder.String() + "/permission/RecordEntries"}} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, ask.method, ask.path, url.Values{"held": {"on"}})
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s %s with the database failing at its call %d answers %d", ask.method, ask.path, n, answered.Code)
			}
		}
	}
}

// memberBrowser is a browser signed in as person, in w's project.
func (w working) memberBrowser(person uuid.UUID) *testapi.Browser {
	w.t.Helper()
	browser := w.Browser()
	browser.Send("POST", "/sign-in", url.Values{"username": {person.String()}, "password": {password}})
	browser.Send("POST", "/project/"+w.id, nil)
	return browser
}

func TestWeb_R173_R112_MyPermissionsShowsWhatTheCallerHoldsAsTheAppsScreen(t *testing.T) {
	w := newWorking(t)
	builder := w.member("User B", "")
	w.Ask("POST", w.path+"/users/"+builder.String()+"/permissions/RecordEntries", ``, w.token)
	browser := w.memberBrowser(builder)
	has(t, browser.Send("GET", "/", nil).Body.String(), `hx-get="/tab/myPermissions"`)
	mine := browser.Send("GET", "/tab/myPermissions", nil).Body.String()
	has(t, mine, `<h2 id="myPermissions.title">صلاحياتي</h2>`)
	has(t, mine, `id="myPermissions.hint"`)
	has(t, mine, `<input id="myPermissions.readProject.input" type="checkbox" checked disabled>`)
	has(t, mine, `<input id="myPermissions.recordEntries.input" type="checkbox" checked disabled>`)
	has(t, mine, `<input id="myPermissions.manageUsers.input" type="checkbox" disabled>`)
	has(t, mine, `<input id="tables.table.`+w.table+`.input" type="checkbox" checked disabled>`)
	has(t, mine, `<button id="myPermissions.ask" class="primary small" type="button" hx-get="/tab/permissionRequest" hx-target="[id='web.content']">`)
	lacks(t, mine, `id="myPermissions.request"`)
	lacks(t, mine, `id="tables.heading.hint"`)
	found := browser.Send("GET", "/tab/myPermissions?text=تسجيل", nil).Body.String()
	has(t, found, `id="myPermissions.recordEntries.input"`)
	lacks(t, found, `id="myPermissions.manageUsers.input"`)
	w.home()
	partner := w.partner("الشريك A", "")
	w.browser.Send("POST", "/user/"+builder.String()+"/partner/"+partner, url.Values{"held": {"on"}})
	w.browser.Send("POST", "/user/"+builder.String()+"/tables?table="+w.table, nil)
	seeing := w.memberBrowser(builder).Send("GET", "/tab/myPermissions", nil).Body.String()
	has(t, seeing, `<p id="tables.heading.hint" class="hint">التقرير الدوري لمن يرى كل الجداول</p>`)
	has(t, seeing, `<input id="myPermissions.seeChosenCapital.input" type="checkbox" checked disabled>`)
	has(t, seeing, `<input id="permissions.partner.`+partner+`.input" type="checkbox" checked disabled>`)
}

func TestWeb_R173_R112_AskingNeedsAProjectAndAnswersAFailingDatabase(t *testing.T) {
	w := newWorking(t)
	builder := w.member("User B", "")
	outside := w.Browser()
	outside.Send("POST", "/sign-in", url.Values{"username": {builder.String()}, "password": {password}})
	for _, ask := range []struct{ method, path string }{{"GET", "/details/permission-request"}, {"POST", "/permission-request"}, {"POST", "/permission-request/withdraw"}} {
		if code := outside.Send(ask.method, ask.path, nil).Code; code != http.StatusNotFound {
			t.Errorf("%s %s with no project: %d", ask.method, ask.path, code)
		}
	}
	browser := w.memberBrowser(builder)
	for _, ask := range []struct{ method, path string }{{"GET", "/details/permission-request?permission=ManageUsers"}, {"POST", "/permission-request"},
		{"POST", "/permission-request/withdraw"}, {"GET", "/tab/permissionRequest"}} {
		for n := int64(1); ; n++ {
			answered, failed := browser.SendFailing(n, ask.method, ask.path, url.Values{"permission": {"ManageUsers"}, "reason": {"أتابع المستخدمين"}})
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s %s with the database failing at its call %d answers %d", ask.method, ask.path, n, answered.Code)
			}
		}
	}
}

func TestWeb_R173_R112_AskForPermissionsAndWithdrawAsTheAppsScreens(t *testing.T) {
	w := newWorking(t)
	builder := w.member("User B", "")
	w.Ask("POST", w.path+"/users/"+builder.String()+"/permissions/RecordEntries", ``, w.token)
	browser := w.memberBrowser(builder)
	asking := browser.Send("GET", "/tab/permissionRequest", nil).Body.String()
	has(t, asking, `id="permissionRequest.send" class="primary small" type="button" disabled`)
	has(t, asking, `<input id="permissionRequest.readProject.input" type="checkbox" checked disabled name="permission" value="ReadProject">`)
	has(t, asking, `<input id="permissionRequest.recordEntries.input" type="checkbox" checked name="permission" value="RecordEntries">`)
	has(t, asking, `data-excludes="permissionRequest.seeChosenCapital.input"`)
	wanted := url.Values{"permission": {"ManageUsers", "SeeReports"}}
	dialog := browser.Send("GET", "/details/permission-request?"+wanted.Encode(), nil).Body.String()
	has(t, dialog, `id="permissionRequest.reason"`)
	has(t, dialog, "يُضاف: إدارة المستخدمين، ")
	has(t, dialog, "يُزال: تسجيل المصروفات")
	has(t, browser.Send("POST", "/permission-request", wanted).Body.String(), `<p id="permissionRequest.refusal" class="refusal" role="alert">اذكر السبب</p>`)
	wanted.Set("reason", "أتابع المستخدمين")
	sent := browser.Send("POST", "/permission-request", wanted)
	if sent.Header().Get("HX-Retarget") != "[id='web.content']" {
		t.Errorf("Send brings My permissions to the content area: %q", sent.Header().Get("HX-Retarget"))
	}
	has(t, sent.Body.String(), `<p id="myPermissions.notice" class="notice" role="status">أُرسل طلبك إلى مدير المستخدمين</p>`)
	has(t, sent.Body.String(), "طلبك بانتظار الموافقة\nيُضاف: إدارة المستخدمين")
	has(t, sent.Body.String(), `id="myPermissions.withdraw"`)
	lacks(t, sent.Body.String(), `id="web.tab.permissionRequest"`)
	has(t, sent.Body.String(), `<div id="web.tab.myPermissions" class="tab" role="tab" aria-selected="true">`)
	if waiting := w.Count(`SELECT count(*) FROM permission_requests WHERE user_id = '` + builder.String() + `' AND status = 0`); waiting != 1 {
		t.Errorf("the request waits: %d", waiting)
	}
	withdrawn := browser.Send("POST", "/permission-request/withdraw", nil).Body.String()
	has(t, withdrawn, "سُحب طلبك")
	lacks(t, withdrawn, `id="myPermissions.request"`)
	has(t, browser.Send("POST", "/permission-request/withdraw", nil).Body.String(), `<p id="myPermissions.refusal" class="refusal" role="alert">رُفض الطلب</p>`)
	if code, answer := w.Ask("POST", w.path+"/close", ``, w.token); code >= 300 {
		t.Fatalf("closing: %d %v", code, answer)
	}
	closed := w.memberBrowser(builder)
	has(t, closed.Send("GET", "/tab/myPermissions", nil).Body.String(), `hx-target="[id='web.content']" disabled>`)
	has(t, closed.Send("POST", "/permission-request", wanted).Body.String(), "المشروع منتهٍ فلا يتغير")
}

func TestWeb_R173_R112_PermissionRequestsApproveAndRejectAsTheAppsScreen(t *testing.T) {
	w := newWorking(t)
	builder := w.member("User B", "")
	ask := func() string {
		w.t.Helper()
		if code, answer := w.Ask("POST", w.path+"/permission-requests", `{"gives":["ManageUsers"],"reason":"أتابع المستخدمين"}`, w.Token(builder, false)); code != http.StatusCreated {
			t.Fatalf("asking: %d %v", code, answer)
		}
		var id string
		if err := w.Database.Read.QueryRow("SELECT id FROM permission_requests WHERE user_id = ? AND status = 0", builder).Scan(&id); err != nil {
			t.Fatal(err)
		}
		return id
	}
	asked := ask()
	w.home()
	list := w.browser.Send("GET", "/tab/requests", nil).Body.String()
	has(t, list, `<strong id="permissionRequests.row.`+asked+`.title">User B</strong>`)
	has(t, list, "يُضاف: إدارة المستخدمين\nأتابع المستخدمين\n")
	has(t, w.browser.Send("GET", "/details/permission-request/"+asked+"/reject", nil).Body.String(), `<h3 id="permissionRequests.rejectTitle">رفض طلب User B</h3>`)
	has(t, w.browser.Send("POST", "/permission-request/"+asked+"/reject", nil).Body.String(), `<p id="permissionRequests.rejectRefusal" class="refusal" role="alert">اذكر السبب</p>`)
	rejected := w.browser.Send("POST", "/permission-request/"+asked+"/reject", url.Values{"reason": {"ليس الآن"}}).Body.String()
	has(t, rejected, `<div id="web.content" hx-swap-oob="innerHTML">`)
	has(t, rejected, "رُفض طلب User B")
	has(t, rejected, `<p id="permissionRequests.empty" class="empty">لا طلب بانتظار الموافقة</p>`)
	has(t, w.memberBrowser(builder).Send("GET", "/tab/myPermissions", nil).Body.String(), "رُفض طلبك: ليس الآن")
	asked = ask()
	approved := w.browser.Send("POST", "/permission-request/"+asked+"/approve", nil).Body.String()
	has(t, approved, "اعتُمد طلب User B")
	if held := w.Count(`SELECT count(*) FROM project_permissions WHERE user_id = '` + builder.String() + `' AND permission = 'ManageUsers'`); held != 1 {
		t.Errorf("approving gives what was asked: %d", held)
	}
	for _, path := range []string{"/permission-request/" + asked + "/approve", "/permission-request/" + uuid.NewString() + "/approve", "/permission-request/" + asked + "/archive"} {
		if code := w.browser.Send("POST", path, nil).Code; code != http.StatusNotFound {
			t.Errorf("%s: %d", path, code)
		}
	}
	if code := w.browser.Send("GET", "/details/permission-request/"+uuid.NewString()+"/reject", nil).Code; code != http.StatusNotFound {
		t.Errorf("rejecting no request: %d", code)
	}
}

func TestWeb_R173_R112_RequestsAreForWhoeverManagesTheUsers(t *testing.T) {
	w := newWorking(t)
	builder := w.member("User B", "")
	w.Ask("POST", w.path+"/permission-requests", `{"gives":["ManageUsers"],"reason":"أتابع المستخدمين"}`, w.Token(builder, false))
	var asked string
	if err := w.Database.Read.QueryRow("SELECT id FROM permission_requests WHERE user_id = ?", builder).Scan(&asked); err != nil {
		t.Fatal(err)
	}
	browser := w.memberBrowser(builder)
	lacks(t, browser.Send("GET", "/", nil).Body.String(), `hx-get="/tab/requests"`)
	if code := browser.Send("POST", "/permission-request/"+asked+"/approve", nil).Code; code != http.StatusForbidden {
		t.Errorf("approving without ManageUsers: %d", code)
	}
	if code := browser.Send("GET", "/details/permission-request/"+asked+"/reject", nil).Code; code != http.StatusNotFound {
		t.Errorf("Reject's sheet without ManageUsers: %d", code)
	}
	w.home()
	if code, answer := w.Ask("POST", w.path+"/close", ``, w.token); code >= 300 {
		t.Fatalf("closing: %d %v", code, answer)
	}
	has(t, w.browser.Send("POST", "/permission-request/"+asked+"/approve", nil).Body.String(), `<p id="permissionRequests.refusal" class="refusal wide" role="alert">المشروع منتهٍ فلا يتغير</p>`)
	for _, ask := range []struct{ method, path string }{{"GET", "/tab/requests"}, {"GET", "/tab/myPermissions"}, {"GET", "/details/permission-request/" + asked + "/reject"},
		{"POST", "/permission-request/" + asked + "/reject"}, {"POST", "/permission-request/withdraw"}} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, ask.method, ask.path, url.Values{"reason": {"ليس الآن"}})
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s %s with the database failing at its call %d answers %d", ask.method, ask.path, n, answered.Code)
			}
		}
	}
}

// idsIn is the ids query answers, in its order, joined by commas as a reorder sends them.
func (w working) idsIn(query string, args ...any) []string {
	w.t.Helper()
	rows, err := w.Database.Read.Query(query, args...)
	if err != nil {
		w.t.Fatal(err)
	}
	defer rows.Close()
	var ids []string
	for rows.Next() {
		var id string
		if err := rows.Scan(&id); err != nil {
			w.t.Fatal(err)
		}
		ids = append(ids, id)
	}
	return ids
}

func TestWeb_R174_UC012_CategoriesAndItemsListAsTheAppsScreen(t *testing.T) {
	w := newWorking(t)
	var category string
	if err := w.Database.Read.QueryRow("SELECT category_id FROM items WHERE id = ?", w.item).Scan(&category); err != nil {
		t.Fatal(err)
	}
	w.home()
	list := w.browser.Send("GET", "/tab/categories", nil).Body.String()
	has(t, list, `<h2 id="categories.title">التصنيفات والبنود</h2>`)
	has(t, list, `<button id="categories.add" class="primary small" type="button" hx-get="/details/structure/category/new" hx-target="[id='web.details']">`)
	has(t, list, `<p id="categories.hint" class="hint wide">اسحب لتغيير الترتيب، وهو ترتيب التقرير الدوري.</p>`)
	has(t, list, `<tbody id="categories.card.`+category+`" data-id="`+category+`" data-reorder="/structure/category/`+category+`/reorder" draggable="true">`)
	has(t, list, `<strong id="categories.name.`+category+`">الحديد</strong>`)
	has(t, list, `<small id="categories.details.`+category+`" class="hint">البنود: 1</small>`)
	has(t, list, `<span id="categories.itemName.`+w.item+`">التسليح</span>`)
	has(t, list, `<small id="categories.itemDetails.`+w.item+`" class="hint">مشتريات</small>`)
	has(t, list, `data-reorder="/structure/reorder"`)
	found := w.browser.Send("GET", "/tab/categories?text=التسليح", nil).Body.String()
	has(t, found, `id="categories.card.`+category+`"`)
	if strings.Count(found, `<tbody id="categories.card.`) != 1 {
		t.Errorf("the search keeps the category whose item it finds: %d", strings.Count(found, `<tbody id="categories.card.`))
	}
	has(t, w.browser.Send("GET", "/tab/categories?text=nothing-here", nil).Body.String(), `<p id="categories.empty" class="empty">`)
}

func TestWeb_R174_UC012_CategoriesAndItemsChangeAsTheAppsDialogs(t *testing.T) {
	w := newWorking(t)
	w.partner("الشريك A", "")
	w.home()
	has(t, w.browser.Send("GET", "/details/structure/category/new", nil).Body.String(), `<form id="category.form" class="details-form" hx-post="/structure/category/new/add"`)
	has(t, w.browser.Send("POST", "/structure/category/new/add", url.Values{"name": {" "}}).Body.String(), `<p id="category.refusal" class="refusal" role="alert">أدخل اسماً من 1 إلى 100 حرف.</p>`)
	added := w.browser.Send("POST", "/structure/category/new/add", url.Values{"name": {"الخرسانة"}}).Body.String()
	has(t, added, `<h3 id="category.title">الخرسانة</h3>`)
	has(t, added, `<div id="web.content" hx-swap-oob="innerHTML">`)
	var category string
	if err := w.Database.Read.QueryRow("SELECT id FROM categories WHERE name = 'الخرسانة'").Scan(&category); err != nil {
		t.Fatal(err)
	}
	has(t, w.browser.Send("GET", "/details/structure/category/"+category+"?asking=rename", nil).Body.String(), `<input id="category.name" name="name" required value="الخرسانة" autofocus>`)
	has(t, w.browser.Send("POST", "/structure/category/"+category+"/rename", url.Values{"name": {"الخرسانة المسلحة"}}).Body.String(), `<h3 id="category.title">الخرسانة المسلحة</h3>`)
	disabled := w.browser.Send("POST", "/structure/category/"+category+"/disable", nil).Body.String()
	has(t, disabled, `<dd id="category.items">معطّل</dd>`)
	has(t, disabled, `id="categories.enable.`+category+`"`)
	has(t, w.browser.Send("POST", "/structure/category/"+category+"/enable", nil).Body.String(), `id="categories.disable.`+category+`"`)
	asking := w.browser.Send("GET", "/details/structure/category/"+category+"?asking=item", nil).Body.String()
	has(t, asking, `<input id="categories.itemType.both.input" type="radio" name="type" value="both" data-side="expenses" checked>`)
	has(t, asking, `<input id="categories.side.false.input" type="radio" name="side" value="expenses" checked>`)
	item := w.browser.Send("POST", "/structure/category/"+category+"/item", url.Values{"name": {"الصب"}, "type": {"purchases"}}).Body.String()
	has(t, item, `<h3 id="item.title">الصب</h3>`)
	has(t, item, `<dd id="item.type">مشتريات</dd>`)
	has(t, item, `<dd id="item.category">الخرسانة المسلحة</dd>`)
	var poured string
	if err := w.Database.Read.QueryRow("SELECT id FROM items WHERE name = 'الصب'").Scan(&poured); err != nil {
		t.Fatal(err)
	}
	has(t, w.browser.Send("POST", "/structure/category/"+category+"/item", url.Values{"name": {"رأس مال ثانٍ"}, "type": {"capital"}}).Body.String(),
		`<p id="item.refusal" class="refusal" role="alert">لهذا المشروع بند رأس المال بالفعل.</p>`)
	has(t, w.browser.Send("POST", "/structure/item/"+poured+"/rename", url.Values{"name": {"صب الخرسانة"}}).Body.String(), `<h3 id="item.title">صب الخرسانة</h3>`)
	has(t, w.browser.Send("GET", "/details/structure/item/"+poured+"?asking=type", nil).Body.String(), `<input id="categories.itemType.purchases.input" type="radio" name="type" value="purchases" data-side="expenses" checked>`)
	has(t, w.browser.Send("POST", "/structure/item/"+poured+"/type", url.Values{"type": {"contractorPayments"}}).Body.String(), `<dd id="item.type">دفعات مقاولين</dd>`)
	w.sent(100)
	has(t, w.browser.Send("POST", "/structure/item/"+w.item+"/type", url.Values{"type": {"sale"}}).Body.String(), "هذا البند عليه قيود، فلا يتغير نوعه.")
	has(t, w.browser.Send("POST", "/structure/item/"+poured+"/percentages", url.Values{"held": {"on"}}).Body.String(),
		`<input id="categories.tablePercentages.input" type="checkbox" name="held" value="on" checked`)
	if on := w.Count(`SELECT count(*) FROM items WHERE id = '` + poured + `' AND shows_table_percentages = 1`); on != 1 {
		t.Errorf("the table percentages are kept: %d", on)
	}
	has(t, w.browser.Send("POST", "/structure/item/"+poured+"/disable", nil).Body.String(), `<dd id="item.type">معطّل</dd>`)
	has(t, w.browser.Send("POST", "/structure/item/"+poured+"/enable", nil).Body.String(), `id="categories.disableItem"`)
	for _, path := range []string{"/structure/item/" + poured + "/archive", "/structure/item/" + uuid.NewString() + "/rename", "/structure/category/" + uuid.NewString() + "/rename",
		"/structure/unit/" + poured + "/rename", "/structure/category/new/rename"} {
		if code := w.browser.Send("POST", path, url.Values{"name": {"x"}}).Code; code != http.StatusNotFound {
			t.Errorf("%s: %d", path, code)
		}
	}
	for _, path := range []string{"/details/structure/item/" + uuid.NewString(), "/details/structure/category/" + uuid.NewString(), "/details/structure/unit/" + poured} {
		if code := w.browser.Send("GET", path, nil).Code; code != http.StatusNotFound {
			t.Errorf("%s: %d", path, code)
		}
	}
}

func TestWeb_R174_D1_ARowMovesAmongItsOwnAndTheOrderIsKept(t *testing.T) {
	w := newWorking(t)
	var category string
	if err := w.Database.Read.QueryRow("SELECT category_id FROM items WHERE id = ?", w.item).Scan(&category); err != nil {
		t.Fatal(err)
	}
	w.Ask("POST", w.path+"/categories/"+category+"/items", `{"name":"الربط","type":"purchases"}`, w.token)
	w.home()
	items := w.idsIn("SELECT id FROM items WHERE category_id = ? ORDER BY sort_order", category)
	slices.Reverse(items)
	moved := w.browser.Send("POST", "/structure/category/"+category+"/reorder", url.Values{"ids": {strings.Join(items, ",")}}).Body.String()
	if strings.Index(moved, `id="categories.item.`+items[0]+`"`) > strings.Index(moved, `id="categories.item.`+items[1]+`"`) {
		t.Error("the items show in the order kept")
	}
	if again := w.idsIn("SELECT id FROM items WHERE category_id = ? ORDER BY sort_order", category); !slices.Equal(again, items) {
		t.Errorf("the items' order is kept: %v", again)
	}
	categories := w.idsIn("SELECT id FROM categories WHERE project_id = ? ORDER BY sort_order", w.id)
	slices.Reverse(categories)
	w.browser.Send("POST", "/structure/reorder", url.Values{"ids": {strings.Join(categories, ",")}})
	if again := w.idsIn("SELECT id FROM categories WHERE project_id = ? ORDER BY sort_order", w.id); !slices.Equal(again, categories) {
		t.Errorf("the categories' order is kept: %v", again)
	}
	has(t, w.browser.Send("POST", "/structure/reorder", url.Values{"ids": {categories[0]}}).Body.String(),
		`<p id="categories.refusal" class="refusal wide" role="alert">تغيّرت القائمة. حاول مرة أخرى.</p>`)
	if code := w.browser.Send("POST", "/structure/category/x/reorder", nil).Code; code != http.StatusNotFound {
		t.Errorf("a category that is no id: %d", code)
	}
}

func TestWeb_R174_StructureIsForWhoeverShapesItAndAFinishedProjectChangesNothing(t *testing.T) {
	w := newWorking(t)
	builder := w.member("User B", "")
	browser := w.memberBrowser(builder)
	lacks(t, browser.Send("GET", "/", nil).Body.String(), `hx-get="/tab/categories"`)
	if code := browser.Send("POST", "/structure/item/"+w.item+"/rename", url.Values{"name": {"x"}}).Code; code != http.StatusForbidden {
		t.Errorf("renaming without ManageStructure: %d", code)
	}
	if code := browser.Send("POST", "/structure/reorder", nil).Code; code != http.StatusForbidden {
		t.Errorf("reordering without ManageStructure: %d", code)
	}
	if code := browser.Send("GET", "/details/structure/item/"+w.item, nil).Code; code != http.StatusNotFound {
		t.Errorf("an item's panel without ManageStructure: %d", code)
	}
	w.home()
	for _, ask := range []struct{ method, path string }{{"GET", "/tab/categories"}, {"GET", "/details/structure/item/" + w.item}, {"POST", "/structure/item/" + w.item + "/rename"},
		{"POST", "/structure/category/new/add"}, {"POST", "/structure/reorder"}} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, ask.method, ask.path, url.Values{"name": {"البند"}, "ids": {w.item}})
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s %s with the database failing at its call %d answers %d", ask.method, ask.path, n, answered.Code)
			}
		}
	}
	if code, answer := w.Ask("POST", w.path+"/close", ``, w.token); code >= 300 {
		t.Fatalf("closing: %d %v", code, answer)
	}
	finished := w.browser.Send("GET", "/tab/categories", nil).Body.String()
	has(t, finished, `hx-target="[id='web.details']" disabled>`)
	lacks(t, finished, `id="categories.hint"`)
	lacks(t, finished, `draggable="true"`)
	lacks(t, w.browser.Send("GET", "/details/structure/item/"+w.item, nil).Body.String(), `id="categories.renameItem"`)
	has(t, w.browser.Send("POST", "/structure/item/"+w.item+"/rename", url.Values{"name": {"x"}}).Body.String(), "هذا المشروع منتهٍ، فلا يتغير فيه شيء.")
	has(t, w.browser.Send("POST", "/structure/reorder", nil).Body.String(), "هذا المشروع منتهٍ، فلا يتغير فيه شيء.")
}

func TestWeb_R174_UC013_ColumnsListAsTheAppsScreen(t *testing.T) {
	w := newWorking(t)
	w.home()
	has(t, w.browser.Send("GET", "/details/structure/item/"+w.item, nil).Body.String(), `hx-get="/tab/itemColumns?item=`+w.item+`"`)
	own := w.browser.Send("GET", "/tab/itemColumns?item="+w.item, nil).Body.String()
	has(t, own, `<h2 id="columns.title">الأعمدة · التسليح</h2>`)
	has(t, own, `<p id="columns.itemDetails" class="hint wide">الحديد · مشتريات</p>`)
	has(t, own, `<button id="columns.thisItem" type="button" aria-pressed="true"`)
	has(t, own, `data-reorder="/columns/`+w.item+`/item/reorder"`)
	core := w.idsIn("SELECT id FROM item_columns WHERE item_id = ? AND is_core = 1 AND is_hidden = 0 ORDER BY sort_order", w.item)
	if len(core) == 0 {
		t.Fatal("a purchases item has core columns")
	}
	has(t, own, `<span id="columns.core.`+core[0]+`" class="chip"`)
	template := w.browser.Send("GET", "/tab/itemColumns?item="+w.item+"&scope=template", nil).Body.String()
	has(t, template, `<button id="columns.template" type="button" aria-pressed="true"`)
	has(t, template, `data-reorder="/columns/`+w.item+`/template/reorder"`)
	var sale string
	if err := w.Database.Read.QueryRow("SELECT items.id FROM items JOIN categories ON categories.id = items.category_id WHERE categories.project_id = ? AND items.type = 3 LIMIT 1", w.id).Scan(&sale); err != nil {
		t.Fatal(err)
	}
	revenue := w.browser.Send("GET", "/tab/itemColumns?item="+sale, nil).Body.String()
	has(t, revenue, `hx-target="[id='web.details']" disabled>`)
	has(t, revenue, "لا يُضاف عمود إلى بند إيرادات: يطلب ما يعطيه قالبه.")
	lacks(t, w.browser.Send("GET", "/tab/itemColumns?item="+uuid.NewString(), nil).Body.String(), `id="columns.itemDetails"`)
}

func TestWeb_R174_UC013_NewEditAndDeleteAColumnAsTheAppsDialogs(t *testing.T) {
	w := newWorking(t)
	w.home()
	page := "/columns/" + w.item + "/item"
	asking := w.browser.Send("GET", "/details/columns/"+w.item+"/item/new", nil).Body.String()
	has(t, asking, `<option id="columns.dataTypeOption.text" value="text" selected>`)
	lacks(t, asking, `value="file"`)
	lacks(t, asking, `id="applyToAll.dialog"`)
	has(t, w.browser.Send("POST", page+"/new/add", url.Values{"name": {" "}, "dataType": {"text"}}).Body.String(), `<p id="column.refusal" class="refusal" role="alert">أدخل اسماً من 1 إلى 100 حرف.</p>`)
	has(t, w.browser.Send("POST", page+"/new/add", url.Values{"name": {"الدرجة"}, "dataType": {"list"}, "choices": {""}}).Body.String(), "أدخل خياراً واحداً على الأقل")
	added := w.browser.Send("POST", page+"/new/add", url.Values{"name": {"الملاحظة"}, "dataType": {"text"}, "required": {"on"}, "reports": {"on"}}).Body.String()
	has(t, added, `<h3 id="column.title">الملاحظة</h3>`)
	has(t, added, `<dd id="column.type">مخصص · نص · إلزامي عند الإدخال · في التقارير</dd>`)
	has(t, added, `<div id="web.content" hx-swap-oob="innerHTML">`)
	var note string
	if err := w.Database.Read.QueryRow("SELECT id FROM item_columns WHERE item_id = ? AND name = 'الملاحظة'", w.item).Scan(&note); err != nil {
		t.Fatal(err)
	}
	has(t, w.browser.Send("GET", "/details/columns/"+w.item+"/item/"+note+"?asking=edit", nil).Body.String(), `<input id="columns.columnName" name="name" required value="الملاحظة" autofocus>`)
	has(t, w.browser.Send("POST", page+"/"+note+"/edit", url.Values{"name": {"ملاحظة"}}).Body.String(), `<dd id="column.type">مخصص · نص · اختياري</dd>`)
	w.browser.Send("POST", page+"/new/add", url.Values{"name": {"الدرجة"}, "dataType": {"list"}, "choices": {"أولى\r\nثانية"}, "reports": {"on"}})
	var grade string
	if err := w.Database.Read.QueryRow("SELECT id FROM item_columns WHERE item_id = ? AND name = 'الدرجة'", w.item).Scan(&grade); err != nil {
		t.Fatal(err)
	}
	has(t, w.browser.Send("GET", "/details/columns/"+w.item+"/item/"+grade+"?asking=edit", nil).Body.String(), `<p id="columns.existingChoices" class="hint">أولى · ثانية</p>`)
	has(t, w.browser.Send("POST", page+"/"+grade+"/edit", url.Values{"name": {"الدرجة"}, "reports": {"on"}, "choices": {"ثالثة"}}).Body.String(), `<dd id="column.choices">أولى · ثانية · ثالثة</dd>`)
	has(t, w.browser.Send("GET", "/details/columns/"+w.item+"/item/"+note+"?asking=delete", nil).Body.String(), `<p id="column.deleteQuestion" class="section-title">حذفه؟</p>`)
	deleted := w.browser.Send("POST", page+"/"+note+"/delete", nil).Body.String()
	lacks(t, deleted, `id="columns.row.`+note+`"`)
	if hidden := w.Count(`SELECT count(*) FROM item_columns WHERE id = '` + note + `' AND is_hidden = 1`); hidden != 1 {
		t.Errorf("a deleted column is hidden, its values kept: %d", hidden)
	}
	core := w.idsIn("SELECT id FROM item_columns WHERE item_id = ? AND is_core = 1", w.item)
	lacks(t, w.browser.Send("GET", "/details/columns/"+w.item+"/item/"+core[0], nil).Body.String(), `id="columns.delete"`)
	has(t, w.browser.Send("POST", page+"/"+core[0]+"/delete", nil).Body.String(), "العمود الأساسي لا يُحذف، ولا يتغير منه إلا اسمه.")
	columns := w.idsIn("SELECT id FROM item_columns WHERE item_id = ? AND is_hidden = 0 ORDER BY sort_order", w.item)
	slices.Reverse(columns)
	w.browser.Send("POST", page+"/reorder", url.Values{"ids": {strings.Join(columns, ",")}})
	if again := w.idsIn("SELECT id FROM item_columns WHERE item_id = ? AND is_hidden = 0 ORDER BY sort_order", w.item); !slices.Equal(again, columns) {
		t.Errorf("the item's columns keep their new order: %v", again)
	}
	has(t, w.browser.Send("POST", page+"/reorder", url.Values{"ids": {columns[0]}}).Body.String(), `<p id="columns.refusal" class="refusal wide" role="alert">تغيّرت القائمة. حاول مرة أخرى.</p>`)
	for _, path := range []string{page + "/" + note + "/archive", page + "/" + uuid.NewString() + "/edit", "/columns/" + w.item + "/unit/new/add", "/columns/" + uuid.NewString() + "/item/new/add",
		"/columns/x/item/new/add", page + "/new/edit"} {
		if code := w.browser.Send("POST", path, url.Values{"name": {"x"}}).Code; code != http.StatusNotFound {
			t.Errorf("%s: %d", path, code)
		}
	}
	for _, path := range []string{"/details/columns/" + w.item + "/item/" + uuid.NewString(), "/details/columns/" + w.item + "/unit/new", "/details/columns/" + uuid.NewString() + "/item/new"} {
		if code := w.browser.Send("GET", path, nil).Code; code != http.StatusNotFound {
			t.Errorf("%s: %d", path, code)
		}
	}
}

func TestWeb_R174_UC013_ATemplatesChangeAsksWhetherItReachesEveryItem(t *testing.T) {
	w := newWorking(t)
	w.home()
	page := "/columns/" + w.item + "/template"
	asking := w.browser.Send("GET", "/details/columns/"+w.item+"/template/new", nil).Body.String()
	has(t, asking, `<button id="applyToAll.templateOnly" class="quiet-button" type="submit" name="applyToAll" value="false">القالب فقط</button>`)
	has(t, asking, `<button id="applyToAll.apply" class="primary small" type="submit" name="applyToAll" value="true">تطبيق على كل البنود</button>`)
	w.browser.Send("POST", page+"/new/add", url.Values{"name": {"المورد الفرعي"}, "dataType": {"text"}, "applyToAll": {"false"}})
	w.browser.Send("POST", page+"/new/add", url.Values{"name": {"رقم الطلب"}, "dataType": {"number"}, "applyToAll": {"true"}})
	if only := w.Count(`SELECT count(*) FROM item_columns WHERE item_id = '` + w.item + `' AND name = 'المورد الفرعي'`); only != 0 {
		t.Errorf("the template only leaves the items: %d", only)
	}
	if every := w.Count(`SELECT count(*) FROM item_columns WHERE item_id = '` + w.item + `' AND name = 'رقم الطلب' AND is_hidden = 0`); every != 1 {
		t.Errorf("every item takes a column applied to all: %d", every)
	}
	var order string
	if err := w.Database.Read.QueryRow("SELECT id FROM default_columns WHERE project_id = ? AND name = 'رقم الطلب'", w.id).Scan(&order); err != nil {
		t.Fatal(err)
	}
	has(t, w.browser.Send("POST", page+"/"+order+"/edit", url.Values{"name": {"رقم أمر الشراء"}, "applyToAll": {"true"}}).Body.String(), `<h3 id="column.title">رقم أمر الشراء</h3>`)
	if renamed := w.Count(`SELECT count(*) FROM item_columns WHERE item_id = '` + w.item + `' AND name = 'رقم أمر الشراء'`); renamed != 1 {
		t.Errorf("an edit applied to all reaches the items: %d", renamed)
	}
	has(t, w.browser.Send("GET", "/details/columns/"+w.item+"/template/"+order+"?asking=delete", nil).Body.String(), `id="applyToAll.apply"`)
	w.browser.Send("POST", page+"/"+order+"/delete", url.Values{"applyToAll": {"true"}})
	if kept := w.Count(`SELECT count(*) FROM item_columns WHERE item_id = '` + w.item + `' AND name = 'رقم أمر الشراء' AND is_hidden = 0`); kept != 0 {
		t.Errorf("a delete applied to all hides it on the items: %d", kept)
	}
	template := w.idsIn("SELECT id FROM default_columns WHERE project_id = ? AND item_type = 0 ORDER BY sort_order", w.id)
	slices.Reverse(template)
	asked := w.browser.Send("POST", page+"/reorder", url.Values{"ids": {strings.Join(template, ",")}})
	if asked.Header().Get("HX-Retarget") != "[id='web.details']" {
		t.Errorf("a template's order asks first, in the details panel: %q", asked.Header().Get("HX-Retarget"))
	}
	has(t, asked.Body.String(), `<input id="column.orderIds" type="hidden" name="ids" value="`+strings.Join(template, ",")+`">`)
	w.browser.Send("POST", page+"/reorder", url.Values{"ids": {strings.Join(template, ",")}, "applyToAll": {"false"}})
	if again := w.idsIn("SELECT id FROM default_columns WHERE project_id = ? AND item_type = 0 ORDER BY sort_order", w.id); !slices.Equal(again, template) {
		t.Errorf("the template keeps its new order: %v", again)
	}
}

func TestWeb_R174_UC013_ColumnsAreForWhoeverShapesTheStructure(t *testing.T) {
	w := newWorking(t)
	builder := w.member("User B", "")
	browser := w.memberBrowser(builder)
	if code := browser.Send("POST", "/columns/"+w.item+"/item/new/add", url.Values{"name": {"x"}, "dataType": {"text"}}).Code; code != http.StatusForbidden {
		t.Errorf("a new column without ManageStructure: %d", code)
	}
	if code := browser.Send("POST", "/columns/"+w.item+"/item/reorder", nil).Code; code != http.StatusForbidden {
		t.Errorf("an order without ManageStructure: %d", code)
	}
	if code := browser.Send("GET", "/details/columns/"+w.item+"/item/new", nil).Code; code != http.StatusNotFound {
		t.Errorf("a column's form without ManageStructure: %d", code)
	}
	w.home()
	for _, ask := range []struct{ method, path string }{{"GET", "/tab/itemColumns?item=" + w.item}, {"GET", "/tab/itemColumns?item=" + w.item + "&scope=template"},
		{"GET", "/details/columns/" + w.item + "/item/new"}, {"POST", "/columns/" + w.item + "/item/new/add"}, {"POST", "/columns/" + w.item + "/template/reorder"},
		{"POST", "/columns/" + w.item + "/item/reorder"}} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, ask.method, ask.path, url.Values{"name": {"الملاحظة"}, "dataType": {"text"}, "ids": {w.item}, "applyToAll": {"false"}})
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s %s with the database failing at its call %d answers %d", ask.method, ask.path, n, answered.Code)
			}
		}
	}
	if code, answer := w.Ask("POST", w.path+"/close", ``, w.token); code >= 300 {
		t.Fatalf("closing: %d %v", code, answer)
	}
	finished := w.browser.Send("GET", "/tab/itemColumns?item="+w.item, nil).Body.String()
	lacks(t, finished, `draggable="true"`)
	lacks(t, finished, `id="columns.hint"`)
	has(t, w.browser.Send("POST", "/columns/"+w.item+"/item/new/add", url.Values{"name": {"x"}, "dataType": {"text"}}).Body.String(), "هذا المشروع منتهٍ، فلا يتغير فيه شيء.")
	has(t, w.browser.Send("POST", "/columns/"+w.item+"/template/reorder", nil).Body.String(), "هذا المشروع منتهٍ، فلا يتغير فيه شيء.")
}

func TestWeb_R174_UC012_TablesListAsTheAppsScreen(t *testing.T) {
	w := newWorking(t)
	w.sent(100)
	w.home()
	has(t, w.browser.Send("GET", "/details/structure/item/"+w.item, nil).Body.String(), `hx-get="/tab/itemTables?item=`+w.item+`"`)
	tables := w.browser.Send("GET", "/tab/itemTables?item="+w.item, nil).Body.String()
	has(t, tables, `<h2 id="tables.title">جداول البند · التسليح</h2>`)
	has(t, tables, `<p id="tables.hint" class="hint wide">`)
	has(t, tables, `<button id="tables.add" class="primary small" type="button" hx-get="/details/tables/`+w.item+`/new" hx-target="[id='web.details']">`)
	has(t, tables, `<tr id="tables.row.`+w.table+`" data-id="`+w.table+`" draggable="true"`)
	has(t, tables, `<span id="tables.name.`+w.table+`">التسليح</span>`)
	has(t, tables, `<small id="tables.details.`+w.table+`" class="hint">مشتريات · 0</small>`)
	has(t, tables, `data-reorder="/tables/`+w.item+`/reorder"`)
	lacks(t, w.browser.Send("GET", "/details/tables/"+w.item+"/"+w.table, nil).Body.String(), `id="tables.rename"`)
	searched := w.browser.Send("GET", "/tab/itemTables?item="+w.item+"&text=nothing-here", nil).Body.String()
	has(t, searched, `<p id="tables.empty" class="empty">لا يوجد جدول مطابق.</p>`)
	lacks(t, w.browser.Send("GET", "/tab/itemTables?item="+w.item+"&text=التسليح", nil).Body.String(), `draggable="true"`)
	var sale string
	if err := w.Database.Read.QueryRow("SELECT items.id FROM items JOIN categories ON categories.id = items.category_id WHERE categories.project_id = ? AND items.type = 3 LIMIT 1", w.id).Scan(&sale); err != nil {
		t.Fatal(err)
	}
	revenue := w.browser.Send("GET", "/tab/itemTables?item="+sale, nil).Body.String()
	has(t, revenue, `hx-target="[id='web.details']" disabled>`)
	has(t, revenue, "إيرادات · 0")
	lacks(t, w.browser.Send("GET", "/tab/itemTables?item="+uuid.NewString(), nil).Body.String(), `id="tables.hint"`)
}

func TestWeb_R174_UC012_NewSubgroupRenameAndDeleteAsTheAppsDialogs(t *testing.T) {
	w := newWorking(t)
	w.home()
	page := "/tables/" + w.item
	has(t, w.browser.Send("GET", "/details/tables/"+w.item+"/new", nil).Body.String(), `<form id="table.form" class="details-form" hx-post="/tables/`+w.item+`/new/add"`)
	has(t, w.browser.Send("POST", page+"/new/add", url.Values{"name": {" "}}).Body.String(), `<p id="table.refusal" class="refusal" role="alert">أدخل اسماً من 1 إلى 100 حرف.</p>`)
	added := w.browser.Send("POST", page+"/new/add", url.Values{"name": {"الدور الأول"}}).Body.String()
	has(t, added, `<h3 id="table.title">الدور الأول</h3>`)
	has(t, added, `<div id="web.content" hx-swap-oob="innerHTML">`)
	var floor string
	if err := w.Database.Read.QueryRow("SELECT id FROM subgroups WHERE item_id = ? AND name = 'الدور الأول'", w.item).Scan(&floor); err != nil {
		t.Fatal(err)
	}
	has(t, added, `id="tables.rename"`)
	has(t, w.browser.Send("GET", "/details/tables/"+w.item+"/"+floor+"?asking=rename", nil).Body.String(), `<input id="table.name" name="name" required value="الدور الأول" autofocus>`)
	has(t, w.browser.Send("POST", page+"/"+floor+"/rename", url.Values{"name": {"الدور الثاني"}}).Body.String(), `<h3 id="table.title">الدور الثاني</h3>`)
	has(t, w.browser.Send("POST", page+"/"+w.table+"/rename", url.Values{"name": {"العام"}}).Body.String(), "المجموعة العامة للبند لا يُعدَّل اسمها ولا تُحذف.")
	order := w.idsIn("SELECT id FROM subgroups WHERE item_id = ? ORDER BY sort_order", w.item)
	slices.Reverse(order)
	w.browser.Send("POST", page+"/reorder", url.Values{"ids": {strings.Join(order, ",")}})
	if again := w.idsIn("SELECT id FROM subgroups WHERE item_id = ? ORDER BY sort_order", w.item); !slices.Equal(again, order) {
		t.Errorf("the tables keep their new order: %v", again)
	}
	has(t, w.browser.Send("POST", page+"/reorder", url.Values{"ids": {order[0]}}).Body.String(), `<p id="tables.refusal" class="refusal wide" role="alert">تغيّرت القائمة. حاول مرة أخرى.</p>`)
	has(t, w.browser.Send("GET", "/details/tables/"+w.item+"/"+floor+"?asking=delete", nil).Body.String(), `<p id="table.deleteQuestion" class="section-title">حذفه؟</p>`)
	deleted := w.browser.Send("POST", page+"/"+floor+"/delete", nil).Body.String()
	lacks(t, deleted, `id="tables.row.`+floor+`"`)
	has(t, deleted, `<div id="web.content" hx-swap-oob="innerHTML">`)
	for _, path := range []string{page + "/" + w.table + "/archive", page + "/" + uuid.NewString() + "/rename", "/tables/" + uuid.NewString() + "/new/add", "/tables/x/new/add",
		page + "/new/rename"} {
		if code := w.browser.Send("POST", path, url.Values{"name": {"x"}}).Code; code != http.StatusNotFound {
			t.Errorf("%s: %d", path, code)
		}
	}
	for _, path := range []string{"/details/tables/" + w.item + "/" + uuid.NewString(), "/details/tables/" + uuid.NewString() + "/new", "/details/tables/x/new"} {
		if code := w.browser.Send("GET", path, nil).Code; code != http.StatusNotFound {
			t.Errorf("%s: %d", path, code)
		}
	}
	if code := w.browser.Send("POST", "/tables/x/reorder", nil).Code; code != http.StatusNotFound {
		t.Errorf("an item that is no id: %d", code)
	}
}

func TestWeb_R174_UC012_TablesAreForWhoeverShapesTheStructure(t *testing.T) {
	w := newWorking(t)
	builder := w.member("User B", "")
	browser := w.memberBrowser(builder)
	if code := browser.Send("POST", "/tables/"+w.item+"/new/add", url.Values{"name": {"x"}}).Code; code != http.StatusForbidden {
		t.Errorf("a new subgroup without ManageStructure: %d", code)
	}
	if code := browser.Send("POST", "/tables/"+w.item+"/reorder", nil).Code; code != http.StatusForbidden {
		t.Errorf("an order without ManageStructure: %d", code)
	}
	if code := browser.Send("GET", "/details/tables/"+w.item+"/new", nil).Code; code != http.StatusNotFound {
		t.Errorf("a subgroup's form without ManageStructure: %d", code)
	}
	w.home()
	for _, ask := range []struct{ method, path string }{{"GET", "/tab/itemTables?item=" + w.item}, {"GET", "/details/tables/" + w.item + "/new"},
		{"POST", "/tables/" + w.item + "/new/add"}, {"POST", "/tables/" + w.item + "/reorder"}} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, ask.method, ask.path, url.Values{"name": {"الدور الأول"}, "ids": {w.table}})
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s %s with the database failing at its call %d answers %d", ask.method, ask.path, n, answered.Code)
			}
		}
	}
	if code, answer := w.Ask("POST", w.path+"/close", ``, w.token); code >= 300 {
		t.Fatalf("closing: %d %v", code, answer)
	}
	finished := w.browser.Send("GET", "/tab/itemTables?item="+w.item, nil).Body.String()
	lacks(t, finished, `draggable="true"`)
	has(t, finished, `hx-target="[id='web.details']" disabled>`)
	has(t, w.browser.Send("POST", "/tables/"+w.item+"/new/add", url.Values{"name": {"x"}}).Body.String(), "هذا المشروع منتهٍ، فلا يتغير فيه شيء.")
	has(t, w.browser.Send("POST", "/tables/"+w.item+"/reorder", nil).Body.String(), "هذا المشروع منتهٍ، فلا يتغير فيه شيء.")
}

func TestWeb_R174_ATabOpenedForOneRecordShowsItAgain(t *testing.T) {
	w := newWorking(t)
	w.home()
	has(t, w.browser.Send("GET", "/tab/itemTables?item="+w.item, nil).Body.String(), `<h2 id="tables.title">جداول البند · التسليح</h2>`)
	w.browser.Send("GET", "/tab/home", nil)
	has(t, w.browser.Send("GET", "/tab/itemTables", nil).Body.String(), `<h2 id="tables.title">جداول البند · التسليح</h2>`)
	has(t, w.browser.Send("GET", "/", nil).Body.String(), `<h2 id="tables.title">جداول البند · التسليح</h2>`)
	w.browser.Send("DELETE", "/tab/itemTables", nil)
	has(t, w.browser.Send("GET", "/tab/itemTables", nil).Body.String(), `<h2 id="tables.title">جداول البند</h2>`)
	w.browser.Send("GET", "/tab/users?text=nobody", nil)
	lacks(t, w.browser.Send("GET", "/tab/users", nil).Body.String(), `value="nobody"`)
}

func TestWeb_R174_UC014_PaymentMethodsListAsTheAppsScreen(t *testing.T) {
	w := newWorking(t)
	var name string
	if err := w.Database.Read.QueryRow("SELECT name FROM payment_methods WHERE id = ?", w.method).Scan(&name); err != nil {
		t.Fatal(err)
	}
	w.home()
	list := w.browser.Send("GET", "/tab/paymentMethods", nil).Body.String()
	has(t, list, `<h2 id="paymentMethods.title">طرق الدفع</h2>`)
	has(t, list, `<button id="paymentMethods.add" class="primary small" type="button" hx-get="/details/payment-method/new" hx-target="[id='web.details']">`)
	has(t, list, `<span id="paymentMethods.name.`+w.method+`">`+name+`</span>`)
	has(t, list, `<small id="paymentMethods.paysFrom.`+w.method+`" class="hint">يدفع من: حساب المؤسسة</small>`)
}

func TestWeb_R174_UC014_PaymentMethodsChangeAsTheAppsDialog(t *testing.T) {
	w := newWorking(t)
	w.sent(100)
	w.home()
	asking := w.browser.Send("GET", "/details/payment-method/new", nil).Body.String()
	has(t, asking, `<input id="paymentMethods.accountTick.`+w.account+`.input" type="checkbox" name="account" value="`+w.account+`" checked>`)
	has(t, w.browser.Send("POST", "/payment-method/new/add", url.Values{"name": {" "}, "account": {w.account}}).Body.String(), `<p id="paymentMethod.refusal" class="refusal" role="alert">أدخل اسماً من 1 إلى 100 حرف.</p>`)
	has(t, w.browser.Send("POST", "/payment-method/new/add", url.Values{"name": {"تحويل"}}).Body.String(), "اختر حسابًا واحدًا على الأقل تدفع منه هذه الطريقة")
	added := w.browser.Send("POST", "/payment-method/new/add", url.Values{"name": {"تحويل"}, "account": {w.account}}).Body.String()
	has(t, added, `<h3 id="paymentMethod.title">تحويل</h3>`)
	has(t, added, `<p id="paymentMethod.paysFrom" class="hint">يدفع من: حساب المؤسسة</p>`)
	has(t, added, `<div id="web.content" hx-swap-oob="innerHTML">`)
	var transfer string
	if err := w.Database.Read.QueryRow("SELECT id FROM payment_methods WHERE project_id = ? AND name = 'تحويل'", w.id).Scan(&transfer); err != nil {
		t.Fatal(err)
	}
	has(t, w.browser.Send("GET", "/details/payment-method/"+transfer+"?asking=rename", nil).Body.String(), `<input id="paymentMethod.name" name="name" required value="تحويل" autofocus>`)
	has(t, w.browser.Send("POST", "/payment-method/"+transfer+"/rename", url.Values{"name": {"تحويل بنكي"}, "account": {w.account}}).Body.String(), `<h3 id="paymentMethod.title">تحويل بنكي</h3>`)
	disabled := w.browser.Send("POST", "/payment-method/"+transfer+"/disable", nil).Body.String()
	has(t, disabled, `<p id="paymentMethod.disabled" class="hint">معطّلة</p>`)
	has(t, disabled, `id="paymentMethods.enable"`)
	has(t, w.browser.Send("POST", "/payment-method/"+transfer+"/enable", nil).Body.String(), `id="paymentMethods.disable"`)
	has(t, w.browser.Send("GET", "/details/payment-method/"+transfer+"?asking=delete", nil).Body.String(), `<p id="paymentMethod.deleteQuestion" class="section-title">حذفه؟</p>`)
	lacks(t, w.browser.Send("POST", "/payment-method/"+transfer+"/delete", nil).Body.String(), `id="paymentMethods.row.`+transfer+`"`)
	has(t, w.browser.Send("POST", "/payment-method/"+w.method+"/delete", nil).Body.String(), "دُفع بهذه الطريقة مصروف، فلا تُحذف. عطّلها بدلاً من ذلك.")
	for _, path := range []string{"/payment-method/" + w.method + "/archive", "/payment-method/" + uuid.NewString() + "/rename", "/payment-method/new/rename"} {
		if code := w.browser.Send("POST", path, url.Values{"name": {"x"}}).Code; code != http.StatusNotFound {
			t.Errorf("%s: %d", path, code)
		}
	}
	if code := w.browser.Send("GET", "/details/payment-method/"+uuid.NewString(), nil).Code; code != http.StatusNotFound {
		t.Errorf("a method the project does not hold: %d", code)
	}
}

func TestWeb_R174_UC014_PaymentMethodsAreForWhoeverManagesTheSettings(t *testing.T) {
	w := newWorking(t)
	builder := w.member("User B", "")
	browser := w.memberBrowser(builder)
	lacks(t, browser.Send("GET", "/", nil).Body.String(), `hx-get="/tab/paymentMethods"`)
	if code := browser.Send("POST", "/payment-method/"+w.method+"/disable", nil).Code; code != http.StatusForbidden {
		t.Errorf("disabling without ManageSettings: %d", code)
	}
	if code := browser.Send("GET", "/details/payment-method/"+w.method, nil).Code; code != http.StatusNotFound {
		t.Errorf("a method's panel without ManageSettings: %d", code)
	}
	w.home()
	for _, ask := range []struct{ method, path string }{{"GET", "/tab/paymentMethods"}, {"GET", "/details/payment-method/new"}, {"GET", "/details/payment-method/" + w.method},
		{"POST", "/payment-method/new/add"}, {"POST", "/payment-method/" + w.method + "/rename"}} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, ask.method, ask.path, url.Values{"name": {"تحويل"}, "account": {w.account}})
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s %s with the database failing at its call %d answers %d", ask.method, ask.path, n, answered.Code)
			}
		}
	}
	if code, answer := w.Ask("POST", w.path+"/close", ``, w.token); code >= 300 {
		t.Fatalf("closing: %d %v", code, answer)
	}
	has(t, w.browser.Send("GET", "/tab/paymentMethods", nil).Body.String(), `hx-target="[id='web.details']" disabled>`)
	lacks(t, w.browser.Send("GET", "/details/payment-method/"+w.method, nil).Body.String(), `id="paymentMethods.rename"`)
	has(t, w.browser.Send("POST", "/payment-method/"+w.method+"/disable", nil).Body.String(), "هذا المشروع منتهٍ، فلا يتغير فيه شيء.")
}

func TestWeb_R175_UC006_MoreInfoShowsTheProjectAsTheAppsScreen(t *testing.T) {
	w := newWorking(t)
	w.home()
	has(t, w.browser.Send("GET", "/", nil).Body.String(), `hx-get="/tab/projectInfo"`)
	info := w.browser.Send("GET", "/tab/projectInfo", nil).Body.String()
	has(t, info, `<h2 id="projectInfo.title">معلومات المشروع</h2>`)
	has(t, info, `<h3 id="projectInfo.name" class="section-title">مشروع A</h3>`)
	has(t, info, `<small id="projectInfo.details" class="hint">تاريخ البدء 2026-11-01</small>`)
	has(t, info, `id="projectInfo.timeZone"`)
	has(t, info, `>A1</span>`)
	has(t, info, `id="projectInfo.rename"`)
	has(t, info, `<textarea id="projectInfo.description" name="value" rows="4" maxlength="2000"></textarea>`)
	builder := w.member("User B", "")
	reader := w.memberBrowser(builder).Send("GET", "/tab/projectInfo", nil).Body.String()
	lacks(t, reader, `id="projectInfo.rename"`)
	has(t, reader, `<p id="projectInfo.descriptionText" class="hint">لا يوجد وصف بعد.</p>`)
}

func TestWeb_R175_UC006_MoreInfoChangesAsTheAppsDialogs(t *testing.T) {
	w := newWorking(t)
	w.home()
	has(t, w.browser.Send("GET", "/details/project-info/rename", nil).Body.String(), `<input id="projectInfo.field" name="value" required value="مشروع A" autofocus>`)
	has(t, w.browser.Send("POST", "/project-info/rename", url.Values{"value": {" "}}).Body.String(), `<p id="projectInfo.formRefusal" class="refusal" role="alert">اكتب اسماً من 1 إلى 100 حرف.</p>`)
	renamed := w.browser.Send("POST", "/project-info/rename", url.Values{"value": {"مشروع B"}}).Body.String()
	has(t, renamed, `<h3 id="projectInfo.name" class="section-title">مشروع B</h3>`)
	has(t, renamed, `<span id="web.sideProject" class="side-project" hx-swap-oob="true">مشروع B</span>`)
	has(t, renamed, `id="web.project"`)
	has(t, w.browser.Send("GET", "/details/project-info/timeZone", nil).Body.String(), `id="projectInfo.timeZoneOption.Asia/Riyadh.input"`)
	has(t, w.browser.Send("POST", "/project-info/timeZone", url.Values{"value": {"Mars/Olympus"}}).Body.String(), "هذه المنطقة الزمنية غير متاحة")
	w.browser.Send("POST", "/project-info/timeZone", url.Values{"value": {"Asia/Dubai"}})
	if kept := w.Count(`SELECT count(*) FROM project_time_zones WHERE project_id = '` + w.id + `' AND time_zone = 'Asia/Dubai'`); kept != 1 {
		t.Errorf("the time zone is kept: %d", kept)
	}
	has(t, w.browser.Send("POST", "/project-info/unit", url.Values{"value": {"A1"}}).Body.String(), "توجد وحدة بهذا الاسم في هذا المشروع.")
	has(t, w.browser.Send("POST", "/project-info/unit", url.Values{"value": {"A2"}}).Body.String(), `>A2</span>`)
	var unit string
	if err := w.Database.Read.QueryRow("SELECT id FROM units WHERE project_id = ? AND name = 'A2'", w.id).Scan(&unit); err != nil {
		t.Fatal(err)
	}
	has(t, w.browser.Send("GET", "/details/project-info/unit?unit="+unit, nil).Body.String(), `value="A2"`)
	has(t, w.browser.Send("POST", "/project-info/unit", url.Values{"unit": {unit}, "value": {"B2"}}).Body.String(), `>B2</span>`)
	has(t, w.browser.Send("GET", "/details/project-info/deleteUnit?unit="+unit, nil).Body.String(), "B2 - حذفه؟")
	lacks(t, w.browser.Send("POST", "/project-info/deleteUnit", url.Values{"unit": {unit}}).Body.String(), `>B2</span>`)
	has(t, w.browser.Send("POST", "/project-info/describe", url.Values{"value": {strings.Repeat("و", 2001)}}).Body.String(), "اكتب 2000 حرف على الأكثر.")
	has(t, w.browser.Send("POST", "/project-info/describe", url.Values{"value": {"فلل سكنية"}}).Body.String(), `<p id="projectInfo.notice" class="notice" role="status">تم حفظ الوصف.</p>`)
	w.sent(100)
	has(t, w.browser.Send("GET", "/details/project-info/close", nil).Body.String(), `<p id="projectInfo.question" class="section-title">إغلاق هذا المشروع؟</p>`)
	has(t, w.browser.Send("POST", "/project-info/close", nil).Body.String(), "في هذا المشروع قيود بانتظار الاعتماد.")
	for _, path := range []string{"/project-info/archive", "/project-info/deleteUnit"} {
		if code := w.browser.Send("POST", path, url.Values{"unit": {uuid.NewString()}}).Code; code != http.StatusNotFound {
			t.Errorf("%s: %d", path, code)
		}
	}
	if code := w.browser.Send("GET", "/details/project-info/unit?unit="+uuid.NewString(), nil).Code; code != http.StatusNotFound {
		t.Errorf("a unit the project does not have: %d", code)
	}
}

func TestWeb_R175_UC006_ClosingTheProjectLeavesItReadOnly(t *testing.T) {
	w := newWorking(t)
	w.home()
	closed := w.browser.Send("POST", "/project-info/close", nil).Body.String()
	has(t, closed, `<small id="projectInfo.details" class="hint">تاريخ البدء 2026-11-01 · للقراءة فقط</small>`)
	lacks(t, closed, `id="projectInfo.close"`)
	has(t, w.browser.Send("GET", "/details/project-info/rename", nil).Body.String(), `id="projectInfo.field"`)
	has(t, w.browser.Send("POST", "/project-info/rename", url.Values{"value": {"مشروع C"}}).Body.String(), "هذا المشروع منتهٍ، فلا يمكن تغييره.")
}

func TestWeb_R175_MoreInfoChangesAreForWhoeverManagesTheProject(t *testing.T) {
	w := newWorking(t)
	builder := w.member("User B", "")
	browser := w.memberBrowser(builder)
	if code := browser.Send("POST", "/project-info/rename", url.Values{"value": {"x"}}).Code; code != http.StatusForbidden {
		t.Errorf("renaming without ManageProject: %d", code)
	}
	if code := browser.Send("GET", "/details/project-info/rename", nil).Code; code != http.StatusNotFound {
		t.Errorf("Rename's form without ManageProject: %d", code)
	}
	w.home()
	for _, ask := range []struct{ method, path string }{{"GET", "/tab/projectInfo"}, {"GET", "/details/project-info/timeZone"}, {"POST", "/project-info/rename"},
		{"POST", "/project-info/describe"}} {
		for n := int64(1); ; n++ {
			answered, failed := w.browser.SendFailing(n, ask.method, ask.path, url.Values{"value": {"مشروع B"}})
			if !failed {
				break
			}
			if answered.Code != http.StatusInternalServerError {
				t.Errorf("%s %s with the database failing at its call %d answers %d", ask.method, ask.path, n, answered.Code)
			}
		}
	}
}

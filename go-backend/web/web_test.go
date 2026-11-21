// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web_test

import (
	"compress/gzip"
	"io"
	"net/http"
	"net/url"
	"regexp"
	"slices"
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
	signedOut := api.Browser().Send("GET", "/", nil).Body.String()
	has(t, signedOut, `<span id="signIn.registerProject" aria-disabled="true">`)
	browser, _ := signedIn(t, api, "User A")
	page := browser.Send("GET", "/", nil).Body.String()
	for _, key := range []string{"home", "entries", "approvals", "accounts", "partners", "contracts", "reports", "history"} {
		has(t, page, `<span id="web.item.`+key+`" class="parked" aria-disabled="true">`)
	}
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
	has(t, browser.Send("DELETE", "/tab/settings", nil).Body.String(), "اختر من القائمة الجانبية لتبدأ.")
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

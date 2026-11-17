package projects_test

import (
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
)

// redix-app #107, D1: a project is in Asia/Riyadh until its manager puts it in another zone of the list; the project answers with its zone and
// the zone's offset now, and a change is a History line, written once.
func TestR4_107_AManagerPutsTheProjectInAZoneOfTheList(t *testing.T) {
	a := newAPI(t)
	_, token := a.person("starter", false)
	id := a.started(token, true)
	project := "/projects/" + id
	_, listed := a.Ask("GET", "/projects", ``, token)
	for _, one := range listed.([]any) {
		if testapi.Field(one, "id") == id && (testapi.Field(one, "timeZone") != "Asia/Riyadh" || testapi.Field(one, "utcOffsetMinutes") != float64(180)) {
			t.Fatalf("a new project: %v", one)
		}
	}
	for range 2 {
		if code, answer := a.Ask("PUT", project+"/time-zone", `{"timeZone":"Asia/Shanghai"}`, token); code != 200 || testapi.Field(answer, "timeZone") != "Asia/Shanghai" ||
			testapi.Field(answer, "utcOffsetMinutes") != float64(480) {
			t.Fatalf("put in Shanghai: %d %v", code, answer)
		}
	}
	if lines := a.Count("SELECT count(*) FROM history WHERE table_name = 'ProjectTimeZones' AND action = 'Update'"); lines != 1 {
		t.Errorf("%d History lines, want 1: choosing the zone it is in already changes nothing", lines)
	}
}

// D3: only a zone of the list is taken, only by whoever manages the project, and not in a finished one.
func TestR4_107_OnlyAZoneOfTheListOnlyByItsManagerOnlyWhileOpen(t *testing.T) {
	a := newAPI(t)
	_, token := a.person("starter", false)
	_, stranger := a.person("stranger", false)
	project := "/projects/" + a.started(token, true)
	if code, answer := a.Ask("PUT", project+"/time-zone", `{"timeZone":"Mars/Olympus"}`, token); code != 400 || testapi.Field(answer, "code") != "unknownTimeZone" {
		t.Errorf("a zone not on the list: %d %v", code, answer)
	}
	if code, _ := a.Ask("PUT", project+"/time-zone", `{"timeZone":"Asia/Dubai"}`, stranger); code != 403 {
		t.Errorf("someone who does not manage it: %d", code)
	}
	a.Ask("POST", project+"/close", ``, token)
	if code, answer := a.Ask("PUT", project+"/time-zone", `{"timeZone":"Asia/Dubai"}`, token); code != 409 || testapi.Field(answer, "code") != "projectFinished" {
		t.Errorf("a finished project: %d %v", code, answer)
	}
}

// D3: the zones a project may take, Saudi Arabia first, each with the word of its city and its offset now.
func TestR4_107_TheZonesOfferedComeWithTheirCityAndOffset(t *testing.T) {
	a := newAPI(t)
	_, token := a.person("starter", false)
	code, answer := a.Ask("GET", "/time-zones", ``, token)
	offered, _ := answer.([]any)
	if code != 200 || len(offered) != 18 || testapi.Field(offered[0], "name") != "Asia/Riyadh" || testapi.Field(offered[0], "wordKey") != "timeZone.riyadh" ||
		testapi.Field(offered[0], "utcOffsetMinutes") != float64(180) {
		t.Fatalf("%d %v", code, answer)
	}
	for _, zone := range offered {
		key := testapi.Field(zone, "wordKey").(string)
		if a.Count("SELECT count(*) FROM translations JOIN translation_keys ON translation_keys.id = translations.translation_key_id WHERE key = ?", key) != 2 {
			t.Errorf("%s has no word in both languages", key)
		}
	}
}

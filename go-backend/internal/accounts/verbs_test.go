// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package accounts_test

import (
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
)

// accountNamed adds a bank account called name to the project at path and is its id, as the list answers it.
func accountNamed(t *testing.T, api *testapi.API, token, path, name string) string {
	t.Helper()
	if code, answer := api.Ask("POST", path+"/accounts", `{"name":"`+name+`","type":"bank"}`, token); code != 200 {
		t.Fatalf("adding %s: %d %v", name, code, answer)
	}
	_, listed := api.Ask("GET", path+"/accounts", ``, token)
	for _, account := range listed.([]any) {
		if testapi.Field(account, "name") == name {
			return testapi.Field(account, "id").(string)
		}
	}
	t.Fatalf("%s is not listed", name)
	return ""
}

// redix-api #98 (the owner, 12 Nov 2026): a plain edit is PUT on the thing's own route, and a POST to that route answers 405 and changes nothing.
func TestR98_AnEditIsPutAndAPostToItIsRefused(t *testing.T) {
	api := testapi.New(t)
	token, path := api.ApprovedProject()
	account := accountNamed(t, api, token, path, "حساب المؤسسة")
	if code, _ := api.Ask("POST", path+"/accounts/"+account, `{"name":"الحساب الجديد"}`, token); code != 405 {
		t.Errorf("a POST to the edit's route: %d, want 405", code)
	}
	if got := api.Count(`SELECT count(*) FROM accounts WHERE id = ? AND name = 'حساب المؤسسة'`, account); got != 1 {
		t.Errorf("the refused POST renamed the account")
	}
	if code, answer := api.Ask("PUT", path+"/accounts/"+account, `{"name":"الحساب الجديد"}`, token); code != 200 {
		t.Fatalf("the PUT: %d %v", code, answer)
	}
	if got := api.Count(`SELECT count(*) FROM accounts WHERE id = ? AND name = 'الحساب الجديد'`, account); got != 1 {
		t.Errorf("the PUT kept no new name")
	}
}

// redix-api #98: a delete is DELETE on the thing's own route; the old POST .../delete route is gone and deletes nothing.
func TestR98_ADeleteIsDeleteAndTheOldDeleteRouteIsGone(t *testing.T) {
	api := testapi.New(t)
	token, path := api.ApprovedProject()
	accountNamed(t, api, token, path, "حساب المؤسسة")
	spare := accountNamed(t, api, token, path, "احتياطي")
	if code, _ := api.Ask("POST", path+"/accounts/"+spare+"/delete", ``, token); code != 404 {
		t.Errorf("the old delete route: %d, want 404", code)
	}
	if got := api.Count(`SELECT count(*) FROM accounts WHERE id = ?`, spare); got != 1 {
		t.Fatalf("the old route deleted the account")
	}
	if code, answer := api.Ask("DELETE", path+"/accounts/"+spare, ``, token); code != 200 {
		t.Fatalf("the DELETE: %d %v", code, answer)
	}
	if got := api.Count(`SELECT count(*) FROM accounts WHERE id = ?`, spare); got != 0 {
		t.Errorf("the DELETE left the account")
	}
}

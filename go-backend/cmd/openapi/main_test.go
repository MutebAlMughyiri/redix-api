// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package main

import (
	"encoding/json"
	"fmt"
	"go/types"
	"os"
	"slices"
	"strings"
	"testing"
)

// operation is what the tests read of one route in the document.
type operation struct {
	Summary     string         `json:"summary"`
	Description string         `json:"description"`
	Security    []any          `json:"security"`
	RequestBody map[string]any `json:"requestBody"`
	Responses   map[string]struct {
		Description string                    `json:"description"`
		Content     map[string]map[string]any `json:"content"`
	} `json:"responses"`
}

type document struct {
	Paths      map[string]map[string]operation `json:"paths"`
	Components struct {
		Schemas map[string]struct {
			Properties map[string]map[string]any `json:"properties"`
			Required   []string                  `json:"required"`
		} `json:"schemas"`
	} `json:"components"`
}

func generated(t *testing.T) ([]byte, document) {
	t.Helper()
	text, err := Generate()
	if err != nil {
		t.Fatal(err)
	}
	var read document
	if err := json.Unmarshal(text, &read); err != nil {
		t.Fatal(err)
	}
	return text, read
}

func TestR1_R3_TheKeptDocumentIsWhatTheCodeSaysAndEveryRouteIsDescribed(t *testing.T) {
	text, read := generated(t)
	kept, err := os.ReadFile("../../internal/apidoc/openapi.json")
	if err != nil || strings.ReplaceAll(string(kept), "\r\n", "\n") != string(text) {
		t.Errorf("internal/apidoc/openapi.json is not what the code says (%v): run go generate ./internal/apidoc", err)
	}
	if len(read.Paths) < 100 {
		t.Errorf("the document holds %d paths", len(read.Paths))
	}
	for path, operations := range read.Paths {
		for method, route := range operations {
			if route.Summary == "" {
				t.Errorf("%s %s has no description: write a doc comment over its Handle call or on its handler", strings.ToUpper(method), path)
			}
		}
	}
}

func TestR1_ARouteSaysWhatItReadsAnswersRefusesAndNeeds(t *testing.T) {
	_, read := generated(t)
	add := read.Paths["/projects/{projectId}/accounts"]["post"]
	if !strings.Contains(add.Description, "needs the ManageSettings permission") || add.Security != nil {
		t.Errorf("adding an account says %q, security %v", add.Description, add.Security)
	}
	if add.RequestBody["content"].(map[string]any)["application/json"] == nil {
		t.Errorf("adding an account reads %v", add.RequestBody)
	}
	if done, answered := add.Responses["200"]; !answered || done.Content != nil {
		t.Errorf("adding an account answers %+v, not 200 with no body", add.Responses)
	}
	if refused := add.Responses["400"].Description; refused != "Refused: custodianRequired, invalidName, unknownCustodian." {
		t.Errorf("adding an account is refused with %q", refused)
	}
	if accounts := read.Paths["/projects/{projectId}/accounts"]["get"].Responses["200"].Content["application/json"]["schema"]; fmt.Sprint(accounts) !=
		"map[items:map[$ref:#/components/schemas/accounts.View] type:array]" {
		t.Errorf("the accounts answer %v", accounts)
	}
	signIn := read.Paths["/auth/sign-in"]["post"]
	if signIn.Security == nil || len(signIn.Security) != 0 || signIn.Responses["401"].Description != "Refused: accountLocked, invalidCredentials." {
		t.Errorf("signing in asks for %v, not nothing, and is refused with %q", signIn.Security, signIn.Responses["401"].Description)
	}
}

func TestR1_ARefusalIsToldUnderTheStatusItsCodeIsAnsweredWith(t *testing.T) {
	_, read := generated(t)
	register := read.Paths["/projects/register"]["post"]
	if register.Responses["409"].Description != "Refused: usernameTaken." {
		t.Errorf("registering is refused with 400 %q and 409 %q", register.Responses["400"].Description, register.Responses["409"].Description)
	}
	if !strings.Contains(register.Responses["400"].Description, "unitNameTaken") || register.Responses["201"].Content["application/json"] == nil {
		t.Errorf("registering answers %+v", register.Responses)
	}
	if people := read.Paths["/projects/{projectId}/users"]["post"].Responses["409"].Description; people != "Refused: emailTaken, usernameTaken." {
		t.Errorf("adding a person is refused with 409 %q", people)
	}
}

func TestR5_FieldsAreCamelCaseAndAnEnumIsItsNames(t *testing.T) {
	_, read := generated(t)
	account := read.Components.Schemas["accounts.View"]
	if !slices.Equal(anyStrings(account.Properties["type"]["enum"]), []string{"bank", "cashCustody"}) || account.Properties["isActive"] == nil ||
		!slices.Contains(account.Required, "name") {
		t.Errorf("an account reads %+v", account)
	}
	if amount := read.Components.Schemas["entries.Request"].Properties["amount"]; amount["type"] != "number" {
		t.Errorf("an entry's amount, a fixed.Typed written by fixed.Number's MarshalJSON, reads %v", amount)
	}
}

func anyStrings(values any) []string {
	var texts []string
	list, _ := values.([]any)
	for _, value := range list {
		texts = append(texts, value.(string))
	}
	return texts
}

func TestR1_GoGenerateWritesTheDocumentWhereItRuns(t *testing.T) {
	text, _ := generated(t)
	t.Cleanup(func() { _ = os.Remove("openapi.json") })
	main()
	if written, err := os.ReadFile("openapi.json"); err != nil || string(written) != string(text) {
		t.Errorf("main wrote %d bytes (%v), Generate %d", len(written), err, len(text))
	}
}

func TestR1_ARouteWhoseCodeAnswersNothingFoundIsDocumentedAs200(t *testing.T) {
	responses := (&route{answers: map[int]types.Type{}, refusals: map[int][]string{}}).responses(newGenerator(nil))
	if fmt.Sprint(responses["200"]) != "map[description:OK.]" {
		t.Errorf("such a route answers %v", responses)
	}
}

func TestR5_EveryKindOfGoValueIsWrittenAsEncodingJsonWritesIt(t *testing.T) {
	g := newGenerator(nil)
	names := types.NewPackage("example.com/names", "names")
	hidden := types.NewStruct([]*types.Var{types.NewField(0, names, "Secret", types.Typ[types.String], false), types.NewField(0, names, "Name", types.Typ[types.String], false)},
		[]string{`json:"-"`, ""})
	for _, kind := range []struct {
		value types.Type
		want  string
	}{
		{types.Typ[types.Float64], "map[type:number]"},
		{types.Typ[types.Bool], "map[type:boolean]"},
		{types.NewArray(types.Typ[types.Int64], 3), "map[items:map[type:integer] type:array]"},
		{types.NewSlice(types.Typ[types.Byte]), "map[format:byte type:string]"},
		{types.NewMap(types.Typ[types.String], types.Typ[types.String]), "map[additionalProperties:map[type:string] type:object]"},
		{types.NewInterfaceType(nil, nil), "map[]"},
		{types.NewPointer(types.Typ[types.String]), "map[type:string]"},
		{types.NewChan(types.SendRecv, types.Typ[types.Int]), "map[]"},
		{types.NewNamed(types.NewTypeName(0, names, "List", nil), types.NewSlice(types.Typ[types.String]), nil), "map[items:map[type:string] type:array]"},
		{hidden, "map[properties:map[Name:map[type:string]] required:[Name] type:object]"},
	} {
		if got := fmt.Sprint(g.schema(kind.value)); got != kind.want {
			t.Errorf("%s is written %s, want %s", kind.value, got, kind.want)
		}
	}
}

// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Command openapi writes internal/apidoc/openapi.json, the OpenAPI document of every route, read from the code by Go's own type checker
// (redix-api #142 R1): run by go generate in internal/apidoc, and checked against the code by this package's test.
package main

import (
	"fmt"
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"log"
	"maps"
	"os"
	"reflect"
	"regexp"
	"slices"
	"sort"
	"strconv"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/encode"
	"golang.org/x/tools/go/packages"
	"golang.org/x/tools/go/types/typeutil"
)

const module = "github.com/MutebAlMughyiri/redix-api/go-backend"

func main() {
	document, err := Generate()
	if err != nil {
		log.Fatal(err)
	}
	if err := os.WriteFile("openapi.json", document, 0o644); err != nil {
		log.Fatal(err)
	}
}

// Generate is the OpenAPI document of every route the API's packages register with web.Router.Handle: the method and path the call names; the
// permission of the gate around its handler; the doc comment over the call, or else the handler's, as its description; and from the handler's
// code and the helpers it calls, what web.Read reads, what web.JSON answers with each status, and the codes problem.Refuse and changes.Refuse
// refuse with.
func Generate() ([]byte, error) {
	loaded, err := packages.Load(&packages.Config{Mode: packages.NeedName | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo}, module+"/internal/...")
	if err != nil {
		return nil, err
	}
	if packages.PrintErrors(loaded) > 0 {
		return nil, fmt.Errorf("the packages did not load")
	}
	g := newGenerator(loaded)
	routes := g.routes()
	if len(routes) == 0 {
		return nil, fmt.Errorf("no route found: run it inside the module, as go generate ./internal/apidoc does")
	}
	paths := map[string]map[string]any{}
	for _, route := range routes {
		path := route.openPath()
		if paths[path] == nil {
			paths[path] = map[string]any{}
		}
		paths[path][strings.ToLower(route.method)] = route.operation(g)
	}
	document := map[string]any{
		"openapi": "3.1.0",
		"info": map[string]any{"title": "REDIX API", "version": "1", "description": "Every route of the REDIX API, generated from its Go code by cmd/openapi " +
			"(redix-api #142). Every route answers under /api/v1 too. Every failure answers problem details with a code and a trace id (redix-api #150)."},
		"security": []any{map[string]any{"bearer": []any{}}},
		"paths":    paths,
		"components": map[string]any{
			"securitySchemes": map[string]any{"bearer": map[string]any{"type": "http", "scheme": "bearer", "bearerFormat": "JWT"}},
			"schemas":         g.schemas,
		},
	}
	g.schemas["problem"] = map[string]any{"type": "object", "required": []string{"type", "title", "status", "code", "traceId"}, "properties": map[string]any{
		"type": map[string]any{"type": "string"}, "title": map[string]any{"type": "string"}, "status": map[string]any{"type": "integer"},
		"code": map[string]any{"type": "string"}, "traceId": map[string]any{"type": "string"}}}
	text, err := encode.Indented(document, "  ")
	return append(text, '\n'), err
}

// source is a function's declaration and the package it is in.
type source struct {
	declaration *ast.FuncDecl
	pkg         *packages.Package
}

type generator struct {
	packages  []*packages.Package
	functions map[string]source
	literals  map[*types.Var]*ast.FuncLit
	values    map[*types.Var]ast.Expr
	schemas   map[string]any
}

func newGenerator(loaded []*packages.Package) *generator {
	g := &generator{packages: loaded, functions: map[string]source{}, literals: map[*types.Var]*ast.FuncLit{}, values: map[*types.Var]ast.Expr{}, schemas: map[string]any{}}
	for _, p := range loaded {
		for _, file := range p.Syntax {
			ast.Inspect(file, func(node ast.Node) bool {
				switch n := node.(type) {
				case *ast.FuncDecl:
					g.functions[p.TypesInfo.Defs[n.Name].(*types.Func).FullName()] = source{n, p}
				case *ast.AssignStmt:
					for i, left := range n.Lhs {
						name, isName := left.(*ast.Ident)
						literal, isLiteral := n.Rhs[min(i, len(n.Rhs)-1)].(*ast.FuncLit)
						if variable, isVariable := p.TypesInfo.ObjectOf(name).(*types.Var); isName && isLiteral && isVariable {
							g.literals[variable] = literal
						}
					}
				case *ast.ValueSpec:
					for i, name := range n.Names {
						if variable, isVariable := p.TypesInfo.Defs[name].(*types.Var); isVariable && i < len(n.Values) {
							g.values[variable] = n.Values[i]
						}
					}
				}
				return true
			})
		}
	}
	return g
}

// route is one Handle call, and what its handler's code was found to read, answer and refuse.
type route struct {
	tag, method, path, description, access string
	request                                types.Type
	answers                                map[int]types.Type
	contentTypes, query, form, docs        []string
	refusals                               map[int][]string
}

func (g *generator) routes() []*route {
	var routes []*route
	for _, p := range g.packages {
		for _, file := range p.Syntax {
			comments := ast.NewCommentMap(p.Fset, file, file.Comments)
			ast.Inspect(file, func(node ast.Node) bool {
				statement, isStatement := node.(*ast.ExprStmt)
				if !isStatement {
					return true
				}
				call, isCall := statement.X.(*ast.CallExpr)
				if !isCall || nameOf(typeutil.StaticCallee(p.TypesInfo, call)) != "web.Router.Handle" {
					return true
				}
				arguments := call.Args
				r := &route{tag: p.Name, method: textOf(p, arguments[0]), path: textOf(p, arguments[1]), answers: map[int]types.Type{}, refusals: map[int][]string{}}
				for _, group := range comments[statement] {
					r.description += group.Text()
				}
				g.handler(arguments[2], p, r, map[string]bool{})
				if r.description == "" && len(r.docs) > 0 {
					r.description = r.docs[len(r.docs)-1]
				}
				routes = append(routes, r)
				return false
			})
		}
	}
	sort.Slice(routes, func(i, j int) bool { return routes[i].path+" "+routes[i].method < routes[j].path+" "+routes[j].method })
	return routes
}

// textOf is a constant string expression's value: a literal, a constant, or constants added together.
func textOf(p *packages.Package, expression ast.Expr) string {
	value := p.TypesInfo.Types[expression].Value
	if value == nil || value.Kind() != constant.String {
		log.Fatalf("%s: a route's method and path are constants, so the document can name them", p.Fset.Position(expression.Pos()))
	}
	return constant.StringVal(value)
}

// nameOf is a function as package.Function or package.Type.Method: "web.JSON", "web.Router.Handle", "http.ResponseWriter.WriteHeader".
func nameOf(function *types.Func) string {
	if function == nil || function.Pkg() == nil {
		return ""
	}
	name := function.Pkg().Name() + "."
	if receiver := function.Signature().Recv(); receiver != nil {
		if named, isNamed := types.Unalias(dereference(receiver.Type())).(*types.Named); isNamed {
			name += named.Obj().Name() + "."
		}
	}
	return name + function.Name()
}

func dereference(t types.Type) types.Type {
	if pointer, isPointer := types.Unalias(t).(*types.Pointer); isPointer {
		return pointer.Elem()
	}
	return t
}

// platform are the packages whose code a route runs but does not describe: the gate, the router, problem details, the database.
var platform = map[string]bool{"access": true, "web": true, "problem": true, "store": true, "db": true}

// handler reads the expression a route is registered with: the gate around it says the access it needs, and every function of the API's own it
// names - the handler, a local wrapper such as keeps, a method that makes the handler - is read for what it does. The innermost function with a
// doc comment, the handler itself rather than a limit around it, describes the route when the call has none over it.
func (g *generator) handler(expression ast.Node, p *packages.Package, r *route, reading map[string]bool) {
	ast.Inspect(expression, func(node ast.Node) bool {
		switch n := node.(type) {
		case *ast.FuncLit:
			g.body(n.Body, p, r, map[*types.Var]bound{}, reading)
			return false
		case *ast.CallExpr:
			switch called := nameOf(typeutil.StaticCallee(p.TypesInfo, n)); called {
			case "access.Gate.Needs", "access.Gate.Changes":
				r.access = "needs the " + constant.StringVal(p.TypesInfo.Types[n.Args[0]].Value) + " permission"
			case "access.Gate.SignedIn":
				r.access = "any session, one that must still change its password too"
			case "access.Gate.Working":
				r.access = "a session whose password is changed"
			}
			if variable, isVariable := typeutil.Callee(p.TypesInfo, n).(*types.Var); isVariable && g.literals[variable] != nil {
				g.handler(g.literals[variable].Body, p, r, reading)
			}
		case *ast.Ident:
			if function, isFunction := p.TypesInfo.Uses[n].(*types.Func); isFunction && function.Pkg() != nil && !platform[function.Pkg().Name()] {
				if code, declared := g.functions[function.FullName()]; declared && !reading[function.FullName()] {
					r.docs = appendDoc(r.docs, code.declaration.Doc.Text())
					reading[function.FullName()] = true
					g.body(code.declaration.Body, code.pkg, r, map[*types.Var]bound{}, reading)
				}
			}
		}
		return true
	})
}

func appendDoc(docs []string, doc string) []string {
	if doc == "" {
		return docs
	}
	return append(docs, doc)
}

// bound is what a helper's parameter was given by the call that is being read: the type of a value passed as any, a constant passed as a string,
// the constants a variable passed as a string can be, read where it was assigned - a refusal handed to a helper that answers it (redix-api #161) -
// and a function literal passed as a function, read in the package it is written in.
type bound struct {
	t       types.Type
	value   constant.Value
	codes   []constant.Value
	literal *ast.FuncLit
	in      *packages.Package
}

// body reads a function's code for what the route reads, answers and refuses, and the API's own functions it calls, four calls deep, with what
// each was given: s.answer(w, r, views, err) answers the type of views.
func (g *generator) body(body *ast.BlockStmt, p *packages.Package, r *route, bindings map[*types.Var]bound, reading map[string]bool) {
	given := func(expression ast.Expr) bound {
		if ident, isIdent := ast.Unparen(expression).(*ast.Ident); isIdent {
			if variable, isVariable := p.TypesInfo.Uses[ident].(*types.Var); isVariable {
				if known, isBound := bindings[variable]; isBound {
					return known
				}
			}
		}
		literal, _ := ast.Unparen(expression).(*ast.FuncLit)
		found := bound{t: p.TypesInfo.TypeOf(expression), value: p.TypesInfo.Types[expression].Value, literal: literal, in: p}
		if text, isBasic := found.t.(*types.Basic); found.value == nil && isBasic && text.Info()&types.IsString != 0 {
			found.codes = g.assigned(body, p, bindings, expression, 0)
		}
		return found
	}
	clauses := clausesIn(body, p.TypesInfo)
	handedOn := handedOnIn(body, p, g.functions)
	refuse := func(call *ast.CallExpr, status bound, statusExpression, expression ast.Expr) {
		if from, statusAt, codeAt := sameCall(body, p, statusExpression, expression); status.value == nil && from != nil {
			for _, pair := range g.paired(p, from, statusAt, codeAt, 0) {
				r.refuse(bound{value: constant.MakeInt64(int64(pair.status))}, pair.codes)
			}
			return
		}
		code := given(expression)
		if code.value != nil {
			r.refuse(status, []constant.Value{code.value})
			return
		}
		codes := code.codes
		if clause, inSwitch := clauses[call]; inSwitch && clause.variable == p.TypesInfo.Uses[identOf(expression)] {
			if codes = clause.of(codes); len(codes) == 0 {
				return
			}
		}
		r.refuse(status, codes)
	}
	ast.Inspect(body, func(node ast.Node) bool {
		if notFound(node, p.TypesInfo) {
			r.refuse(bound{value: constant.MakeInt64(404)}, nil)
		}
		call, isCall := node.(*ast.CallExpr)
		if !isCall {
			return true
		}
		arguments := call.Args
		function, _ := typeutil.Callee(p.TypesInfo, call).(*types.Func)
		switch nameOf(function) {
		case "web.Read":
			r.request = dereference(given(arguments[2]).t)
		case "web.JSON":
			if status, answer := statusOf(given(arguments[2])), given(arguments[3]).t; r.answers[status] == nil || types.IsInterface(r.answers[status]) {
				r.answers[status] = answer
			}
		case "http.ResponseWriter.WriteHeader":
			if status := statusOf(given(arguments[0])); status > 0 {
				if _, known := r.answers[status]; !known {
					r.answers[status] = nil
				}
			}
		case "http.Header.Set":
			if name := given(arguments[0]).value; name != nil && constant.StringVal(name) == "Content-Type" {
				contentType := "application/octet-stream"
				if value := given(arguments[1]).value; value != nil {
					contentType = strings.Split(constant.StringVal(value), ";")[0]
				}
				r.contentTypes = appendText(r.contentTypes, constant.MakeString(contentType))
			}
		case "url.Values.Get":
			r.query = appendText(r.query, given(arguments[0]).value)
		case "http.Request.FormValue", "http.Request.FormFile":
			r.form = appendText(r.form, given(arguments[0]).value)
		case "problem.Status":
			r.refuse(given(arguments[2]), nil)
		case "problem.Refuse", "problem.RefuseOver":
			refuse(call, given(arguments[2]), arguments[2], arguments[3])
		case "changes.Refuse", "changes.RefuseOver":
			refuse(call, given(arguments[0]), arguments[0], arguments[1])
		case "changes.Unknown":
			refuse(call, bound{value: constant.MakeInt64(400)}, nil, arguments[1])
		default:
			if handedOn[call] {
				return false
			}
			if function == nil || function.Pkg() == nil || platform[function.Pkg().Name()] || reading[function.FullName()] || len(reading) > 5 {
				return true
			}
			if code, declared := g.functions[function.FullName()]; declared && !gathers(code.declaration) {
				calledBindings := map[*types.Var]bound{}
				place := 0
				for _, field := range code.declaration.Type.Params.List {
					for _, name := range field.Names {
						if parameter, isVariable := code.pkg.TypesInfo.Defs[name].(*types.Var); isVariable && place < len(arguments) {
							calledBindings[parameter] = given(arguments[place])
						}
						place++
					}
				}
				reading[function.FullName()] = true
				g.body(code.declaration.Body, code.pkg, r, calledBindings, reading)
				delete(reading, function.FullName())
			}
		}
		return true
	})
}

func statusOf(given bound) int {
	if given.value == nil {
		return 0
	}
	status, _ := constant.Int64Val(given.value)
	return int(status)
}

func appendText(texts []string, value constant.Value) []string {
	if value == nil || value.Kind() != constant.String || constant.StringVal(value) == "" {
		return texts
	}
	for _, known := range texts {
		if known == constant.StringVal(value) {
			return texts
		}
	}
	return append(texts, constant.StringVal(value))
}

// refuse notes a refusal the code answers with a constant status from 400 to 499, with its codes; a status passed on is answered by whoever chose
// it, and every failure is in the default answer.
func (r *route) refuse(status bound, codes []constant.Value) {
	at := statusOf(status)
	if at < 400 || at >= 500 {
		return
	}
	r.refusals[at] = appendText(r.refusals[at], nil)
	for _, code := range codes {
		r.refusals[at] = appendText(r.refusals[at], code)
	}
}

// assigned is the constants a function assigns to the variable an expression names - refusal = "custodianRequired" - or that what it was
// assigned from returns in its place: refusal, left := attempt(...) takes each code attempt returns first, and refusal = decide(...) each code the
// function literal given as decide returns. It follows returned variables and calls six functions deep.
func (g *generator) assigned(body *ast.BlockStmt, p *packages.Package, bindings map[*types.Var]bound, expression ast.Expr, depth int) []constant.Value {
	variable := p.TypesInfo.Uses[identOf(expression)]
	if variable == nil || depth > 6 {
		return nil
	}
	var values []constant.Value
	ast.Inspect(body, func(node ast.Node) bool {
		assignment, isAssignment := node.(*ast.AssignStmt)
		if !isAssignment {
			return true
		}
		for i, left := range assignment.Lhs {
			if name, isName := left.(*ast.Ident); !isName || p.TypesInfo.ObjectOf(name) != variable {
				continue
			}
			from, place := assignment.Rhs[min(i, len(assignment.Rhs)-1)], max(0, i-len(assignment.Rhs)+1)
			values = append(values, g.valuesOf(body, p, bindings, from, place, depth)...)
		}
		return true
	})
	return values
}

// valuesOf is the constants an expression can be: a constant, what a variable is assigned, what a call returns at place.
func (g *generator) valuesOf(body *ast.BlockStmt, p *packages.Package, bindings map[*types.Var]bound, expression ast.Expr, place, depth int) []constant.Value {
	if value := p.TypesInfo.Types[expression].Value; value != nil {
		return []constant.Value{value}
	}
	if call, isCall := ast.Unparen(expression).(*ast.CallExpr); isCall {
		return g.returned(p, bindings, call, place, depth+1)
	}
	return g.assigned(body, p, bindings, expression, depth+1)
}

// pair is a status a function answers a refusal with, and the codes it answers with it.
type pair struct {
	status int
	codes  []constant.Value
}

// paired is each status a call returns at statusAt with the codes it returns beside it at codeAt: a constant status with the codes its code can
// be, or both passed on from one call, followed six calls deep - so a refusal whose status was chosen further down is told with that status.
func (g *generator) paired(p *packages.Package, call *ast.CallExpr, statusAt, codeAt, depth int) []pair {
	code, declared := g.functions[fullName(typeutil.StaticCallee(p.TypesInfo, call))]
	if !declared || depth > 6 {
		return nil
	}
	body, in := code.declaration.Body, code.pkg
	var pairs []pair
	ast.Inspect(body, func(node ast.Node) bool {
		if _, isLiteral := node.(*ast.FuncLit); isLiteral {
			return false
		}
		returned, isReturn := node.(*ast.ReturnStmt)
		if !isReturn || len(returned.Results) <= max(statusAt, codeAt) {
			return true
		}
		status, codes := returned.Results[statusAt], returned.Results[codeAt]
		if value := in.TypesInfo.Types[status].Value; value != nil {
			at, _ := constant.Int64Val(value)
			pairs = append(pairs, pair{status: int(at), codes: g.valuesOf(body, in, nil, codes, 0, depth)})
		} else if from, fromStatus, fromCode := sameCall(body, in, status, codes); from != nil {
			pairs = append(pairs, g.paired(in, from, fromStatus, fromCode, depth+1)...)
		}
		return true
	})
	return pairs
}

// sameCall is the call two variables are assigned from in one statement - status, refusal := decide(...) - with their places among its results.
func sameCall(body *ast.BlockStmt, p *packages.Package, first, second ast.Expr) (*ast.CallExpr, int, int) {
	a, b := p.TypesInfo.ObjectOf(identOf(first)), p.TypesInfo.ObjectOf(identOf(second))
	if identOf(first) == nil || identOf(second) == nil || a == nil || b == nil {
		return nil, 0, 0
	}
	var found *ast.CallExpr
	firstAt, secondAt := -1, -1
	ast.Inspect(body, func(node ast.Node) bool {
		assignment, isAssignment := node.(*ast.AssignStmt)
		if found != nil || !isAssignment || len(assignment.Rhs) != 1 {
			return found == nil
		}
		call, isCall := ast.Unparen(assignment.Rhs[0]).(*ast.CallExpr)
		if !isCall {
			return true
		}
		firstAt, secondAt = -1, -1
		for i, left := range assignment.Lhs {
			if name, isName := left.(*ast.Ident); isName {
				switch p.TypesInfo.ObjectOf(name) {
				case a:
					firstAt = i
				case b:
					secondAt = i
				}
			}
		}
		if firstAt >= 0 && secondAt >= 0 {
			found = call
		}
		return found == nil
	})
	return found, firstAt, secondAt
}

// returned is the constants a call returns as its result at place: a function of the API's own, or a function literal a parameter was given.
func (g *generator) returned(p *packages.Package, bindings map[*types.Var]bound, call *ast.CallExpr, place, depth int) []constant.Value {
	body, in := (*ast.BlockStmt)(nil), p
	if code, declared := g.functions[fullName(typeutil.StaticCallee(p.TypesInfo, call))]; declared {
		body, in = code.declaration.Body, code.pkg
	} else if parameter, isVariable := typeutil.Callee(p.TypesInfo, call).(*types.Var); isVariable && bindings[parameter].literal != nil {
		body, in = bindings[parameter].literal.Body, bindings[parameter].in
	}
	if body == nil || depth > 6 {
		return nil
	}
	var values []constant.Value
	ast.Inspect(body, func(node ast.Node) bool {
		if _, isLiteral := node.(*ast.FuncLit); isLiteral {
			return false
		}
		returned, isReturn := node.(*ast.ReturnStmt)
		switch {
		case !isReturn:
		case place < len(returned.Results) && len(returned.Results) > 1 || place == 0 && len(returned.Results) == 1:
			values = append(values, g.valuesOf(body, in, nil, returned.Results[place], 0, depth)...)
		case len(returned.Results) == 1:
			values = append(values, g.valuesOf(body, in, nil, returned.Results[0], place, depth)...)
		}
		return true
	})
	return values
}

func fullName(function *types.Func) string {
	if function == nil {
		return ""
	}
	return function.FullName()
}

func identOf(expression ast.Expr) *ast.Ident {
	ident, _ := ast.Unparen(expression).(*ast.Ident)
	return ident
}

// clause is the case of a switch on a variable that a call sits in: case invalidName, invalidDescription takes those of the codes the variable can
// hold; default every other one. A route whose variable can hold none of a case's codes never answers that case.
type clause struct {
	variable     types.Object
	names, other []constant.Value
	isDefault    bool
}

// of is the codes this case takes of every code the variable can hold.
func (c clause) of(codes []constant.Value) []constant.Value {
	var taken []constant.Value
	for _, code := range codes {
		named := slices.ContainsFunc(c.names, func(name constant.Value) bool { return constant.Compare(name, token.EQL, code) })
		other := slices.ContainsFunc(c.other, func(name constant.Value) bool { return constant.Compare(name, token.EQL, code) })
		if named || c.isDefault && !other {
			taken = append(taken, code)
		}
	}
	return taken
}

// clausesIn is, for every call inside a case of a switch on a variable, that case.
func clausesIn(body *ast.BlockStmt, info *types.Info) map[*ast.CallExpr]clause {
	clauses := map[*ast.CallExpr]clause{}
	ast.Inspect(body, func(node ast.Node) bool {
		switchStatement, isSwitch := node.(*ast.SwitchStmt)
		if !isSwitch || identOf(switchStatement.Tag) == nil || info.Uses[identOf(switchStatement.Tag)] == nil {
			return true
		}
		var every []constant.Value
		for _, statement := range switchStatement.Body.List {
			for _, name := range statement.(*ast.CaseClause).List {
				every = appendValue(every, info.Types[name].Value)
			}
		}
		for _, statement := range switchStatement.Body.List {
			c := clause{variable: info.Uses[identOf(switchStatement.Tag)], other: every, isDefault: statement.(*ast.CaseClause).List == nil}
			for _, name := range statement.(*ast.CaseClause).List {
				c.names = appendValue(c.names, info.Types[name].Value)
			}
			ast.Inspect(statement, func(inner ast.Node) bool {
				if call, isCall := inner.(*ast.CallExpr); isCall {
					clauses[call] = c
				}
				return true
			})
		}
		return true
	})
	return clauses
}

func appendValue(values []constant.Value, value constant.Value) []constant.Value {
	if value == nil {
		return values
	}
	return append(values, value)
}

func (r *route) operation(g *generator) map[string]any {
	summary, description := firstSentence(r.description), strings.Join(strings.Fields(r.description), " ")
	if r.access != "" {
		description += "\n\nAccess: " + r.access + "."
	}
	operation := map[string]any{"tags": []string{r.tag}, "summary": summary, "description": description, "responses": r.responses(g)}
	if r.access == "" {
		operation["security"] = []any{}
	}
	var parameters []any
	for _, value := range routeValue.FindAllStringSubmatch(r.path, -1) {
		schema := map[string]any{"type": "string"}
		switch value[2] {
		case "guid":
			schema["format"] = "uuid"
		case "int":
			schema = map[string]any{"type": "integer"}
		}
		parameters = append(parameters, map[string]any{"name": value[1], "in": "path", "required": true, "schema": schema})
	}
	for _, name := range r.query {
		parameters = append(parameters, map[string]any{"name": name, "in": "query", "schema": map[string]any{"type": "string"}})
	}
	if len(parameters) > 0 {
		operation["parameters"] = parameters
	}
	switch {
	case r.request != nil:
		operation["requestBody"] = map[string]any{"required": true, "content": map[string]any{"application/json": map[string]any{"schema": g.schema(r.request)}}}
	case len(r.form) > 0:
		fields := map[string]any{}
		for _, name := range r.form {
			fields[name] = map[string]any{"type": "string"}
		}
		if _, file := fields["file"]; file {
			fields["file"] = map[string]any{"type": "string", "format": "binary"}
		}
		operation["requestBody"] = map[string]any{"required": true, "content": map[string]any{"multipart/form-data": map[string]any{"schema": map[string]any{"type": "object", "properties": fields}}}}
	}
	return operation
}

// path is the route's path as OpenAPI writes it, its values without their constraints: "/projects/{projectId}".
func (r *route) openPath() string { return routeValue.ReplaceAllString(r.path, "{$1}") }

var routeValue = regexp.MustCompile(`\{(\w+)(?::(guid|int))?}`)

func (r *route) responses(g *generator) map[string]any {
	responses := map[string]any{}
	for status, answer := range r.answers {
		response := map[string]any{"description": statusText(status)}
		if answer != nil {
			response["content"] = map[string]any{"application/json": map[string]any{"schema": g.schema(answer)}}
		}
		responses[strconv.Itoa(status)] = response
	}
	if _, answered := r.answers[200]; len(r.contentTypes) > 0 && !answered {
		content := map[string]any{}
		for _, contentType := range r.contentTypes {
			schema := map[string]any{"type": "string", "format": "binary"}
			switch {
			case strings.HasPrefix(contentType, "text/"):
				schema = map[string]any{"type": "string"}
			case contentType == "application/json":
				schema = map[string]any{}
			}
			content[contentType] = map[string]any{"schema": schema}
		}
		responses["200"] = map[string]any{"description": statusText(200), "content": content}
	}
	if len(responses) == 0 {
		responses["200"] = map[string]any{"description": statusText(200)}
	}
	for status, codes := range r.refusals {
		sort.Strings(codes)
		description := "Refused, with problem details."
		if len(codes) > 0 {
			description = "Refused: " + strings.Join(codes, ", ") + "."
		}
		responses[strconv.Itoa(status)] = map[string]any{"description": description, "content": problemContent}
	}
	responses["default"] = map[string]any{"description": "Problem details, with the failure's code and trace id (redix-api #150).", "content": problemContent}
	return responses
}

var problemContent = map[string]any{"application/problem+json": map[string]any{"schema": map[string]any{"$ref": "#/components/schemas/problem"}}}

func statusText(status int) string {
	switch status {
	case 200:
		return "OK."
	case 201:
		return "Created."
	case 202:
		return "Accepted; the work goes on after the answer."
	case 204:
		return "Done; nothing to answer."
	}
	return strconv.Itoa(status) + "."
}

// firstSentence is a doc comment's first sentence, which says what the route does.
func firstSentence(doc string) string {
	doc = strings.Join(strings.Fields(doc), " ")
	if end := strings.Index(doc, ". "); end >= 0 {
		return doc[:end+1]
	}
	return doc
}

// special are the types whose JSON is not their fields.
var special = map[string]map[string]any{
	"uuid.UUID":       {"type": "string", "format": "uuid"},
	"fixed.Number":    {"type": "number", "description": "A figure kept to four decimals."},
	"fixed.Total":     {"type": "number", "description": "A sum kept to four decimals."},
	"fixed.Written":   {"type": "number", "description": "A figure as it was written."},
	"moment.Moment":   {"type": "string", "format": "date-time"},
	"moment.Date":     {"type": "string", "format": "date"},
	"time.Time":       {"type": "string", "format": "date-time"},
	"json.RawMessage": {},
}

// schema is a type's JSON Schema, as encoding/json writes it.
func (g *generator) schema(t types.Type) map[string]any {
	switch t := types.Unalias(t).(type) {
	case *types.Pointer:
		return g.schema(t.Elem())
	case *types.Named:
		return g.named(t)
	case *types.Basic:
		switch {
		case t.Info()&types.IsString != 0:
			return map[string]any{"type": "string"}
		case t.Info()&types.IsBoolean != 0:
			return map[string]any{"type": "boolean"}
		case t.Info()&types.IsInteger != 0:
			return map[string]any{"type": "integer"}
		case t.Info()&types.IsFloat != 0:
			return map[string]any{"type": "number"}
		}
	case *types.Slice:
		if basic, isBasic := t.Elem().(*types.Basic); isBasic && basic.Kind() == types.Byte {
			return map[string]any{"type": "string", "format": "byte"}
		}
		return map[string]any{"type": "array", "items": g.schema(t.Elem())}
	case *types.Array:
		return map[string]any{"type": "array", "items": g.schema(t.Elem())}
	case *types.Map:
		return map[string]any{"type": "object", "additionalProperties": g.schema(t.Elem())}
	case *types.Struct:
		return g.object(t)
	}
	return map[string]any{}
}

// named is a named type's schema: one that writes itself with the MarshalJSON of a special type, such as fixed.Typed with fixed.Number's, what
// that type writes; an enum, which writes itself by its name, a string of its names; a struct a component of its own, named as Go names it;
// anything else what it is made of.
func (g *generator) named(t *types.Named) map[string]any {
	name := types.TypeString(t, func(p *types.Package) string { return p.Name() })
	if known, isSpecial := special[name]; isSpecial {
		return known
	}
	if method, _, _ := types.LookupFieldOrMethod(t, true, t.Obj().Pkg(), "MarshalJSON"); method != nil {
		if owner, isSpecial := special[strings.TrimSuffix(nameOf(method.(*types.Func)), ".MarshalJSON")]; isSpecial {
			return owner
		}
		schema := map[string]any{"type": "string"}
		if names := g.enumOf(method.(*types.Func)); len(names) > 0 {
			schema["enum"] = names
		}
		return schema
	}
	structure, isStruct := t.Underlying().(*types.Struct)
	if !isStruct {
		return g.schema(t.Underlying())
	}
	key := componentName.ReplaceAllString(name, "_")
	if _, made := g.schemas[key]; !made {
		g.schemas[key] = map[string]any{}
		g.schemas[key] = g.object(structure)
	}
	return map[string]any{"$ref": "#/components/schemas/" + key}
}

var componentName = regexp.MustCompile(`[^A-Za-z0-9._-]+`)

// enumOf is the names an enum's MarshalJSON writes it by: the web.Enum it writes with, such as accountTypes.
func (g *generator) enumOf(marshal *types.Func) []string {
	code, declared := g.functions[marshal.FullName()]
	if !declared {
		return nil
	}
	var names []string
	ast.Inspect(code.declaration.Body, func(node ast.Node) bool {
		ident, isIdent := node.(*ast.Ident)
		if !isIdent || names != nil {
			return names == nil
		}
		variable, isVariable := code.pkg.TypesInfo.Uses[ident].(*types.Var)
		if literal, isLiteral := g.values[variable].(*ast.CompositeLit); isVariable && isLiteral {
			for _, element := range literal.Elts {
				if value := code.pkg.TypesInfo.Types[element].Value; value != nil && value.Kind() == constant.String {
					names = append(names, constant.StringVal(value))
				}
			}
		}
		return true
	})
	return names
}

// object is a struct as encoding/json writes it: each exported field by its json name, required unless omitempty or omitzero, as web.Read
// requires it; an embedded struct's fields as its own.
func (g *generator) object(structure *types.Struct) map[string]any {
	properties := map[string]any{}
	var required []string
	for i := range structure.NumFields() {
		field := structure.Field(i)
		name, options, _ := strings.Cut(reflect.StructTag(structure.Tag(i)).Get("json"), ",")
		if name == "-" {
			continue
		}
		if inner, isStruct := dereference(field.Type()).Underlying().(*types.Struct); field.Embedded() && name == "" && isStruct {
			embedded := g.object(inner)
			maps.Copy(properties, embedded["properties"].(map[string]any))
			required = append(required, embedded["required"].([]string)...)
			continue
		}
		if !field.Exported() {
			continue
		}
		if name == "" {
			name = field.Name()
		}
		properties[name] = g.schema(field.Type())
		if !strings.Contains(options, "omitempty") && !strings.Contains(options, "omitzero") {
			required = append(required, name)
		}
	}
	sort.Strings(required)
	return map[string]any{"type": "object", "properties": properties, "required": required}
}

// handedOnIn is every call of body handed as an argument to a function of our own: what it answers is that function's to judge - codeOf turns
// an owner's refusal into a row of a project file's rowsRefused -, so the route does not answer its refusals itself and they are not read as
// the route's (findings row 292).
func handedOnIn(body *ast.BlockStmt, p *packages.Package, functions map[string]source) map[*ast.CallExpr]bool {
	handed := map[*ast.CallExpr]bool{}
	ast.Inspect(body, func(node ast.Node) bool {
		outer, isCall := node.(*ast.CallExpr)
		if !isCall {
			return true
		}
		function, _ := typeutil.Callee(p.TypesInfo, outer).(*types.Func)
		if function == nil {
			return true
		}
		if _, ours := functions[function.FullName()]; !ours {
			return true
		}
		for _, argument := range outer.Args {
			if inner, isCall := ast.Unparen(argument).(*ast.CallExpr); isCall {
				handed[inner] = true
			}
		}
		return true
	})
	return handed
}

// gathers says whether declaration is marked //apidoc:gathers: it collects the refusals it meets into rows its caller answers together - a project
// file's load answers every row's under rowsRefused -, so none of them is read as the route's own answer (findings row 292).
func gathers(declaration *ast.FuncDecl) bool {
	return declaration.Doc != nil && slices.ContainsFunc(declaration.Doc.List, func(line *ast.Comment) bool { return line.Text == "//apidoc:gathers" })
}

// notFound says whether node names the refusal of a row not found - changes.NotFound, or changes.Found, which answers it - so the route it is
// read in answers 404 (redix-api #173: a rule lifted out of a route keeps the 404 the route wrote itself).
func notFound(node ast.Node, info *types.Info) bool {
	ident, isIdent := node.(*ast.Ident)
	if !isIdent {
		return false
	}
	used := info.Uses[ident]
	return used != nil && used.Pkg() != nil && used.Pkg().Name() == "changes" && (used.Name() == "NotFound" || used.Name() == "Found")
}

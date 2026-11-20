// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package main

import (
	"go/ast"
	"go/parser"
	"go/token"
	"io/fs"
	"path/filepath"
	"strings"
	"testing"
)

// Every exported name in the backend carries a doc comment: a func or method, a type, and a const or var unless its group has one (redix-api
// #77, the rule .NET's CS1591 kept). sqlc's generated store and the tests are left out.
func TestR77_EveryExportedNameHasADocComment(t *testing.T) {
	root := filepath.Join("..", "..")
	files := token.NewFileSet()
	_ = filepath.WalkDir(root, func(path string, entry fs.DirEntry, err error) error {
		if err != nil || entry.IsDir() || !strings.HasSuffix(path, ".go") || strings.HasSuffix(path, "_test.go") ||
			strings.Contains(filepath.ToSlash(path), "internal/db/store/") {
			return err
		}
		file, err := parser.ParseFile(files, path, nil, parser.ParseComments)
		if err != nil {
			t.Fatal(err)
		}
		for _, name := range undocumented(file) {
			t.Errorf("%s: %s has no doc comment", files.Position(name.Pos()), name.Name)
		}
		return nil
	})
}

// undocumented is every exported name in file with no doc comment of its own or of its group.
func undocumented(file *ast.File) []*ast.Ident {
	var names []*ast.Ident
	for _, declaration := range file.Decls {
		switch d := declaration.(type) {
		case *ast.FuncDecl:
			if d.Name.IsExported() && d.Doc == nil {
				names = append(names, d.Name)
			}
		case *ast.GenDecl:
			for _, spec := range d.Specs {
				switch s := spec.(type) {
				case *ast.TypeSpec:
					if s.Name.IsExported() && d.Doc == nil && s.Doc == nil {
						names = append(names, s.Name)
					}
				case *ast.ValueSpec:
					for _, name := range s.Names {
						if name.IsExported() && d.Doc == nil && s.Doc == nil && s.Comment == nil {
							names = append(names, name)
						}
					}
				}
			}
		}
	}
	return names
}

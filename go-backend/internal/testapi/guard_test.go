package testapi

import (
	"io/fs"
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// foundations are the packages under internal whose own tests drive a piece of the platform directly - a middleware, the router, the net, the
// replay of .NET's answers - rather than asking the API as a domain's tests do.
var foundations = map[string]bool{"platform": true, "db": true, "history": true, "access": true, "testapi": true, "replay": true}

// A domain's test that asks the router itself could leave the History net out, which is the forgetting the net exists to stop: every domain's
// test asks through testapi.
func TestEveryDomainsTestsAskThroughTestapi(t *testing.T) {
	filepath.WalkDir("..", func(path string, entry fs.DirEntry, err error) error {
		if err != nil {
			return err
		}
		if entry.IsDir() && filepath.Dir(path) == ".." && foundations[entry.Name()] {
			return filepath.SkipDir
		}
		if !strings.HasSuffix(path, "_test.go") {
			return nil
		}
		source, _ := os.ReadFile(path)
		for _, direct := range []string{"ServeHTTP(", "httptest.NewRecorder("} {
			if strings.Contains(string(source), direct) {
				t.Errorf("%s asks the router itself (%s): ask through testapi, so the History net cannot be left out", path, direct)
			}
		}
		return nil
	})
}

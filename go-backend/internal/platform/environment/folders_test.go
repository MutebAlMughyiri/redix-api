package environment

import (
	"path/filepath"
	"testing"
)

// The data folder is .NET's LocalApplicationData on each operating system, as the server finds it.
func TestTheDataFolderIsLocalApplicationDataOnEachSystem(t *testing.T) {
	lookup := func(variables map[string]string) func(string) (string, bool) {
		return func(key string) (string, bool) { value, ok := variables[key]; return value, ok }
	}
	for name, want := range map[string]struct {
		goos      string
		variables map[string]string
		folder    string
	}{
		"Windows":                  {"windows", map[string]string{"LOCALAPPDATA": `C:\Users\a\AppData\Local`}, `C:\Users\a\AppData\Local`},
		"Linux with XDG_DATA_HOME": {"linux", map[string]string{"XDG_DATA_HOME": "/data", "HOME": "/home/a"}, "/data"},
		"Linux with HOME only":     {"linux", map[string]string{"HOME": "/home/a"}, filepath.Join("/home/a", ".local", "share")},
	} {
		if got := localData(want.goos, lookup(want.variables)); got != want.folder {
			t.Errorf("%s: %q, not %q", name, got, want.folder)
		}
	}
}

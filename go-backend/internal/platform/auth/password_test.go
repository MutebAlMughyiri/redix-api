package auth

import (
	"encoding/base64"
	"testing"
)

// Hashes of the made-up password "Test-password-1", made by ASP.NET Core 10's own PasswordHasher: with its defaults, and with 10,000 iterations.
const (
	dotNetHash     = "AQAAAAIAAYagAAAAEA48vTz0z9B6QQx+Tl3bhEuQO6+aVAfvxGCTWskO4Gug+7kR+U/wJBbzM052YBjOaw=="
	dotNetWeakHash = "AQAAAAIAACcQAAAAEDB8FIM26Hb8PtCMfrzpwuhaDuhlw9+iFcUazwdNEoGam3klXE5prKtW+Y7tyLBsUw=="
)

func TestR26_APasswordKeptByDotNetStillSignsIn(t *testing.T) {
	if got := Check(dotNetHash, "Test-password-1"); got != Right {
		t.Errorf("right password: %v", got)
	}
	if got := Check(dotNetHash, "test-password-1"); got != Wrong {
		t.Errorf("wrong password: %v", got)
	}
}

func TestR26_APasswordKeptWithFewerIterationsIsRightButHashedAgain(t *testing.T) {
	if got := Check(dotNetWeakHash, "Test-password-1"); got != RightButWeak {
		t.Errorf("weak hash: %v", got)
	}
}

func TestR26_AHashMadeHereHasDotNetsFormatAndChecks(t *testing.T) {
	kept := Hash("Test-password-1")
	header, _ := base64.StdEncoding.DecodeString(kept)
	if len(header) != 61 || header[0] != 1 || header[4] != 2 || header[12] != 16 {
		t.Errorf("format %v", header[:13])
	}
	if Check(kept, "Test-password-1") != Right || Check(kept, "Test-password-2") != Wrong {
		t.Error("a hash made here does not check")
	}
	if Hash("Test-password-1") == kept {
		t.Error("two hashes of one password share a salt")
	}
}

func TestR26_AnythingButAVersion3HashIsWrong(t *testing.T) {
	for _, kept := range []string{"", "not base64!", base64.StdEncoding.EncodeToString([]byte{0, 1, 2}), "AAAAAAIAAYagAAAAEA48vTz0z9B6QQx+Tl3bhEuQO6+aVAfvxGCTWskO4Gug+7kR+U/wJBbzM052YBjOaw=="} {
		if Check(kept, "Test-password-1") != Wrong {
			t.Errorf("%q checked", kept)
		}
	}
}

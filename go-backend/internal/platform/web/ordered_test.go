package web

import "testing"

func TestR4_KeptTextIsTheFormSqlServersJsonColumnGaveBackToDotNet(t *testing.T) {
	typed := Ordered{Keys: []string{"custom.a", "note"}, Values: map[string]string{
		"custom.a": "ال x+y<z>&'w/v\\q\"t",
		"note":     "\u0001\b\f\r\t\u001f\n\u007f ",
	}}
	want := "{\"custom.a\":\"ال x+y<z>&'w/v\\\\q\\\"t\",\"note\":\"\\u0001\\b\\f\\r\\t\\u001F\\n\u007f \"}"
	if got := typed.KeptText(); got != want {
		t.Errorf("kept %q, SQL Server gave back %q", got, want)
	}
}

package spelling

import "testing"

func TestFR43_OneFormOfEachLetterAndNoTashkeel(t *testing.T) {
	for typed, want := range map[string]string{
		"أحمد إبراهيم آل": "احمد ابراهيم ال",
		"مؤسسة الرّياض":   "موسسه الرياض",
		"مستشفى هيئة":     "مستشفي هييه",
		"مَوْرِدٌ":        "مورد",
		"Supplier 12 أ":   "Supplier 12 ا",
		"":                "",
	} {
		if got := Normalize(typed); got != want {
			t.Errorf("Normalize(%q) = %q, want %q", typed, got, want)
		}
	}
}

// Package spelling is what a search treats alike in Arabic (ArabicSpelling, FR-43): أ إ آ with ا, ة with ه, ى with ي, ؤ with و, ئ with ي, and no
// tashkeel. A table keeps a normalized copy of what is searched beside it, filled when it is saved, and a search normalizes what was typed the
// same way.
package spelling

import "strings"

var letters = map[rune]rune{'أ': 'ا', 'إ': 'ا', 'آ': 'ا', 'ة': 'ه', 'ى': 'ي', 'ؤ': 'و', 'ئ': 'ي'}

// The tashkeel, fathatan to sukun.
const (
	firstTashkeel = 'ً'
	lastTashkeel  = 'ْ'
)

// Normalize is text with one form of each letter and no tashkeel; anything else stays as it is.
func Normalize(text string) string {
	return strings.Map(func(letter rune) rune {
		if letter >= firstTashkeel && letter <= lastTashkeel {
			return -1
		}
		if one, found := letters[letter]; found {
			return one
		}
		return letter
	}, text)
}

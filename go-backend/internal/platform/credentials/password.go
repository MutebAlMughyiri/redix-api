// Package credentials owns who a request comes from: passwords and codes kept as ASP.NET Core Identity hashed them, and the access tokens the app sends
// (redix-api #140, N2, N3, R26).
package credentials

import (
	"crypto/pbkdf2"
	"crypto/rand"
	"crypto/sha1"
	"crypto/sha256"
	"crypto/sha512"
	"crypto/subtle"
	"encoding/base64"
	"encoding/binary"
	"hash"
)

// Verdict is what checking a password against its kept hash says.
type Verdict int

const (
	// Wrong: the password is not the one kept.
	Wrong Verdict = iota
	// Right: the password is the one kept.
	Right
	// RightButWeak: the password is right, but kept more weakly than today's hash; hash it again (SuccessRehashNeeded).
	RightButWeak
)

// Identity's version 3 format: a version byte, then the PRF, the iterations and the salt length as big-endian uint32, the salt, the key.
const (
	version3   = 0x01
	sha512PRF  = 2
	iterations = 100_000
	saltLength = 16
	keyLength  = 32
	header     = 13
)

var prfs = map[uint32]func() hash.Hash{0: sha1.New, 1: sha256.New, sha512PRF: sha512.New}

// Hash keeps password as ASP.NET Core Identity's PasswordHasher does by default: version 3, HMAC-SHA512, 100,000 iterations, base64.
func Hash(password string) string {
	salt := make([]byte, saltLength)
	rand.Read(salt)
	key, _ := pbkdf2.Key(sha512.New, password, salt, iterations, keyLength)
	kept := make([]byte, header, header+saltLength+keyLength)
	kept[0] = version3
	binary.BigEndian.PutUint32(kept[1:], sha512PRF)
	binary.BigEndian.PutUint32(kept[5:], iterations)
	binary.BigEndian.PutUint32(kept[9:], saltLength)
	return base64.StdEncoding.EncodeToString(append(append(kept, salt...), key...))
}

// Check says whether password is the one kept in hash, which ASP.NET Core Identity or Hash made.
func Check(kept, password string) Verdict {
	bytes, err := base64.StdEncoding.DecodeString(kept)
	if err != nil || len(bytes) < header || bytes[0] != version3 {
		return Wrong
	}
	prf, count, length := binary.BigEndian.Uint32(bytes[1:]), int(binary.BigEndian.Uint32(bytes[5:])), int(binary.BigEndian.Uint32(bytes[9:]))
	newHash, known := prfs[prf]
	if !known || length < 16 || len(bytes) < header+length+16 {
		return Wrong
	}
	salt, want := bytes[header:header+length], bytes[header+length:]
	got, err := pbkdf2.Key(newHash, password, salt, count, len(want))
	if err != nil || subtle.ConstantTimeCompare(got, want) != 1 {
		return Wrong
	}
	if count < iterations || prf != sha512PRF {
		return RightButWeak
	}
	return Right
}

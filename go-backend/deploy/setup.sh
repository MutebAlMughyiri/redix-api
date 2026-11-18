#!/usr/bin/env bash
# Copyright 2026 Tech Details. All rights reserved.
# Use of this file is governed by the LICENSE file at the root of this repository.

# Prepares an Ubuntu 24.04 server for REDIX, or brings one up to date (redix-api #145). It is the record of every step: on a new server, at any
# company, copy this folder over and run it again. Running it twice changes nothing the second time (R4).
#
# From the PC, in go-backend (README.md, "Deploying"):
#   CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -trimpath -ldflags='-s -w' -o bin/redix-server ./cmd/server
#   scp -i ~/.ssh/redix -r deploy bin/redix-server <redix.env> <backup.env> root@<ip>:/opt/redix-setup/
#   ssh -i ~/.ssh/redix root@<ip> bash /opt/redix-setup/deploy/setup.sh
#
# It installs only what REDIX needs (R15): the API, Typst for the PDFs, Litestream and rclone for the backups, Postfix and OpenDKIM for the email.
# The API holds its own HTTPS certificate (D1), so there is no web server in front of it.
set -euo pipefail

here=$(cd "$(dirname "$0")/.." && pwd)
readonly here
readonly typst_version=0.15.1
readonly typst_sha256=a6d077d0a95eed5a2eba715b2dae06be954f624ccbf85758a03f389ded33118c
readonly litestream_version=0.5.18
readonly litestream_sha256=11d36a2d2058731ee99f201f6b68a52fcb1dd3050bdfd2237f7f39741dafe6ae
restart_api=false

say() { printf '\n== %s\n' "$*"; }
stop() { printf '\nSTOPPED: %s\n' "$*" >&2; exit 1; }

# installs copies $1 to $2 with mode $3 when it differs, and answers whether it did.
installs() {
  if cmp -s "$1" "$2"; then return 1; fi
  install -m "$3" "$1" "$2"
}

say "1. This is a fresh or known Ubuntu 24.04, run as root, with both settings files"
[[ $EUID -eq 0 ]] || stop "run it as root"
. /etc/os-release
[[ $ID == ubuntu && $VERSION_ID == 24.04 ]] || stop "it is made for Ubuntu 24.04, this is $PRETTY_NAME"
for file in redix.env backup.env; do
  [[ -s $here/$file ]] || stop "copy the filled $file next to deploy/ (deploy/prod.env.example and backup.env.example, filled on the PC)"
  sed -i 's/\r$//' "$here/$file"
done
set -a; . "$here/redix.env"; . "$here/backup.env"; set +a
for key in REDIX_Urls REDIX_Jwt__SigningKey REDIX_Prod_Admin__Username REDIX_Prod_Admin__Password REDIX_Domain REDIX_Email__From \
           REDIX_BACKUP_BUCKET REDIX_BACKUP_ENDPOINT AWS_ACCESS_KEY_ID AWS_SECRET_ACCESS_KEY; do
  [[ -n ${!key:-} ]] || stop "$key is empty in redix.env or backup.env"
done
[[ -s $here/redix-server ]] || stop "copy bin/redix-server next to deploy/ (built on the PC, README.md \"Deploying\")"
grep -qE '^(ssh-ed25519|ssh-rsa|ecdsa-)' /root/.ssh/authorized_keys 2>/dev/null \
  || stop "no key in /root/.ssh/authorized_keys: turning passwords off would lock everyone out"

say "2. The clock reads UTC (R14)"
timedatectl set-timezone UTC

say "3. Ubuntu's packages, and its security updates every day (R7, R9)"
debconf-set-selections <<EOF
postfix postfix/main_mailer_type select Internet Site
postfix postfix/mailname string $REDIX_Domain
EOF
export DEBIAN_FRONTEND=noninteractive
apt-get update -q
apt-get install -y -q --no-install-recommends openssh-server ufw unattended-upgrades postfix opendkim opendkim-tools rclone
cat > /etc/apt/apt.conf.d/20auto-upgrades <<'EOF'
APT::Periodic::Update-Package-Lists "1";
APT::Periodic::Unattended-Upgrade "1";
EOF

say "4. SSH takes keys only (R7)"
cat > /etc/ssh/sshd_config.d/10-redix.conf <<'EOF'
# redix-api #145: keys only, never a password.
PasswordAuthentication no
KbdInteractiveAuthentication no
PermitRootLogin prohibit-password
EOF
sshd -t
systemctl try-reload-or-restart ssh

say "5. The firewall lets in SSH and the ports REDIX_Urls names, and closes any other it had opened (R7, R21)"
open="22/tcp"
for url in ${REDIX_Urls//;/ }; do open+=" ${url##*:}/tcp"; done
for rule in $(ufw show added | awk '$1 == "ufw" && $2 == "allow" {print $3}'); do
  [[ " $open " == *" $rule "* ]] || ufw delete allow "$rule"
done
for rule in $open; do ufw allow "$rule"; done
ufw --force enable
https_url=${REDIX_Urls%%;*}

say "6. Typst $typst_version and Litestream $litestream_version, checked against their pinned SHA-256 (R9)"
download=$(mktemp -d)
trap 'rm -rf "$download"' EXIT
if [[ $(typst --version 2>/dev/null) != "typst $typst_version "* ]]; then
  curl -fsSL -o "$download/typst.tar.xz" "https://github.com/typst/typst/releases/download/v$typst_version/typst-x86_64-unknown-linux-musl.tar.xz"
  echo "$typst_sha256  $download/typst.tar.xz" | sha256sum -c -
  tar -xJf "$download/typst.tar.xz" -C "$download" --strip-components=1 typst-x86_64-unknown-linux-musl/typst
  install -m 755 "$download/typst" /usr/local/bin/typst
fi
if [[ $(dpkg-query -W -f='${Version}' litestream 2>/dev/null) != "$litestream_version" ]]; then
  curl -fsSL -o "$download/litestream.deb" "https://github.com/benbjohnson/litestream/releases/download/v$litestream_version/litestream-$litestream_version-linux-x86_64.deb"
  echo "$litestream_sha256  $download/litestream.deb" | sha256sum -c -
  dpkg -i "$download/litestream.deb"
fi

say "7. The redix user, its settings and the API"
id redix >/dev/null 2>&1 || useradd --system --home-dir /var/lib/redix --no-create-home --shell /usr/sbin/nologin redix
install -d -m 700 /etc/redix
install -d -m 750 -o redix -g redix /var/lib/redix /var/lib/redix/attachments
installs "$here/redix.env" /etc/redix/redix.env 600 && restart_api=true
installs "$here/backup.env" /etc/redix/backup.env 600 || true
installs "$here/redix-server" /usr/local/bin/redix-server 755 && restart_api=true

say "8. Email: Postfix sends from this machine alone, OpenDKIM signs it (R12)"
postconf -e "inet_interfaces = loopback-only" "inet_protocols = ipv4" "myhostname = $REDIX_Domain" "mydomain = $REDIX_Domain" 'myorigin = $mydomain' \
  "mydestination = localhost" "smtpd_tls_security_level = none" "smtp_tls_security_level = may" \
  "smtpd_milters = inet:127.0.0.1:8891" 'non_smtpd_milters = $smtpd_milters' "milter_default_action = accept"
keys=/etc/opendkim/keys/$REDIX_Domain
if [[ ! -s $keys/redix.private ]]; then
  install -d -m 700 -o opendkim -g opendkim "$keys"
  opendkim-genkey -b 2048 -d "$REDIX_Domain" -D "$keys" -s redix
  chown opendkim:opendkim "$keys"/redix.*
fi
cat > /etc/opendkim.conf <<EOF
# redix-api #145: signs every message this machine sends for $REDIX_Domain with the selector redix.
Syslog yes
UserID opendkim
UMask 007
Mode s
Domain $REDIX_Domain
Selector redix
KeyFile $keys/redix.private
Socket inet:8891@127.0.0.1
PidFile /run/opendkim/opendkim.pid
EOF
systemctl enable opendkim postfix
systemctl restart opendkim postfix

say "9. The services: the API, Litestream's copy of the database, and the attachments' copy every ten minutes"
install -m 644 "$here/deploy/litestream.yml" /etc/litestream.yml
install -d /etc/systemd/system/litestream.service.d
printf '[Service]\nEnvironmentFile=/etc/redix/backup.env\n' > /etc/systemd/system/litestream.service.d/redix.conf
for unit in redix.service redix-attachments.service redix-attachments.timer; do
  installs "$here/deploy/$unit" "/etc/systemd/system/$unit" 644 && restart_api=true
done
systemctl daemon-reload

say "10. On a new server, the data comes back from the bucket before the API first starts (R8)"
if [[ ! -e /var/lib/redix/redix.db ]]; then
  litestream restore -if-replica-exists -config /etc/litestream.yml /var/lib/redix/redix.db
  rclone copy ":s3,provider=Other,env_auth=true,endpoint='$REDIX_BACKUP_ENDPOINT':$REDIX_BACKUP_BUCKET/attachments" /var/lib/redix/attachments
  chown -R redix:redix /var/lib/redix
fi

systemctl enable --now litestream redix redix-attachments.timer
systemctl restart litestream
if $restart_api; then systemctl restart redix; fi

say "11. What runs now (R10: no request is sent to the API)"
for service in redix litestream redix-attachments.timer postfix opendkim; do
  printf '%-26s %s\n' "$service" "$(systemctl is-active "$service")"
done
echo | timeout 20 openssl s_client -connect "$REDIX_Domain:${https_url##*:}" -servername "$REDIX_Domain" 2>/dev/null \
  | openssl x509 -noout -issuer -enddate 2>/dev/null || echo "HTTPS: no certificate yet - is the domain's A record $REDIX_Domain -> this server?"
if timeout 5 bash -c 'exec 3<>/dev/tcp/gmail-smtp-in.l.google.com/25' 2>/dev/null; then
  echo "Port 25 out: open, mail goes straight to the receiving servers"
else
  echo "Port 25 out: CLOSED by the company - ask them to open it, or name a relay (Q7)"
fi

say "12. The DNS records for the mail (R13), and this server's reverse DNS, set in the company's panel"
cat <<EOF
$(hostname -I | awk '{print $1}')  PTR  $REDIX_Domain.
$REDIX_Domain.         TXT  "v=spf1 a:$REDIX_Domain -all"
_dmarc.$REDIX_Domain.  TXT  "v=DMARC1; p=quarantine"
EOF
cat "$keys/redix.txt"

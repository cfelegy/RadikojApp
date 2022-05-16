#!/bin/sh -eu
exec >>/var/log/acme.log 2>&1
date

stats() {
	cert="/etc/ssl/uacme/$1/cert.pem"
	if ! [ -e "$cert" ]
	then
		return
	fi
	expiration=$(date -d"$(openssl x509 -enddate -noout -in "$cert" \
		| cut -d= -f2)" -D'%b %d %H:%M:%S %Y GMT' +'%s')
	printf '# TYPE certificate_expiration gauge\n'
	printf '# HELP certificate_expiration Timestamp when SSL certificate will expire\n'
	printf 'certificate_expiration{instance="%s"} %s\n' "$1" "$expiration"
}

acme() {
	site=$1
	shift
	/usr/bin/uacme -v -h /usr/share/uacme/uacme.sh issue $site $* || true
	stats $site
}

acme DOMAIN SUBDOMAIN...
doas nginx -s reload

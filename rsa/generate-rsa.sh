scriptdir=$(dirname "$(readlink -f "$0")")
cd ${scriptdir} || exit 1
openssl genrsa -out jwt_private.pem 2048
openssl rsa -in jwt_private.pem -pubout -out jwt_public.pem

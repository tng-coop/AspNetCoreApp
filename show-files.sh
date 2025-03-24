FILES=$(find . -type f| egrep -vi '(./git|./published|node_module|logs|docs|./bin|./out|/obj|/.git|tests|asset|migratio|\.sh$|launchSett|\.txt$|\.md$|\.css$|\.mjs$|\.env$|localhost|Dockerfile|package-lock|package.json|\.ps1$)')
for f in $FILES
do
  echo $f
  echo "----------------"

  cat $f
  echo "----------------"
done
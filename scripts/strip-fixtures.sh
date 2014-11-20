TO_HOST=123-abc-456.mktorest.com
TO_HOST_DIR=$TO_HOST-443
TO_CLIENT_ID=someId
TO_CLIENT_SECRET=someSecret

cd fixtures
mkdir -p $TO_HOST_DIR

# move all recorded files into $HOST directory
DIRS=$(ls -d */ | grep -v ${TO_HOST})

for FROM_HOST in $DIRS; do
  # remove the oauth file
  rm `grep 'identity/oauth/token' $FROM_HOST/* -l`

  FROM_HOST_DIR=$(echo $FROM_HOST | sed 's/\/$//')
  FROM_HOST=$(echo $FROM_HOST | sed 's/-443\/$//')
  for j in $(find $FROM_HOST_DIR -not -path $FROM_HOST_DIR); do
    sed -i "s/$FROM_HOST/$TO_HOST/g" $j
    sed -i "s/$FROM_CLIENT_ID/$TO_CLIENT_ID/g" $j
    sed -i "s/$FROM_CLIENT_SECRET/$TO_CLIENT_SECRET/g" $j
  done
done;

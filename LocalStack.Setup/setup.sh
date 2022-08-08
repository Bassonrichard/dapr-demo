awslocal dynamodb create-table \
--table-name statestore \
--attribute-definitions \
AttributeName=key,AttributeType=S \
--key-schema \
AttributeName=key,KeyType=HASH \
--provisioned-throughput \
ReadCapacityUnits=10,WriteCapacityUnits=5 || true
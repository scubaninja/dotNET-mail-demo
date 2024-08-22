	docker run -d \
    --name=mailpit \
    --rm \
    -p 8025:8025 \
    -p 1025:1025 \
    axllent/mailpit \
&& dotnet watch
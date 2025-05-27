# Build stage
FROM golang:1.20-alpine AS builder

WORKDIR /app
COPY go.mod go.sum ./
RUN go mod download

COPY . .
RUN CGO_ENABLED=0 GOOS=linux go build -o /mailservice

# Final stage
FROM alpine:3.17
RUN apk --no-cache add ca-certificates

WORKDIR /app/
COPY --from=builder /mailservice .
COPY config.yaml .

EXPOSE 8080

CMD ["./mailservice"]

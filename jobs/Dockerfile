FROM golang:latest AS builder
#FROM cgr.dev/chainguard/go AS builder

WORKDIR /app

# install mage via go install (option 1)
RUN go install github.com/magefile/mage@latest

# install mage using go modules (option 2)
#RUN git clone https://github.com/magefile/mage && \
#    cd mage && \
#    go run bootstrap.go

# copy magefile.go, etc, into container
COPY . .

# run mage to install dependencies
RUN mage -compile mage

# we name the binary "mage" so that we can call it the same way
# whether we are running a full golang container, with mage,
# and a magefile.go, or whether we have compiled a static binary

# copy binary into distroless container
# see: https://edu.chainguard.dev/chainguard/chainguard-images/getting-started/getting-started-go/

FROM cgr.dev/chainguard/glibc-dynamic
COPY --from=builder /app/mage /usr/bin/
CMD ["mage", "test:hello"]

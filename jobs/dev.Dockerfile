FROM golang:latest

WORKDIR /app

# our container is already large (~1Gb) so we are instaling
# vim for convenience when debugging
RUN apt-get update && \
    apt-get install -y vim

# install mage via go install (option 1)
RUN go install github.com/magefile/mage@latest

# install mage using go modules (option 2)
#RUN git clone https://github.com/magefile/mage && \
#    cd mage && \
#    go run bootstrap.go

# copy magefile.go, etc, into container
COPY . .

# run mage to install dependencies
RUN mage

# run our chosen "default" mage target, hello
CMD ["mage", "test:hello"]

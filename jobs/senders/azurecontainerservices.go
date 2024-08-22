package senders

import (
	"bytes"
	"crypto/hmac"
	"crypto/sha256"
	"encoding/base64"
	"encoding/json"
	"errors"
	"fmt"
	"io"
	"net/http"
	"net/url"
	"os"
	"time"
)

type AzureContainerServices struct {
	Endpoint string
	Key      string
	From     string
}

// NewAzureContainerServicesFromEnv creates an AzureContainerServices
// client from environment variables ACS_ENDPOINT, ACS_KEY, ACS_FROM
func NewAzureContainerServicesFromEnv() (*AzureContainerServices, error) {
	endpoint := os.Getenv("ACS_ENDPOINT")
	if endpoint == "" {
		return nil, errors.New("ACS_ENDPOINT is not set")
	}
	key := os.Getenv("ACS_KEY")
	if key == "" {
		return nil, errors.New("ACS_KEY is not set")
	}
	from := os.Getenv("ACS_FROM")
	if from == "" {
		return nil, errors.New("ACS_FROM is not set")
	}
	return &AzureContainerServices{
		Endpoint: endpoint,
		Key:      key,
		From:     from,
	}, nil
}

// NewAzureContainerServices creates an AzureContainerServices
// instance with the provided endpoint, key and from email
func NewAzureContainerServices(endpoint, key, from string) (*AzureContainerServices, error) {
	return &AzureContainerServices{
		Endpoint: endpoint,
		Key:      key,
		From:     from,
	}, nil
}

// SendOne sends one test email to <to> via Azure Communications Services
func (a AzureContainerServices) SendOne(to string) error {
	// create email
	subject := "hello"
	body := "world"

	b, err := a.NewEmail(a.From, to, subject, body)
	if err != nil {
		return err
	}
	// log request json
	//fmt.Printf("%s\n", b)

	// see: https://learn.microsoft.com/en-us/rest/api/communication/email/send?tabs=HTTP
	pathAndQuery := "/emails:send?api-version=2023-03-31"
	verb1 := http.MethodPost
	// timestamp in format
	// "Mon, 02 Jan 2006 15:04:05 GMT"
	loc, _ := time.LoadLocation("GMT")
	timestamp1 := time.Now().In(loc).Format(time.RFC1123)
	// hash body
	hash1 := sha256.New()
	hash1.Write(b)
	contenthash1 := base64.StdEncoding.EncodeToString(hash1.Sum(nil))
	// string to sign
	/*
		StringToSign=Verb + "\n"
		URIPathAndQuery + "\n"
		Timestamp + ";" + Host + ";" + ContentHash
	*/
	tmp1, err := url.Parse(a.Endpoint)
	if err != nil {
		return err
	}
	host1 := tmp1.Host
	stringToSign := fmt.Sprintf("%s\n%s\n%s;%s;%s", verb1, pathAndQuery, timestamp1, host1, contenthash1)

	// create signature
	// Signature=Base64(HMAC-SHA256(UTF8(StringToSign), Base64.decode(<your_access_key>)))
	// base64 decode key
	key1, err := base64.StdEncoding.DecodeString(a.Key)
	if err != nil {
		return err
	}
	hash2 := hmac.New(sha256.New, key1)
	hash2.Write([]byte(stringToSign))
	signature1 := base64.StdEncoding.EncodeToString(hash2.Sum(nil))
	authorization1 := fmt.Sprintf("HMAC-SHA256 SignedHeaders=x-ms-date;host;x-ms-content-sha256&Signature=%s", signature1)

	// send request
	url1 := a.Endpoint + pathAndQuery
	req, err := http.NewRequest(http.MethodPost, url1, bytes.NewBuffer(b))
	if err != nil {
		return err
	}
	req.Header.Add("x-ms-date", timestamp1)
	req.Header.Add("x-ms-content-sha256", contenthash1)
	req.Header.Add("host", host1)
	req.Header.Add("Authorization", authorization1)
	req.Header.Add("Content-Type", "text/json")
	res, err := http.DefaultClient.Do(req)
	if err != nil {
		return err
	}
	defer res.Body.Close()

	// read body
	b, err = io.ReadAll(res.Body)
	if err != nil {
		return err
	}

	// log response
	fmt.Printf("%s\n", b)

	return nil
}

// GetResult gets the result of <id> from Azure Communication Services
func (a AzureContainerServices) GetResult(operationID string) error {
	// see: https://learn.microsoft.com/en-us/rest/api/communication/email/get-send-result?tabs=HTTP
	pathAndQuery := fmt.Sprintf("/emails/operations/%s?api-version=2023-03-31", operationID)
	verb1 := http.MethodGet
	// timestamp in format
	// "Mon, 02 Jan 2006 15:04:05 GMT"
	loc, _ := time.LoadLocation("GMT")
	timestamp1 := time.Now().In(loc).Format(time.RFC1123)

	b := []byte("")
	// hash body
	hash1 := sha256.New()
	hash1.Write(b)
	contenthash1 := base64.StdEncoding.EncodeToString(hash1.Sum(nil))
	// string to sign
	/*
		StringToSign=Verb + "\n"
		URIPathAndQuery + "\n"
		Timestamp + ";" + Host + ";" + ContentHash
	*/
	tmp1, err := url.Parse(a.Endpoint)
	if err != nil {
		return err
	}
	host1 := tmp1.Host
	stringToSign := fmt.Sprintf("%s\n%s\n%s;%s;%s", verb1, pathAndQuery, timestamp1, host1, contenthash1)

	// create signature
	// Signature=Base64(HMAC-SHA256(UTF8(StringToSign), Base64.decode(<your_access_key>)))
	// base64 decode key
	key1, err := base64.StdEncoding.DecodeString(a.Key)
	if err != nil {
		return err
	}
	hash2 := hmac.New(sha256.New, key1)
	hash2.Write([]byte(stringToSign))
	signature1 := base64.StdEncoding.EncodeToString(hash2.Sum(nil))
	authorization1 := fmt.Sprintf("HMAC-SHA256 SignedHeaders=x-ms-date;host;x-ms-content-sha256&Signature=%s", signature1)

	// send request
	url1 := a.Endpoint + pathAndQuery
	req, err := http.NewRequest(http.MethodGet, url1, bytes.NewBuffer(b))
	if err != nil {
		return err
	}
	req.Header.Add("x-ms-date", timestamp1)
	req.Header.Add("x-ms-content-sha256", contenthash1)
	req.Header.Add("host", host1)
	req.Header.Add("Authorization", authorization1)
	res, err := http.DefaultClient.Do(req)
	if err != nil {
		return err
	}
	defer res.Body.Close()

	// read body
	b, err = io.ReadAll(res.Body)
	if err != nil {
		return err
	}

	// log response
	fmt.Printf("%s\n", b)

	return nil
}

// NewEmail builds the JSON request body for sending an email
func (a AzureContainerServices) NewEmail(senderAddress, to, subject, body string) ([]byte, error) {
	// see full request schema at:
	// https://learn.microsoft.com/en-us/rest/api/communication/email/send?tabs=HTTP#send-email
	msg1 := struct {
		SenderAddress string `json:"senderAddress"`
		Recipients    struct {
			To []struct {
				Address string `json:"address"`
			} `json:"to"`
		} `json:"recipients"`
		Content struct {
			Subject   string `json:"subject"`
			PlainText string `json:"plainText"`
		} `json:"content"`
	}{
		SenderAddress: senderAddress,
		Recipients: struct {
			To []struct {
				Address string `json:"address"`
			} `json:"to"`
		}{
			To: []struct {
				Address string `json:"address"`
			}{
				{
					Address: to,
				},
			},
		},
		Content: struct {
			Subject   string `json:"subject"`
			PlainText string `json:"plainText"`
		}{
			Subject:   subject,
			PlainText: body,
		},
	}
	b, err := json.Marshal(msg1)
	if err != nil {
		return nil, err
	}
	return b, nil
}

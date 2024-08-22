//go:build mage
// +build mage

package main

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"os"

	"github.com/Azure/azure-sdk-for-go/sdk/azidentity"
	"github.com/Azure/azure-sdk-for-go/sdk/keyvault/azsecrets"
	"github.com/Azure/azure-sdk-for-go/sdk/messaging/azservicebus"
	"github.com/Azure/azure-sdk-for-go/sdk/storage/azblob"
	"github.com/magefile/mage/mg"
	"github.com/magefile/mage/sh"
)

type Azure mg.Namespace

// ContainerRegistry outputs ACCESS_TOKEN and LOGIN_SERVER
// environment variables for AZURE_CONTAINER_REGISTRY_NAME
func (Azure) ContainerRegistry() error {
	registryName := os.Getenv("AZURE_CONTAINER_REGISTRY_NAME")
	if registryName == "" {
		return errors.New("AZURE_CONTAINER_REGISTRY_NAME environment variable not found")
	}

	cmd1 := []string{
		"az",
		"acr",
		"login",
		"--name",
		registryName,
		"--expose-token",
	}

	result, err := sh.Output(cmd1[0], cmd1[1:]...)
	if err != nil {
		return err
	}

	tokenAndServer := struct {
		AccessToken string `json:"accessToken"`
		LoginServer string `json:"loginServer"`
	}{}

	err = json.Unmarshal([]byte(result), &tokenAndServer)
	if err != nil {
		return err
	}

	fmt.Printf("export ACCESS_TOKEN='%s'\n", tokenAndServer.AccessToken)
	fmt.Printf("export LOGIN_SERVER='%s'\n", tokenAndServer.LoginServer)
	fmt.Fprintf(os.Stderr, "docker login --username 00000000-0000-0000-0000-000000000000 --password $ACCESS_TOKEN $LOGIN_SERVER\n")

	return nil
}

// Storage connects to AZURE_STORAGE_ACCOUNT_NAME and lists
// the blogs in AZURE_STORAGE_CONTAINER_NAME
func (Azure) Storage() error {
	storageAccountName := os.Getenv("AZURE_STORAGE_ACCOUNT_NAME")
	if storageAccountName == "" {
		return errors.New("AZURE_STORAGE_ACCOUNT_NAME environment variable not found")
	}
	storageContainerName := os.Getenv("AZURE_STORAGE_CONTAINER_NAME")
	if storageContainerName == "" {
		return errors.New("AZURE_STORAGE_CONTAINER_NAME environment variable not found")
	}

	credential, err := azidentity.NewDefaultAzureCredential(nil)
	if err != nil {
		return err
	}

	url1 := fmt.Sprintf("https://%s.blob.core.windows.net/", storageAccountName)
	client, err := azblob.NewClient(url1, credential, nil)
	if err != nil {
		return err
	}

	pager := client.NewListBlobsFlatPager(storageContainerName,
		&azblob.ListBlobsFlatOptions{
			Include: azblob.ListBlobsInclude{
				Snapshots: true,
				Versions:  true,
			},
		})

	ctx := context.Background()
	for pager.More() {
		resp, err := pager.NextPage(ctx)
		if err != nil {
			return err
		}
		for _, blob := range resp.Segment.BlobItems {
			url2 := fmt.Sprintf("https://%s.blob.core.windows.net/%s/%s", storageAccountName, storageContainerName, *blob.Name)
			fmt.Println(url2)
		}
	}
	return nil
}

// KeyVault connects to AZURE_KEY_VAULT_URL and
// lists the secrets
func (Azure) KeyVault() error {
	vaultURL := os.Getenv("AZURE_KEY_VAULT_URL")
	if vaultURL == "" {
		return errors.New("AZURE_KEY_VAULT_URL environment variable not found")
	}

	cred, err := azidentity.NewDefaultAzureCredential(nil)
	if err != nil {
		return err
	}

	client, err := azsecrets.NewClient(vaultURL, cred, nil)
	if err != nil {
		return err
	}

	ctx := context.Background()
	pager := client.NewListSecretsPager(nil)
	for pager.More() {
		page, err := pager.NextPage(ctx)
		if err != nil {
			return err
		}
		for _, secret := range page.Value {
			fmt.Printf("%s\n", *secret.ID)
		}
	}

	return nil
}

// ServiceBus connects to AZURE_SERVICEBUS_HOSTNAME and peeks
// 5 messages in AZURE_SERVICEBUS_HOSTNAME
func (Azure) ServiceBus() error {
	namespace := os.Getenv("AZURE_SERVICEBUS_HOSTNAME")
	if namespace == "" {
		return errors.New("AZURE_SERVICEBUS_HOSTNAME environment variable not found")
	}

	queueName := os.Getenv("AZURE_SERVICEBUS_QUEUE_NAME")
	if queueName == "" {
		return errors.New("AZURE_SERVICEBUS_QUEUE_NAME environment variable not found")
	}

	cred, err := azidentity.NewDefaultAzureCredential(nil)
	if err != nil {
		return err
	}

	client, err := azservicebus.NewClient(namespace, cred, nil)
	if err != nil {
		return err
	}

	receiver, err := client.NewReceiverForQueue(queueName, nil)
	if err != nil {
		return err
	}

	ctx := context.Background()
	messages, err := receiver.PeekMessages(ctx, 5, nil)
	if err != nil {
		return err
	}
	for _, x := range messages {
		fmt.Printf("%s\n", x.Body)
	}

	return nil
}

// Postgres outputs the environment variables to connect
// to the Postgres database in the provided <resource group>
func (Azure) Postgres(resourceGroup string) error {
	pgDatabase := os.Getenv("PGDATABASE")
	if pgDatabase == "" {
		pgDatabase = "postgres"
	}

	cmd := []string{
		"az",
		"account",
		"show",
		"--query",
		"user.name",
		"--out",
		"tsv",
	}
	username, err := sh.Output(cmd[0], cmd[1:]...)
	if err != nil {
		return err
	}

	cmd = []string{
		"az",
		"postgres",
		"flexible-server",
		"list",
		"--resource-group",
		resourceGroup,
		"--query",
		"[0].fullyQualifiedDomainName",
		"--out",
		"tsv",
	}
	host, err := sh.Output(cmd[0], cmd[1:]...)
	if err != nil {
		return err
	}

	token, err := getToken()
	if err != nil {
		return err
	}

	fmt.Printf("export PGHOST=%s\n", host)
	fmt.Printf("export PGDATABASE=%s\n", pgDatabase)
	fmt.Printf("export PGUSER=%s\n", username)
	fmt.Printf("export PGPASSWORD='%s'\n", token)
	return nil
}

func getToken() (string, error) {
	cmd := []string{
		"az",
		"account",
		"get-access-token",
		"--resource=https://ossrdbms-aad.database.windows.net",
		"--query",
		"accessToken",
		"--output",
		"tsv",
	}
	return sh.Output(cmd[0], cmd[1:]...)
}

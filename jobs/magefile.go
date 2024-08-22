//go:build mage
// +build mage

package main

import (
	"errors"
	"fmt"
	"jobs/senders"
	"os"
	"time"

	"github.com/magefile/mage/mg"
	"github.com/magefile/mage/sh"
)

type Test mg.Namespace

// Hello is our default mage target which we also call
// by default within our Docker container
func (Test) Hello() error {
	now := time.Now().UTC().Format(time.RFC3339)
	fmt.Printf("hello %s\n", now)
	return nil
}

// Goodbye is an alternative mage target we can call
func (Test) Goodbye() error {
	now := time.Now().UTC().Format(time.RFC3339)
	fmt.Printf("goodbye %s\n", now)
	return nil
}

type Email mg.Namespace

// SendOne sends one test email to <to> via Azure Communications Services
func (Email) SendOne(to string) error {
	s, err := senders.NewAzureContainerServicesFromEnv()
	if err != nil {
		return err
	}
	return s.SendOne(to)
}

// GetResult gets the result of <id> from Azure Communication Services
func (Email) GetResult(operationID string) error {
	s, err := senders.NewAzureContainerServicesFromEnv()
	if err != nil {
		return err
	}
	return s.GetResult(operationID)
}

type Docker mg.Namespace

// Build builds the container image, "jobs", with --no-cache
// and Dockerfile which builds a static binary and
// multi-stage builds to utilize a distroless image
func (Docker) Build() error {
	cmd1 := []string{
		"docker",
		"build",
		"--no-cache",
		"-t",
		"jobs",
		".",
	}
	return sh.RunV(cmd1[0], cmd1[1:]...)
}

// BuildDev builds the container image, "jobs", with --no-cache
// and dev.Dockerfile which uses the golang:latest image,
// installs mage and vim, for more interactive development
func (Docker) BuildDev() error {
	cmd1 := []string{
		"docker",
		"build",
		"--no-cache",
		"-f",
		"dev.Dockerfile",
		"-t",
		"jobs",
		".",
	}
	return sh.RunV(cmd1[0], cmd1[1:]...)
}

// Run runs the jobs container with the mage target
func (Docker) Run(target string) error {
	cmd1 := []string{
		"docker",
		"run",
		"-it",
		"jobs",
		"mage",
		target,
	}
	return sh.RunV(cmd1[0], cmd1[1:]...)
}

type Deploy mg.Namespace

// ContainerApps deploys the Container App(s) via containerapp.bicep
// into the provided <resource group>
// Requires: AZURE_SERVICEBUS_CONNECTION_STRING
func (Deploy) ContainerApps(resourceGroup string) error {
	serviceBusConnection := os.Getenv("AZURE_SERVICEBUS_CONNECTION_STRING")
	if serviceBusConnection == "" {
		return errors.New("AZURE_SERVICEBUS_CONNECTION_STRING environment variable not found")
	}
	cmd1 := []string{
		"az",
		"deployment",
		"group",
		"create",
		"--resource-group",
		resourceGroup,
		"--template-file",
		"deploy/azure-container-apps/containerapp.bicep",
		"--parameters",
		"service_bus_connection=" + serviceBusConnection,
	}
	return sh.RunV(cmd1[0], cmd1[1:]...)
}

// ContainerApps deploys the Container App(s) via containerapp.bicep
// into the provided <resource group>
// Requires: AZURE_SERVICEBUS_CONNECTION_STRING

// Storage deploys 5 "Storage" services via main.bicep into the
// provided <resource group>. The services are:
// Container Registry, Blob Storage, Service Bus, Key Vault and Postgres
func (Deploy) Storage(resourceGroup string) error {
	cmd1 := []string{
		"az",
		"deployment",
		"group",
		"create",
		"--resource-group",
		resourceGroup,
		"--template-file",
		"deploy/azure-data/main.bicep",
		"--parameters",
		"deployPostgres=false",
	}
	return sh.RunV(cmd1[0], cmd1[1:]...)
}

// StorageAndPostgres deploys Storage and Azure Database for Postgres
func (Deploy) StorageAndPostgres(resourceGroup string) error {
	cmd1 := []string{
		"az",
		"deployment",
		"group",
		"create",
		"--resource-group",
		resourceGroup,
		"--template-file",
		"deploy/azure-data/main.bicep",
		"--parameters",
		"deployPostgres=true",
	}
	return sh.RunV(cmd1[0], cmd1[1:]...)
}

// RBAC deploys Role Based Access Control using rbac.bicep
// with the principalID of the currently signed in user
func (Deploy) RBAC(resourceGroup string) error {
	cmd1 := []string{
		"az",
		"ad",
		"signed-in-user",
		"show",
		"--query",
		"id",
		"--out",
		"tsv",
	}
	principalID, err := sh.Output(cmd1[0], cmd1[1:]...)
	if err != nil {
		return err
	}

	cmd2 := []string{
		"az",
		"deployment",
		"group",
		"create",
		"--resource-group",
		resourceGroup,
		"--template-file",
		"deploy/azure-data/rbac.bicep",
		"--parameters",
		"principalID=" + principalID,
	}
	return sh.RunV(cmd2[0], cmd2[1:]...)
}

// Empty empties the <resource group> via empty.bicep
func (Deploy) Empty(resourceGroup string) error {
	fmt.Printf("Emptying Resource Group (%s) in 10 seconds.\n", resourceGroup)
	time.Sleep(time.Second * 10)

	cmd1 := []string{
		"az",
		"deployment",
		"group",
		"create",
		"--resource-group",
		resourceGroup,
		"--mode",
		"Complete",
		"--template-file",
		"deploy/azure-container-apps/empty.bicep",
	}
	return sh.RunV(cmd1[0], cmd1[1:]...)
}

// Group creates the <resource group> in <location>
func (Deploy) Group(name, location string) error {
	cmd1 := []string{
		"az",
		"group",
		"create",
		"--name",
		name,
		"--location",
		location,
	}
	return sh.RunV(cmd1[0], cmd1[1:]...)
}

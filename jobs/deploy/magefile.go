//go:build mage
// +build mage

package main

import (
	"fmt"

	"github.com/magefile/mage/mg"
	"github.com/magefile/mage/sh"
)

type Deploy mg.Namespace

// Test deployment to <name>
func (Deploy) Test(name string) error {
	fmt.Printf("Testing deployment to: %s\n", name)
	return nil
}

// Empty empties the <resource group> via empty.bicep
func (Deploy) Empty(resourceGroup string) error {
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

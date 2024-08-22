#!/usr/bin/env node
require("dotenv").config();
const { program } = require("commander");
const path = require("path");
const fs = require("fs");
const consola = require("consola");
const mailDirs = require("../lib/dirs");

let description = `
🤙🏽 Welcome to Tailwind Mail! Your mail list service, powered by markdown and a simple CLI.
`;

program.version("0.0.1").description(description);

program.command("init")
  .description("Setup the directories you'll need to work with MDMail")
  .action(async function(){
    const {confirm} = require("@inquirer/prompts");
    const dirsExist = mailDirs.dirsExist(__dirname);
    if(dirsExist){
      consola.error("Can't initialize, a /mail directory exists already.")
      return;
    }
    const doIt = await confirm({
      message: "About to create a /mail directory for you. Proceed? (Y|n)",
      default: true
    });
    if(doIt) mailDirs.makeDirs(__dirname);
  })

program.command("broadcast", "Setup a broadcast to send to your people");
program.command("contact", "Find a person and manage their status");
program.parse(process.argv);

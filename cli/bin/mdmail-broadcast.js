const axios = require('axios');
const { program } = require('commander');
const fs = require("fs");
const path = require("path");
const consola = require("consola");
const highlight = require("cli-highlight").highlight;
const {confirm, select, input} = require("@inquirer/prompts");
const apiUrl = process.env.API_ROOT || "http://localhost:5000/admin";;
const mailDirs = require("../lib/dirs");
const _ = require('lodash');
const Spinner = require('cli-spinner').Spinner;
const parser = require("../lib/parser");

const pingAI = async function(prompt){
  const url = `${apiUrl}/get-chat`;
  consola.info("OK, here we go... be patient...\n");
  var Spinner = require('cli-spinner').Spinner;
  var spin = new Spinner("talking to api... %s");
  spin.setSpinnerString(29);
  spin.start();
  var res = await axios.post(url, {
    prompt,
  });
  spin.stop();
  //console.log(res.data);
  return res.data.reply;
}

const getBroadcastFile = async function(){

  const ready = mailDirs.dirsExist(__dirname);
  if(!ready){
    consola.error("Please be sure you have a /mail directory and a /mail/broadcasts directory. Run init to set it up.")
    return;
  }

  const docs = getBroadcasts();
  let file;
  if(docs.length === 0){
    consola.error("There are no broadcasts to regenerate. Run broadcast new to generate one.")
    return;
  }
  if(docs.length > 1){
    file = await select({
      message: "Which broadcast?",
      choices: docs
    });
    return file.name
  }else{
    return docs[0].name;
  }
}

program.command("new")
  .description("Create a broadcast markdown file")
  .argument("<subject>", "The subject of the email to send. This will also create a slug, which you can change.")
  .option("-p", "Add a prompt for AI generation.")
  .action(async function(subject){
    const ready = mailDirs.dirsExist(__dirname);
    if(!ready){
      consola.error("Please be sure you have a /mail directory and a /mail/broadcasts directory. Run init to set it up.")
      return;
    }
    const slug = _.kebabCase(subject);

    var useAI = await confirm({
      message: "Want to set this up with an OpenAI prompt? We can generate the body of the email for you, which you should then edit. (Y|n)",
      default: true
    });
    var body = `This is the markdown body of the email, which you should change. If you want to send on a particular date, add that using 
  "SendAt" in the frontmatter. The "SendToTag" is set to everyone, but you can restrict based on one or more tag slugs, separated by a comma.`
    if(useAI){
      var prompt = await input({
        message: "Enter your prompt. It can take up to 20 seconds to reply, so be patient:",
        default: "\"What color is a moose in spring?\""
      });
      if(prompt){
        body = await pingAI(prompt);
      }
    }

    const addPrompt = prompt ? `\nPrompt: ${prompt}` : "";
    const template = `---
Subject: "${subject}"
Slug: "${slug}"
Summary: "Summarize the email here for the preview"
SendToTag: "*"${addPrompt}
---

${body}
`
    const docPath = path.resolve(__dirname, "./mail/broadcasts",`${slug}.md`);
    // if(!fs.existsSync(docPath)){
      fs.writeFileSync(docPath, template);
      consola.success(`\nYour broadcast file is ready to go, and is located at /mail/broadcasts/${slug}.md\n\n`);
      console.log(body);
    // }else{
    //   consola.error("This file exists already. Please delete or move by hand.")
    // }

  });

const getBroadcasts = function(){
  const dirPath = path.resolve(__dirname, "./mail/broadcasts");
  return fs.readdirSync(dirPath). map(f => {
    return {
      name: f,
      value: f
    }
  });
}

program.command("generate")
  .description("Rebuild your email using a prompt")
  .action(async function(){
    let file = await getBroadcastFile();
    const fileName = file.indexOf(".md") > 0 ? file : file+".md";
    const docPath = path.resolve(__dirname, "./mail/broadcasts",fileName);
    const {data, markdown} = parser.build(docPath);
    if(!data.Prompt){
      consola.error("Make sure there's a Prompt in the frontmatter.")
      return;
    }
    const body = await pingAI(data.Prompt);

    const template = `---
Subject: "${data.Subject}"
Slug: "${data.Slug}"
Summary: "${data.Summary}"
SendToTag: "*"
Prompt: "${data.Prompt}"
---

${body}
`
    // if(!fs.existsSync(docPath)){
    fs.writeFileSync(docPath, template);
    consola.success(`\n\nOK, regenerated!\n\n`);
    console.log(body);

  })

program.command("validate")
  .description("Validates the markdown doc against the API")
  //.argument("<file>", "The name of the markdown doc")
  .action(async function(){
    let file = await getBroadcastFile();

    const url = `${apiUrl}/validate` //using dotnet run for the server
    var fileName = file.indexOf(".md") > 0 ? file : file+".md";
    const docPath = path.resolve(__dirname, "./mail/broadcasts",fileName);
    const markdown = fs.readFileSync(docPath,"utf-8");
    const {data} = await axios.post(url,{markdown});
    if(data.valid){
      console.log("\n\n");
      consola.success("Subject is present");
      consola.success("Summary is present");
      consola.success("There's a markdown body");
      if(data.contacts > 0){
        consola.success(`Sending to ${data.contacts} contacts\n\n`);
        consola.info("Here's the data that was parsed\n")
        console.log(data.data.data)
      }else{
        consola.error(`There are no contacts to send to. The Sending Tag is ${data.data.data.SendToTag}.`)
      }

    }else{
      consola.error(new Error(data.message));
    }
  });
program.command("send")
  .description("Queues the broadcast for send, depending on the delay. Default is now.")
  //.argument("<file>", "The name of the markdown doc")
  .action(async function(){
    let file = await getBroadcastFile();
    //hard-coding this for now
    let url = `${apiUrl}/validate` //using dotnet run for the server
    var fileName = file.indexOf(".md") > 0 ? file : file+".md";
    const docPath = path.resolve(__dirname, "./mail/broadcasts",fileName);
    const markdown = fs.readFileSync(docPath,"utf-8");

    let {data} = await axios.post(url,{markdown});
    const shouldSend = await confirm({
      message: `This is going to send this broadcast to ${data.contacts} contacts. Proceed? (y | N)`,
      default: false
    });

    if(shouldSend){
      
      url = `${apiUrl}/queue-broadcast`
      try{
        const {data} =  await axios.post(url,{markdown});
        console.log(data);
        console.log("\n\n");
        if(data.success){
          consola.success(`${data.message}\n\n`);
          //move into sent
          const newPath = path.resolve(__dirname, "./mail/sent",fileName);
          fs.renameSync(docPath, newPath);
        }else{
          consola.error(new Error(data.message));
        }
      }catch(err){
        consola.error("Looks like a server error happened. This is typically caused by reusing a message slug");
        console.log(err.message);
      }

      
    }
  });

program.parse(process.argv);
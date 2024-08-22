const { program } = require('commander');
const consola = require("consola");
const axios = require("axios");
const fs = require("fs");
const path = require("path");
const {input, confirm} = require("@inquirer/prompts");

const apiUrl = process.env.API_ROOT || "http://localhost:5000/admin";

program.command("tag")
  .description("Tag a group of contacts. Requires a CSV of email addresses")
  .argument("<file>", "The .csv file name")
  .action(async function(file){
    const fileName = file.indexOf(".csv") > 0 ? file : file + ".csv";
    const csvPath = path.resolve(__dirname, "../csvs", `${fileName}`)
    const text = fs.readFileSync(csvPath, "utf-8");
    const emails = text.replace("\"", "").split("\n").filter(e => e.indexOf("@") > 0);
    //we need to validate this somehow
    if(emails.length > 0){
      let confirmed = await confirm({message: `There are ${emails.length} contacts in your list. Proceed  (Y | n)?`, default: true});
      if(!confirmed) return;
      let tag = await input({message: `What tag should be applied? You can separate with a comma too.`});
      if(!tag){
        consola.error("Need a tag or this won't work, kid.");
        return;
      }
      confirmed = await confirm({message: `Applying ${tag} to ${emails.length} contacts. Proceed  (y | N)?`, default: true});
      if(confirmed){
        url = `${apiUrl}/bulk/contacts/tag`
        try{
          const {data} =  await axios.post(url,{emails, tag});
          console.log(data);
          console.log("\n\n");
          if(data.success){
            consola.success(`${data.message}\n\n`);
          }else{
            consola.error(new Error(data.message));
          }
        }catch(err){
          consola.error("Looks like a server error happened... check the output");
          console.log(err.message);
        }
      }
    }else{
      consola.error("There's no list here")
    }
  });

program.command("search")
  .description("Searches your contacts using a fuzzy match on email or name")
  .argument("<term>", "Your search term")
  .action(async function(term){
    if(term.length < 3){
      consola.error("Term should be 3 or more characters.")
    }
    url = `${apiUrl}/contacts/search`
    try{
      const {data} =  await axios.get(url,{
        params: {
          term
        }
      });
      console.log(data);
      console.log("\n\n");
    }catch(err){
      console.log(err);
      consola.error("Looks like a server error happened.");
      console.log(err.message);
    }
  });

program.parse(process.argv);
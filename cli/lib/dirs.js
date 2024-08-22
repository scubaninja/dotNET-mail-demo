const path = require("path");
const fs = require("fs");


const getDirs = function(root){
  const out = [];
  const mailDir = path.resolve(root,"mail");
  out.push(mailDir);
  out.push(path.resolve(mailDir,"broadcasts"));
  out.push(path.resolve(mailDir,"sequences"));
  out.push(path.resolve(mailDir,"sent"));
  return out;
}
exports.dirsExist = function(root){
  const dirs = getDirs(root);
  for(let d of dirs){
    if(!fs.existsSync(d)) return false;
  }
  return true;
}

exports.makeDirs = function(root){
  const dirs = getDirs(root);
  for(let d of dirs){
    if(!fs.existsSync(d)) fs.mkdirSync(d);
  }
  const readme = `# Welcome! This Is Your Mail Directory...

This is where the magic happens. MDMail is powered by markdown, and where you put what is important! Please don't change these directories or rename them, otherwise things will break.

Going to add more text to this, including instructions, as we go along here.`
    const readMeFile = path.resolve(root, "mail/README.md");
    fs.writeFileSync(readMeFile,readme);

}
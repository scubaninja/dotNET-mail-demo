require("dotenv").config();
const fs = require("fs");
const path = require("path");
const gm = require("gray-matter");
const marked = require('marked');

exports.build = filePath => {
  let pageContents = fs.readFileSync(filePath,"utf8");
  let formatted = gm(pageContents);

  const fileName = path.basename(filePath).replace(".md","");
  const splits = fileName.split("_");
  const data = formatted.data;
  data.slug = data.slug || splits[1]
  let body = marked.parse(formatted.content);
  return {data: data, html: body, markdown: formatted.content};
}
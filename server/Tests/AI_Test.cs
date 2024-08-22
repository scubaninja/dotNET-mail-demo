// using Xunit;
// using Tailwind.Data;
// using Tailwind.Mail.Commands;
// using Markdig;
// using Markdig.Syntax;
// using Tailwind.Mail.Models;
// using Dapper;
// using Microsoft.AspNetCore.Mvc;
// using Tailwind.Mail.Api.Admin;
// using Tailwind.AI;

// namespace Tailwind.Mail.Tests;

// //probably end up mocking this at some point
// //for now, just use the test db


// [Collection("AI Testing")]
// public class AITest:TestBase
// {
//   public AITest()
//   {
//   }
//   [Fact]
//   public async void A_simple_api_ping()
//   {
//     var chat = new Chat();
//     var res = await chat.Prompt("Create a 4-paragraph essay on the topic of 'How much gold is still missing from the Spanish sunken fleet'.");
//     Console.WriteLine(res);
//   }
// }
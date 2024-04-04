// See https://aka.ms/new-console-template for more information
using CielaDocs.Shared.ExpressionEngine;


var testString = "NInvPreCase / NIReal / TMonths";

var pieces=testString.Split(' ');


var t = new Tokenizer(new StringReader(testString));
List<string> vars = new List<string>();
while (t.Token != Token.EOF)
{
    if(vars.IndexOf(t.Identifier)<0)
    { vars.Add(t.Identifier); }

    t.NextToken();
}
foreach (var v in vars) { 
    Console.WriteLine(v);
}

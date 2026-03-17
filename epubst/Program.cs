using epubst.Commands;

var root = RootCommandBuilder.Build();
return await root.Parse(args).InvokeAsync();

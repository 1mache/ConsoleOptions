# ConsoleOptions
An easy way to parse console arguments.

First you need to create an config class, this class will serve as a set of istructions to the Parser.  
In the config class you can implement whatever functionality you want, the only thing Parser will care about is methods marked by the [Command] attribute and properties marked by the [Param] attribute. ! The marked property/method obviously need to be PUBLIC, the Parser looks for public methods/properties so it can interact with them!

### <ins>The Param attribute</ins>
The param attribute lets you mark a property as a required or optional param.  
The Param attribute takes 3 arguments: a name (this is not related to the name of the property), a bool defining whether the param is optional (true - optional, false - mandatory) and a param description for the help screen. Note that for now the description is only used by the optional params so you can do anything as a second argument for required ones.  
-***Example***: [Param(false, "foo")], [Param(true, "A path to the target file")]  
If a param is required then it should be passed in console immediately after the cli command. Those are params that are critical to an app's
functionality.  
A property marked with Param attribute must be: public, string, non-static, have a public set. If you expect something other than a string you can create a private field of the desired type and cast to this type in the setter of the property.  
**Critical:** Because of how Parser works, in your Options class all the properties marked as required params must come before those marked as optional params.

### <ins>The Command attribute</ins>
The command attribute lets you mark a method that will be executed by the Parser if it sees a specified command.  
The Command attribute takes 2 arguments: a string which the parser will look out for(this should start with a "-") and a string description of what the command does for help screen.  
-***Example***: [Command("-h", "Prints Hello")], [Command("-gm", "Prints Good Morning")]  
A method marked with Command attribute must be: public, non-static, void, no arguments. If you want some sort of argument for example say hello with a name, you can create a property for name and mark it as a param and then refer to it inside the method.  
Marked methods can come anywhere in options class even before the params. 

### <ins>Parser</ins>
Parser is a generic class whose type T is the type of you options class. It's constructor takes 2 arguments: a T instance of your options class and a string which is the clicommand that your app will have (for help screen).  
The Parser.Parse method will take the args[] that the Main method got and look for the params and commands that you defined. It will automatically create a help screen with instructions and descriptions, and how it when the app is called with --help as only argument. Important note: right now the parser calls the marked method as soon as it sees the command in args so if your method is using an optional param and the command for it was passed before the param itself then you are using an undefined property in the method. Maybe Ill fix it some day.

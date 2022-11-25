# ConsoleOptions
An easy way to parse console arguments.

First you need to create an config class, this class will serve as a set of istructions to the Parser.  
In the config class you can implement whatever functionality you want, the only thing Parser will care about is methods marked by the [Command] attribute and properties marked by the [Param] attribute. ! The marked property/method obviously need to be PUBLIC, the Parser looks for public methods/properties so it can interact with them!

### <ins>The Param attribute</ins>
The param attribute lets you mark a property as a required or optional param.  
The Param attribute takes 3 arguments: a name (this is not related to the name of the property), a bool defining whether the param is optional (true - optional, false - mandatory) and a param description for the help screen.   
-***Example***: [Param("foo_string",false, "string that says foo")], [Param("src_path",true, "A path to the target file")]  
If a param is mandatory (optional=false) the parser will raise an exception whenever it doesn't find it.
A property marked with Param attribute must be: public, string, non-static, have a public set. If you expect something other than a string you can cast to the desired type afterwards. 

### <ins>The Command attribute</ins>
The command attribute lets you mark a method that will be executed by the Parser if it sees a specified option(starts with a -).  
The Command attribute takes 2 arguments: the commands name - a string which the parser will look out for and a string description of what the command does for help screen. The command name is WHAT COMES AFTER THE DASH. So if you want the method to be called by -a option the name argument should be a and not -a, if the command name will be -a then the Parser will look for --a.
-***Example***: [Command(,"-h", "Prints Hello")], [Command("-gm", "Prints Good Morning")]  
A method marked with Command attribute must be: public, non-static, void, no arguments. If you want some sort of argument for example say hello with a name, you can create a property for name and mark it as a param and then refer to it inside the method. 
Parser will run all the methods that he recognized the commands of at the very end of parsing so it will first assign all the params to their properties.   

### <ins>Parser</ins>
Parser is a generic class whose type T is the type of your config class. It has 2 constructors: first constructor takes 2 arguments:  a string which is the clicommand that your tool will have (for help screen) and a T instance of your config class. This will create you a help screen (manual for your tool) based on the contents of the config file and show it whenever the tool is run with -help option. The second constructor will take the T instance of your config class first and a string helpScreen (your own manual if you dont like the procedural one) as second argument.
The Parser.Parse method will take the args[] that the Main method got and look for options that you defined.
Parser will run all the methods that he recognized the commands of at the very end of parsing so it will first assign all the params to their properties.

This currently does not support option chaining and long options. It treats long options and short one letter ones as the same as chaining is not supporteds.
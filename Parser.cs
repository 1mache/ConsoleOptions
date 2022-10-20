using System.Reflection;
class Parser<T>
{
    private T _options;
    private Type _type;
    private string _commandName;
    
    public Parser(T optionsClass, string commandName)
    {
        if(optionsClass is null || commandName is null) 
            throw new ArgumentNullException();

        _options = optionsClass;
        _type = typeof(T);
        _commandName = commandName;
    }

    public T? Parse(string[] cmdArgs) 
    {
        var props = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var startId = 0;

        foreach (var prop in props)
        {
            if(prop.GetSetMethod() is null)
                throw new Exception($"No public set method for property: {prop.Name}, cannot be a console parameter!");
            
            //gets the param attribute of property 
            var param = prop.GetCustomAttribute<ParamAttribute>();
            if(param is not null)
            {
                if(param.Optional)
                {
                    try
                    {
                        prop.SetValue(_options, cmdArgs[startId]);    
                    }
                    catch (System.IndexOutOfRangeException)
                    {
                        ShowHelp();
                        return default(T);
                    } 
                }
            }
        }

        return _options;
    }

    public void ShowHelp()
    {
        var props = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        //final text
        string helpText = "==============(--help)==============";

        //a one line instruction of how to use the command :
        // commandName <param1> <param2>
        string useInstruction = $"\n*Use: {_commandName} ";

        //optional parameters explanations
        string optionalParamsText = "\n*Optional Parameters:";

        foreach (var prop in props)
        {
            var param = prop.GetCustomAttribute<ParamAttribute>();
            
            if (param is not null)
            {
                if(param.Optional)
                {
                    //if useInstructions are not commplete 
                    //and an optional parameter is met then it means no
                    //mandatory parameters will be met so the use instruction is done.
                    //Add it to the final text
                    if(useInstruction[useInstruction.Length-1] != '.')
                    {
                        useInstruction.Append('.');
                        helpText += useInstruction;
                    }

                    optionalParamsText += $"\n<{prop.Name}>   -   {param.Description}";
                }
                //if parameter is mandatory add it to instruction
                else
                {
                    //if useInstruction is complete means optional param was met
                    //this is invalid condition, mandatory parameters should come
                    //first in order of option class properties
                    if(useInstruction[useInstruction.Length-1] == '.') 
                        throw new Exception($"In options class ({_type}) mandatory params should come before optional params!");

                    useInstruction += $"<{prop.Name}> ";
                }
            }
        }

        helpText += optionalParamsText;
        //execution option instructions will be added here 

        System.Console.WriteLine(helpText);
    }
}
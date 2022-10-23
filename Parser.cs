using System.Reflection;
class Parser<T>
{
    private T _options;
    private Type _type;
    private string _commandName;
    
    public Parser(T optionsInstance, string commandName)
    {
        if(optionsInstance is null || commandName is null) 
            throw new ArgumentNullException();

        _options = optionsInstance;
        _type = typeof(T);
        _commandName = commandName;

        Validate();
    }


    public T? Parse(string[] cmdArgs) 
    {
        if(cmdArgs.Length == 1 && cmdArgs[0].Equals("-help"))
        {
            ShowHelp();
            return default(T);
        }
        
        var props = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        int requiredCount = 0;

        foreach (var prop in props)
        {
            var param = prop.GetCustomAttribute<ParamAttribute>();

            if (param is not null)
            {
                if(param.Optional)
                    break;
                else
                {
                    try
                    {
                        prop.SetValue(_options, cmdArgs[requiredCount]);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new Exception("Not all required params were passed, use '--help' for info");
                    }
                    requiredCount ++;
                }
            }
        }

        for(int i = requiredCount; i < cmdArgs.Length; i++)
        {
            var split = cmdArgs[i].Split('=');
            //format is ParamName=ParamValue
            if(split.Length == 2)
            {
                string name = split[0], value = split[1];
                bool found = false;
                for (int j = requiredCount; j < props.Length; j++)
                {
                    if(props[j].Name.Equals(name))
                    {
                        props[j].SetValue(_options,value);
                        found = true;
                        break;
                    }
                }

                if(!found)
                    throw new Exception($"Unknown optional param {name}, use --help.");
            }
            else
                throw new Exception($"Unknown format: {cmdArgs[i]}, use --help");
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
                    optionalParamsText += $"\n<{prop.Name}>   -   {param.Description}";
                else
                    useInstruction += $"<{prop.Name}> ";
            }
        }
        useInstruction += "<OptionalParamName>=Value ...";
        helpText += useInstruction;
        helpText += optionalParamsText;
        //execution option instructions will be added here 

        System.Console.WriteLine(helpText);
    }
    
    //after we validate the options class in the constructor all the methods
    //can safely assume that mandatory parameters come before optional ones.
    private void Validate()
    {
        var props = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var methods = _type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        //ignores the property methods for getting/ setting
        methods = methods.Where(m => !m.IsSpecialName).ToArray();


        bool requirementsMet = false;

        if(props.Length == 0) System.Console.WriteLine("WARNING: Pointless Parser. Options class doesnt have public properties");

        foreach (var prop in props)
        {
            var param = prop.GetCustomAttribute<ParamAttribute>();

            if(param is not null)
            {
                if(prop.PropertyType != typeof(string))
                    throw new Exception($"Console params should all be of type string, while {prop.Name} is {prop.PropertyType}!");

                if(prop.GetSetMethod() is null)
                    throw new Exception($"No public set method for property: {prop.Name}, cannot be a console parameter!");

                if(param.Optional)
                {
                    if(!requirementsMet)
                        requirementsMet = true;
                }
                else
                {
                    //if requirementsMet is true it means optional param was encountered
                    //this is invalid condition, mandatory parameters should come
                    //first in order of option class properties
                    if(requirementsMet)
                    {
                        throw new Exception($"In options class ({_type}) mandatory params should come before optional params!");
                    }
                }
            }

            var errorAttribute = prop.GetCustomAttribute<CommandAttribute>();
            if(errorAttribute is not null) 
                throw new Exception($"You cannot assign a command attribute to properties ({prop.Name})! Methods only");
        }

        foreach (var method in methods)
        {
            var command = method.GetCustomAttribute<CommandAttribute>();
            if(command is not null)
            {
                if(method.GetParameters().Length > 0)
                    throw new Exception($"Methods that are called by cmd commands cannot have parameters! {method.Name} has parameters!");
            }

            var errorAttribute = method.GetCustomAttribute<ParamAttribute>();
            if(errorAttribute is not null) 
                throw new Exception($"You cannot assign a param attribute to methods ({method.Name})! Properties only");
        }
    }
}
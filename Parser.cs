using System.Reflection;

class Parser<T>
{
    private T _options;
    private Type _type;
    private string _CLICommandName;

    private bool _hasOptionalParams = false, _hasCommands = false;
    
    public Parser(T optionsInstance, string CLICommandName)
    {
        if(optionsInstance is null || CLICommandName is null) 
            throw new ArgumentNullException();

        _options = optionsInstance;
        _type = typeof(T);
        _CLICommandName = CLICommandName;

        Validate();
    }


    public void Parse(string[] cmdArgs) 
    {
        if(cmdArgs.Length == 1 && cmdArgs[0].Equals("-help"))
        {
            ShowHelp();
            return;
        }
        
        var props = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        MethodInfo[] methods = new MethodInfo[]{};
        if(_hasCommands)
        {
            methods = _type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            methods = methods.Where(m => !m.IsSpecialName).ToArray();
        }
        
        //console args that were already encountered by the parser
        //to avoid duplicating commands/param evaluation
        var encounteredArgs = new Dictionary<string, bool>();

        int requiredCount = 0;

        SetRequiredParams(props, cmdArgs, ref requiredCount);

        for(int i = requiredCount; i < cmdArgs.Length; i++)
        {
            if(cmdArgs[i][0].Equals('-'))
            {
                //if the options class doesnt have commands then the GetMethod method will
                //simply recieve an empty array and return false
                if(Encountered(encounteredArgs, cmdArgs[i]))
                    throw new InvalidArgumentException($"Command duplication! {cmdArgs[i]}");
                
                var method = GetMethod(methods, cmdArgs[i]);
                //invoke method with no parameters
                method.Invoke(_options, new object?[]{});
                encounteredArgs.Add(cmdArgs[i], true);
            }
            else
            {
                if(!_hasOptionalParams)
                    throw new InvalidArgumentException($"Unknown param {cmdArgs[i]}, options do not include opional params");

                //format is ParamName=ParamValue
                var split = cmdArgs[i].Split('=');
                if(split.Length == 2)
                {
                    string name = split[0], value = split[1];
                    if(Encountered(encounteredArgs, name))
                        throw new InvalidArgumentException($"Got param '{name}' twice.");

                    var prop = GetOptionalProperty(props, requiredCount, name);
                    prop.SetValue(_options, value);
                    encounteredArgs.Add(name, true);
                }
                else
                    throw new FormatException($"Unknown format: {cmdArgs[i]}, use --help");

            }
        }
    }

    //sets the required params from the values in cmdArgs, and stores how many are there in a ref counter
    private void SetRequiredParams(PropertyInfo[] props, string[] cmdArgs, ref int count )
    {
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
                        prop.SetValue(_options, cmdArgs[count]);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new RequiredParamsException("Not all required params were passed, use '--help' for info");
                    }
                    count++;
                }
            }
        }
    }

    //returns a bool that represents whether or not a cmd argument was already encountered
    private bool Encountered(Dictionary<string, bool> dict, string key)
    {
        bool b = false;
        dict.TryGetValue(key, out b);

        return b;
    }
    
    //gets method that corresponds to the passed command if there is one
    private MethodInfo GetMethod(MethodInfo[] methods, string command)
    {
        foreach(var method in methods)
        {
            var commandAttr = method.GetCustomAttribute<CommandAttribute>();

            if(commandAttr is not null)
            {
                if(commandAttr.Command.Equals(command))
                {
                    return method;
                }
            }
        }
        throw new InvalidArgumentException($"Unknown command {command}. Use --help");
    }

    //gets a property that corresponds to a name if there is one
    private PropertyInfo GetOptionalProperty(PropertyInfo[] props, int requiredCount, string paramName)
    {
        for (int j = requiredCount; j < props.Length; j++)
        {
            if(props[j].Name.Equals(paramName))
            {
                return props[j];
            }
        }
        throw new InvalidArgumentException($"Unknown param name {paramName}! Use --help");
    }

    private void ShowHelp()
    {
        var props = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        //final text
        string helpText = "==============(--help)==============\n";

        //a one line instruction of how to use the command :
        // commandName <param1> <param2>
        string useInstruction = $"\n*Use: {_CLICommandName} ";

        //optional parameters explanations
        string optionalParamsText = "\n*Optional Parameters:";
        // command explanations
        string commandText = "\n*Optional Commands:";

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
        useInstruction += "<OptionalParamName>=Value ...\n";
        helpText += useInstruction;
        if(_hasOptionalParams)
            helpText += optionalParamsText + "\n";

        if (_hasCommands)
        {
            var methods = _type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            //ignores the property methods for getting/ setting
            methods = methods.Where(m => !m.IsSpecialName).ToArray();
            
            foreach (var method in methods)
            {
                var command = method.GetCustomAttribute<CommandAttribute>();
    
                if(command is not null)
                {
                    commandText += $"\n<{command.Command}>   -   {command.Description}";
                }
            }
            helpText += commandText;
        }

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
                    throw new InvalidAttributeException($"Console params should all be of type string, while {prop.Name} is {prop.PropertyType}!");

                if(prop.GetSetMethod() is null)
                    throw new InvalidAttributeException($"No public set method for property: {prop.Name}, cannot be a console parameter!");

                if(param.Optional)
                {
                    if(!requirementsMet)
                        requirementsMet = true;
                    _hasOptionalParams = true;
                }
                else
                {
                    //if requirementsMet is true it means optional param was encountered
                    //this is invalid condition, mandatory parameters should come
                    //first in order of option class properties
                    if(requirementsMet)
                    {
                        throw new RequiredParamsException($"In options class ({_type}) mandatory params should come before optional params!");
                    }
                }
            }

            var errorAttribute = prop.GetCustomAttribute<CommandAttribute>();
            if(errorAttribute is not null) 
                throw new InvalidAttributeException($"You cannot assign a command attribute to properties ({prop.Name})! Methods only");
        }

        foreach (var method in methods)
        {
            var command = method.GetCustomAttribute<CommandAttribute>();
            if(command is not null)
            {
                if(method.GetParameters().Length > 0)
                    throw new InvalidAttributeException($"Methods that are called by cmd commands cannot have parameters! {method.Name} has parameters!");
                _hasCommands = true;
            }

            var errorAttribute = method.GetCustomAttribute<ParamAttribute>();
            if(errorAttribute is not null) 
                throw new InvalidAttributeException($"You cannot assign a param attribute to methods ({method.Name})! Properties only");
        }
    }
}
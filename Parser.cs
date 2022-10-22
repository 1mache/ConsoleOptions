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
        var props = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        //since parameters should be passed in order,
        //this will show which param are we currently looking at.
        int paramId = 0;

        //will be set to true once all the required parameters got passed.
        bool requiredParams = false;

        if(cmdArgs[0].Equals("--help"))
        {
            ShowHelp();
            return default(T);
        }

        foreach (var prop in props)
        {
            if(prop.GetSetMethod() is null)
                throw new Exception($"No public set method for property: {prop.Name}, cannot be a console parameter!");
            
            //gets the param attribute of property. 
            var param = prop.GetCustomAttribute<ParamAttribute>();
            if(param is not null)
            {
                if(param.Optional)
                {
                    if(!requiredParams)
                        requiredParams = true;
                    
                    try
                    {
                        prop.SetValue(_options, cmdArgs[paramId]);    
                    }
                    catch (System.IndexOutOfRangeException)
                    {
                        // cmdArgs is out of range and required parameters were passed which means
                        return _options;
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
                    optionalParamsText += $"\n<{prop.Name}>   -   {param.Description}";
                else
                    useInstruction += $"<{prop.Name}> ";
            }
        }
        
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

        bool requirementsMet = false;

        if(props.Length == 0) System.Console.WriteLine("WARNING: Pointless Parser. Options class doesnt have public properties");

        foreach (var prop in props)
        {
            var param = prop.GetCustomAttribute<ParamAttribute>();

            if(param is not null)
            {
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
        }
    }
}
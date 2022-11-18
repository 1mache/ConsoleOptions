
namespace ConsoleOptions
{
    using System.Reflection;
    public class Parser<T>
    {
        private T _config;
        private ConfigInfo<T> _configInfo;
        private string _commandName;

        private bool _hasOptionalParams = false, _hasCommands = false;
        
        public Parser(T configInstance, string commandName)
        {
            if(configInstance is null || commandName is null) 
                throw new ArgumentNullException();

            _config = configInstance;
            _commandName = commandName;

            _configInfo = new ConfigInfo<T>(_config);
        }


        public void Parse(string[] cmdArgs) 
        {
            if(cmdArgs.Length == 1 && cmdArgs[0].Equals("--help"))
            {
                ShowHelp();
                return;
            }
            
            var props = _configInfo.ConfigType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            MethodInfo[] methods = new MethodInfo[]{};
            if(_hasCommands)
            {
                methods = _configInfo.ConfigType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
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
                    if(encounteredArgs.ContainsKey(cmdArgs[i]))
                        throw new InvalidArgumentException($"Command duplication! {cmdArgs[i]}");
                    
                    var method = GetMethod(methods, cmdArgs[i]);
                    //invoke method with no parameters
                    method.Invoke(_config, new object?[]{});
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
                        if(encounteredArgs.ContainsKey(name))
                            throw new InvalidArgumentException($"Got param '{name}' twice.");

                        var prop = GetOptionalProperty(props, requiredCount, name);
                        prop.SetValue(_config, value);
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
                            prop.SetValue(_config, cmdArgs[count]);
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
            var props = _configInfo.ConfigType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            //final text
            string helpText = "==============(--help)==============\n";

            //a one line instruction of how to use the command :
            // commandName <param1> <param2>
            string useInstruction = $"\n*Use: {_commandName} ";

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
            if(_hasOptionalParams)
                useInstruction += "<OptionalParamName>=Value ...\n";
            helpText += useInstruction;
            if(_hasOptionalParams)
                helpText += optionalParamsText + "\n";

            if (_hasCommands)
            {
                var methods = _configInfo.ConfigType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
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
    }
}
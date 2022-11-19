
namespace ConsoleOptions
{
    using System.Reflection;
    public class Parser<T>
    {
        private T _configObject;
        private ConfigInfo<T> _configInfo;
        private string _commandName;

        private Action? _optionalActions;
        
        public Parser(T configObject, string commandName)
        {
            if(configObject is null || commandName is null) 
                throw new ArgumentNullException();

            _configObject = configObject;
            _commandName = commandName;

            _configInfo = new ConfigInfo<T>(_configObject);
        }


        public void Parse(string[] cmdArgs) 
        {
            if(cmdArgs.Contains<string>("-help"))
            {
                ShowHelp();
                return;
            }

            if(cmdArgs.Length < _configInfo.RequiredQueue.Count)
                throw new RequiredParamsException("Didn't get enough params. Use -help");

            var requiredQueue = _configInfo.RequiredQueue;
            var optionsTable = _configInfo.OptionsTable;
            foreach (var arg in cmdArgs)
            {
                if(arg[0] == '-')
                {
                    //cuts out the - symbol
                    string noDashArg = arg.Substring(1);
                    //if its an option then it should be in optionTable
                    if(optionsTable.ContainsKey(noDashArg))
                    {
                        if(optionsTable[noDashArg] is MethodInfo)
                        {
                            var methodInfo = (MethodInfo)optionsTable[noDashArg];
                            _optionalActions += (Action)Delegate.CreateDelegate(typeof(Action), _configObject, methodInfo); 
                            continue;
                        }
                        else    
                            throw new OptionException($"Unknown Option {arg}");
                    }

                    //if not an option it might be an optional param so we split by = sumbol
                    //and check
                    var split = noDashArg.Split('=');

                    if(split.Length == 2)
                    {
                        var name = split[0];
                        var value = split[1];
                        if(value.Length == 0)
                            throw new OptionException($"No value for optional param {name}");

                        MemberInfo? memberInfo; 
                        optionsTable.TryGetValue(name, out memberInfo);

                        if(memberInfo is PropertyInfo)
                        {
                            ((PropertyInfo)memberInfo).SetValue(_configObject, value);
                        }
                        else
                            throw new OptionException($"Unknown Optional Param {name}. Use -help");
                    }
                    else
                        throw new OptionException($"Unknown Option {arg}. Use -help");
                }
                //if there is no dash symbol then it must be a regular argument
                else
                {
                    if(requiredQueue.Count > 0)
                        requiredQueue.Dequeue().SetValue(_configObject,arg);
                    else
                        throw new InvalidArgumentException($"Unknown argument {arg}");
                }
            }

            //if we went through the arguments and didnt find all of the required ones
            if(requiredQueue.Count > 0)
                throw new RequiredParamsException($"Missing some key arguments like: {requiredQueue.Dequeue().GetCustomAttribute<ParamAttribute>()!.Name}");

            _optionalActions?.Invoke();
        }

        private void ShowHelp()
        {
            // var props = _configInfo.ConfigType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            // //final text
            // string helpText = "==============(--help)==============\n";

            // //a one line instruction of how to use the command :
            // // commandName <param1> <param2>
            // string useInstruction = $"\n*Use: {_commandName} ";

            // //optional parameters explanations
            // string optionalParamsText = "\n*Optional Parameters:";
            // // command explanations
            // string commandText = "\n*Optional Commands:";

            // foreach (var prop in props)
            // {
            //     var param = prop.GetCustomAttribute<ParamAttribute>();
                
            //     if (param is not null)
            //     {
            //         if(param.Optional)
            //             optionalParamsText += $"\n<{prop.Name}>   -   {param.Description}";
            //         else
            //             useInstruction += $"<{prop.Name}> ";
            //     }
            // }
            // if(_hasOptionalParams)
            //     useInstruction += "<OptionalParamName>=Value ...\n";
            // helpText += useInstruction;
            // if(_hasOptionalParams)
            //     helpText += optionalParamsText + "\n";

            // if (_hasCommands)
            // {
            //     var methods = _configInfo.ConfigType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            //     //ignores the property methods for getting/ setting
            //     methods = methods.Where(m => !m.IsSpecialName).ToArray();
                
            //     foreach (var method in methods)
            //     {
            //         var command = method.GetCustomAttribute<CommandAttribute>();
        
            //         if(command is not null)
            //         {
            //             commandText += $"\n<{command.Name}>   -   {command.Description}";
            //         }
            //     }
            //     helpText += commandText;
            // }

            System.Console.WriteLine("Help");
        }
    }
}
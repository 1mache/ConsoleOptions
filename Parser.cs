
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


        public bool Parse(string[] cmdArgs) 
        {
            if(cmdArgs.Contains<string>("-help"))
            {
                ShowHelp();
                return false;
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
            return true;
        }

        private void ShowHelp()
        {
            var props = _configInfo.ConfigType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            //final text
            string helpText = "==============(-help)==============\n";

            var requiredQueue = _configInfo.RequiredQueue;
            var optionsTable = _configInfo.OptionsTable;

            //a one line instruction of how to use the command :
            // commandName <param1> <param2>
            string useInstruction = $"\n*Usage: {_commandName}";
            
            string paramGuide = "";
            if(requiredQueue.Count > 0)
                paramGuide += "\n\n*****Required Parameters*****:";
            //options explanations
            string optionsGuide = "";
            if(optionsTable.Count>0)
            {
                optionsGuide += "\n\n*****Options*****:";
                useInstruction+= " [OPTIONS]";
            }

            while(requiredQueue.Count>0)
            {
                var param = requiredQueue.Dequeue().GetCustomAttribute<ParamAttribute>();
                useInstruction += $" <{param!.Name}>";
                paramGuide += $"\n{param.Name}            -             {param.Description}";
            }

            foreach (var option in optionsTable)
            {
                var(name, info) = option;
                if(info is PropertyInfo)
                {
                    var param = ((PropertyInfo)info).GetCustomAttribute<ParamAttribute>();
                    optionsGuide += $"\n-{param!.Name}=VALUE             -             {param.Description}";
                }
                else
                {
                    var command = ((MethodInfo)info).GetCustomAttribute<CommandAttribute>();
                    optionsGuide += $"\n'-{command!.Name}'             -             {command.Description}";
                }
            }

            helpText+= useInstruction;
            helpText+= paramGuide;
            helpText+= optionsGuide;

            System.Console.WriteLine(helpText);
        }
    }
}